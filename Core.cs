using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nysc.API.Data;
using Nysc.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nysc.API
{
    public class Core
    {
        #region Properties

        #region Solids

        #region JWT Claim Identifiers
        public const string JWT_CLAIM_ID_ROL = "rol";
        public const string JWT_CLAIM_ID_ID = "id";
        public const string JWT_CLAIM_ID_VERIFIED = "ver";
        #endregion

        #region JWT Claims
        public const string JWT_CLAIM_API_ACCESS = "api_access";
        // public const string
        #endregion

        #endregion

        #region Internals
        public static Core Instance { get; private set; }
        IConfiguration Configuration { get; set; }
        UserManager<User> UserManager { get; }
        UserDataContext UserDataContext { get; }
        
        #endregion

        #endregion

        #region Constructors
        public Core(IConfiguration configuration, UserManager<User> userManager, UserDataContext userContext)
        {
            Configuration = configuration;
            UserManager = userManager;
            UserDataContext = userContext;
        }
        #endregion

        #region Methods
        

        public async void SeedUsers()
        {
            string data = File.ReadAllText("Data/UserSeedData.json");
            var users = JsonConvert.DeserializeObject<List<User>>(data, new IsoDateTimeConverter() { DateTimeFormat = "d/MM/yyyy" });

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];
                user.UserName = user.FileNo.ToString();
                await UserManager.AddPasswordAsync(user, user.PhoneNumber.ToString());
                await UserManager.CreateAsync(user);
            }
        }
        #endregion
    }
}
