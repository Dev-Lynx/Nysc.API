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

            await UserDataContext.Entry(user).Collection(u => u.Photos).LoadAsync();
            return new OkObjectResult(Mapper.Map<UserProfileViewModel>(user));
        }

        [HttpPost("updateUser")]
        public async Task<IActionResult> UpdateUser(UserProfileViewModel model)
        {
            string id = User.FindFirst("id").Value;
            User user = await UserManager.FindByIdAsync(id);

            if (user == null || !user.PhoneNumberConfirmed) return Unauthorized();
            if (user.FileNo != model.FileNo) return Unauthorized();

            user = Mapper.Map(model, user);
            UserDataContext.Update(user);
            await UserDataContext.SaveChangesAsync();
            return Ok();
        }
        #endregion                  
    }
}
