﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nysc.API.Data;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;

namespace Nysc.API
{
    public class Startup
    {
        #region Properties
        
        IConfiguration Configuration { get; }
        SymmetricSecurityKey SecretKey { get; set; }
        #endregion

        #region Constructors
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            SecretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("Authentication:Key").Value));
        }
        #endregion

        #region Methods
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddCors();
            services.AddAutoMapper();
            services.AddDbContext<UserDataContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));


            ConfigureAuthentication(services);
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IResourceRepository, ResourceRepository>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IJwtFactory, JwtFactory>();
            services.AddScoped<UserManager<User>, UserManager<User>>();
            services.AddScoped<Core>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Core core)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // app.UseHttpsRedirection();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();
            app.UseMvc();

            app.UseSpaStaticFiles();
            app.UseSpa(spa => { });
            

            // core.SeedUsers();
        }

        public IServiceCollection ConfigureAuthentication(IServiceCollection services)
        {
            JwtIssuerOptions jwtIssuerOptions = Configuration.GetSection(nameof(JwtIssuerOptions))
                .Get<JwtIssuerOptions>();
            jwtIssuerOptions.SigningCredentials = new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha512Signature);

            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Audience = jwtIssuerOptions.Audience;
                options.Issuer = jwtIssuerOptions.Issuer;
                options.SigningCredentials = jwtIssuerOptions.SigningCredentials;
                options.Subject = jwtIssuerOptions.Subject;
            });
            //services.AddScoped<JwtIssuerOptions>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtIssuerOptions.Issuer;
                configureOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuerOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtIssuerOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = SecretKey,

                    RequireExpirationTime = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                configureOptions.SaveToken = true;
            });


            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy => policy.RequireClaim(Core.JWT_CLAIM_ID_ROL, Core.JWT_CLAIM_API_ACCESS));
            });

            services.AddIdentityCore<User>(u =>
            {
                u.Password.RequireDigit = false;
                u.Password.RequireLowercase = false;
                u.Password.RequireUppercase = false;
                u.Password.RequireNonAlphanumeric = false;
                u.Password.RequiredLength = 8;
                u.User.RequireUniqueEmail = false;
            }).AddEntityFrameworkStores<UserDataContext>().AddDefaultTokenProviders();

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "wwwroot";
            });

            return services;
        }
        #endregion
    }
}
