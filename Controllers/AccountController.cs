using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;
using Nysc.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nysc.API.Data;
using Nysc.API.Models.Entities;
using Nysc.API.Models.UserActivities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Nysc.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        #region Properties

        #region Internals
        UserManager<User> UserManager { get; }
        UserDataContext UserDataContext { get; }
        IResourceRepository Resources { get; }
        IMapper Mapper { get; }
        #endregion

        #endregion

        #region Constructors
        public AccountController(UserManager<User> userManager, UserDataContext userDataContext, IMapper mapper)
        {
            UserManager = userManager;
            UserDataContext = userDataContext;
            Mapper = mapper;
        }
        #endregion

        #region Methods
        [HttpGet("user")]
        public async Task<IActionResult> GetUser()
        {
            string id = User.FindFirst("id").Value;
            User user = await UserManager.FindByIdAsync(id);

            if (user == null) return Unauthorized();

            if (!user.PhoneNumberConfirmed) return new OkObjectResult(Mapper.Map<UserLoginViewModel>(user));

            EntityEntry<User> entry = UserDataContext.Entry(user);
            
            await entry.Collection(u => u.Photos).LoadAsync();
            await entry.Collection(u => u.Activities).LoadAsync();
            return new OkObjectResult(Mapper.Map<UserProfileViewModel>(user));
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateUser(UserProfileViewModel model)
        {
            string id = User.FindFirst("id").Value;
            User user = await UserManager.FindByIdAsync(id);

            if (user == null || !user.PhoneNumberConfirmed) return Unauthorized();
            if (user.FileNo != model.FileNo) return Unauthorized();

            user = Mapper.Map(model, user);

            user.Activities.Add(new AccountActivity() { ActivityType = Models.Entities.Activity.AccountUpdated });
            UserDataContext.Update(user);
            await UserDataContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody]PasswordChangeViewModel model)
        {
            string id = User.FindFirst("id").Value;
            User user = await UserManager.FindByIdAsync(id);

            IdentityResult result = await UserManager.ChangePasswordAsync(user, 
                model.CurrentPassword, model.NewPassword);

            if (result.Succeeded) return Ok();
            return BadRequest();
        }
        #endregion                  
    }
}
