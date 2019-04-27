using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;
using Nysc.API.ViewModels;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Properties

        #region Internals
        JwtSecurityTokenHandler TokenHandler { get; } = new JwtSecurityTokenHandler();
        IAuthRepository Auth { get; }
        ISmsService SMS { get; }
        IJwtFactory JwtFactory { get; }
        #endregion

        #endregion


        #region Constructors
        public AuthController(IAuthRepository authRepository, IJwtFactory jwtFactory, ISmsService sms)
        {
            Auth = authRepository;
            JwtFactory = jwtFactory;
            SMS = sms;
        }
        #endregion

        #region Methods

        #region RestFul Conventions
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginViewModel model)
        {
            string token = await Auth.Login(model);
            if (token == null) return Unauthorized();
            return Ok(new { access_token = token });
        }

        [Authorize]
        [HttpPost("phone-verification")]
        public async Task<IActionResult> GenerateOTP()
        {
            string id = User.FindFirst("id").Value;

            var user = await Auth.GetUserById(id);
            if (user == null) return Unauthorized();

            if (user.PhoneNumberConfirmed) return BadRequest("The user is already verified");
            if (await Auth.GenerateOTP(user)) return Ok();

            return BadRequest("An error occured while generating a one time password");
        }

        [Authorize]
        [HttpPost("verify/{code}")]
        public async Task<IActionResult> VerifyOTP(string code)
        {
            string id = User.FindFirst("id").Value;

            var user = await Auth.GetUserById(id);
            if (user == null) return Unauthorized();

            string token = string.Empty;

            if (user.PhoneNumberConfirmed)
            {
                token = await JwtFactory.GenerateToken(user);
                return Ok(new { access_token = token });
            }
                

            token = await Auth.ValidateOTP(user, code);
            if (!string.IsNullOrWhiteSpace(token))
                return Ok(new { access_token = token });

            return BadRequest("Validation Failed");
        }
        #endregion


        #endregion
    }
}