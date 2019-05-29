using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Nysc.API.Data
{
    public class JwtFactory : IJwtFactory
    {
        #region Properties

        #region Internals
        public JwtIssuerOptions JwtOptions { get; }
        UserManager<User> UserManager { get; }
        IAuthRepository Auth { get; }
        #endregion

        #endregion

        #region Constructors
        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions, UserManager<User> userManager)
        {
            JwtOptions = jwtOptions.Value;
            UserManager = userManager;
        }
        #endregion

        #region Methods

        #region IJwtFactory Implementation
        public async Task<string> GenerateToken(User user)
        {
            string role = (await UserManager.GetRolesAsync(user)).FirstOrDefault();

            var identity = new ClaimsIdentity(new GenericIdentity(user.Id, "Token"), new[]
            {
                new Claim(Core.JWT_CLAIM_ID, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, await JwtOptions.JtiGenerator()),
                new Claim(Core.JWT_CLAIM_ROL, role),
                new Claim(Core.JWT_CLAIM_VERIFIED, user.PhoneNumberConfirmed.ToString())
            });

            /*
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, await JwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(JwtOptions.IssuedAt).ToString()),
                identity.FindFirst(Core.JWT_CLAIM_ID_ROL),
                identity.FindFirst(Core.JWT_CLAIM_ID_ID),
                identity.FindFirst(Core.JWT_CLAIM_ID_VERIFIED),
            };

            var jwt = new JwtSecurityToken(
                issuer: JwtOptions.Issuer,
                audience: JwtOptions.Audience,
                claims: claims,
                notBefore: JwtOptions.NotBefore,
                expires: JwtOptions.Expiration,
                signingCredentials: JwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
            */


            return new JwtSecurityTokenHandler().CreateEncodedJwt(
                JwtOptions.Issuer, JwtOptions.Audience, identity,
                JwtOptions.NotBefore, JwtOptions.Expiration, JwtOptions.IssuedAt,
                JwtOptions.SigningCredentials);
        }


        #endregion

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);
        #endregion
    }
}
