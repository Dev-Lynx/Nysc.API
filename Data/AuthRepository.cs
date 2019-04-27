using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;
using Nysc.API.Models.Entities;
using Nysc.API.ViewModels;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nysc.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        #region Properties

        #region Internals
        UserManager<User> UserManager { get; }
        UserDataContext UserDataContext { get; }
        IJwtFactory JwtFactory { get; }
        IConfiguration Configuration { get; }
        ISmsService SMS { get; }
        Hotp Generator { get; }
        #endregion

        #endregion

        #region Constructors
        public AuthRepository(UserManager<User> userManager, UserDataContext userDataContext, IConfiguration configuration, IJwtFactory jwtFactory, ISmsService sms)
        {
            UserManager = userManager;
            UserDataContext = userDataContext;
            JwtFactory = jwtFactory;
            Configuration = configuration;
            Generator = new Hotp(Encoding.ASCII.GetBytes(Configuration.GetSection("Authentication:Key").Value));
            SMS = sms;
        }
        #endregion

        #region Methods

        #region IAuthRepository Implemenation
        public async Task<string> Login(UserLoginViewModel model)
        {
            var user = await UserManager.Users.FirstOrDefaultAsync(u => u.FileNo == model.FileNo);
            if (user == null) return null;

            if (!(await UserManager.CheckPasswordAsync(user, model.Password))) return null;
            return await JwtFactory.GenerateToken(user);
        }

        public async Task<User> GetUserById(string id) => await UserManager.FindByIdAsync(id);

        public async Task<bool> UserExists(int fileNo) => await UserManager.Users.FirstOrDefaultAsync(u => u.FileNo == fileNo) != null;

        public async Task<bool> GenerateOTP(User user)
        {
            await UserDataContext.Entry(user).Reference(u => u.OneTimePassword).LoadAsync();

            if (user.OneTimePassword != null && !user.OneTimePassword.IsExpired)
                return true;

            await ClearOneTimePasswords(user);

            OneTimePassword password = new OneTimePassword(user);
            await UserDataContext.OneTimePasswords.AddAsync(password);
            await UserDataContext.SaveChangesAsync();

            password.Code = Generator.ComputeHOTP(password.Id);
            UserDataContext.Update(password);
            await UserDataContext.SaveChangesAsync();

            string message = $"Your NYSC One Time Password is {password.Code}."
                + " Keep this password should be kept private and should not be shared with anyone.";
            await SMS.SendMessage(user.FormattedPhoneNumber, message);
            return true;
        }

        public async Task<string> ValidateOTP(User user, string code)
        {
            await UserDataContext.Entry(user).Reference(u => u.OneTimePassword).LoadAsync();

            if (user.OneTimePassword.IsExpired || !user.OneTimePassword.Verify(code))
            {
                await ClearOneTimePasswords(user);
                return null;
            }

            user.PhoneNumberConfirmed = true;
            await ClearOneTimePasswords(user);


            UserDataContext.Update(user);
            await UserDataContext.SaveChangesAsync();
            return await JwtFactory.GenerateToken(user);
        }

        async Task ClearOneTimePasswords(User user)
        {
            // Gather and remove all old passwords by the user
            List<OneTimePassword> oldPasswords = await UserDataContext
                .OneTimePasswords.Where(p => p.User.Id == user.Id).ToListAsync();

            UserDataContext.OneTimePasswords.RemoveRange(oldPasswords);
        }
        #endregion

        #endregion
    }
}
