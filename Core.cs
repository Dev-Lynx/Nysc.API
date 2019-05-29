using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using NLog.Config;
using NLog.Targets;
using Nysc.API.Data;
using Nysc.API.Data.Indexing;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;
using Nysc.API.Models.UserActivities;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nysc.API
{
    public static class Core
    {
        #region Properties

        public static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        public static PhoneNumberUtil Phone { get; } = PhoneNumberUtil.GetInstance();
        public static string[] StartupArguments { get; set; } = new string[0];

        #region Solids

        #region JWT Claim Identifiers
        public const string JWT_CLAIM_ID = "id";
        public const string JWT_CLAIM_ROL = "rol";
        public const string JWT_CLAIM_VERIFIED = "ver";
        #endregion

        #region JWT Claims
        public const string JWT_CLAIM_API_ACCESS = "api_access";
        #endregion

        #region Directories
        public static readonly string BASE_DIR = Directory.GetCurrentDirectory();
        public static readonly string WORK_DIR = Path.Combine(BASE_DIR, "ApplicationBase");
        public static readonly string DATA_DIR = Path.Combine(WORK_DIR, "Data");
        public static readonly string INDEX_DIR = Path.Combine(WORK_DIR, "Indexes");
        public readonly static string LOG_DIR = Path.Combine(WORK_DIR, "Logs");
        #endregion

        #region Paths
        public static readonly string IDENTITY_DATABASE_PATH = Path.Combine(DATA_DIR, "Identity.mdf");
        public static readonly string ERROR_LOG_PATH = Path.Combine(LOG_DIR, ERROR_LOG_NAME + ".log");
        #endregion

        static string PRODUCT_NAME = "NYSC API";
        static string AUTHOR = "Prince Owen";

        public static LuceneVersion LuceneVersion { get; } = LuceneVersion.LUCENE_48;

        #region Names
        public const string CONSOLE_LOG_NAME = "console-debugger";
        public const string LOG_LAYOUT = "${longdate}|${uppercase:${level}}| ${message}";
        public const string ERROR_LOG_NAME = "Errors";
        #endregion

        #endregion

        #region Services
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IConfiguration Configuration { get; private set; }
        #endregion

        #region Internals
        #endregion

        #endregion

        #region Constructors
        static Core()
        {
            CreateDirectories(WORK_DIR, DATA_DIR, LOG_DIR, INDEX_DIR);
            ConfigureLogger();

            Core.Log.Debug($"Welcome to the {PRODUCT_NAME} Debugger");
            Core.Log.Debug($"Built by {AUTHOR}");
            Core.Log.Debug("Copyright 2019. \nAll rights reserved");
        }
        #endregion

        #region Methods
        public static void ConfigureCoreServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISearchEngine<User>, LuceneSearchEngine<User>>();
        }

        public static Task Initialize(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            ServiceProvider.GetRequiredService<IConfiguration>();

            InitializeData().Wait();
            
            if (StartupArguments.Length > 0)
                if (StartupArguments[0] == "--seed")
                    SeedUsers().Wait();

            Core.Log.Debug($"{PRODUCT_NAME} has successfully been initialized.");
            return Task.CompletedTask;
        }

        static async Task InitializeData()
        {
            var roleManager = ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = ServiceProvider.GetRequiredService<UserManager<User>>();

            string[] roles = Enum.GetValues(typeof(UserRole)).OfType<UserRole>().Select(u => u.ToString()).ToArray();

            foreach (var role in roles)
            {
                if (await roleManager.RoleExistsAsync(role)) continue;
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            User powerUser = await userManager.FindByEmailAsync("admin@nysc.com");

            if (powerUser != null) return;

            powerUser = new User()
            {
                UserName = "admin",
                Email = "admin@nysc.com",
                PhoneNumberConfirmed = true,
                EmailConfirmed = true
            };

            string password = "nyscAdmin#1";

            var result = await userManager.CreateAsync(powerUser, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(powerUser, UserRole.Administrator.ToString());
        }

        public static async Task SeedUsers()
        {
            var watch = Stopwatch.StartNew();
            DateTime sessionStart = DateTime.Now;

            var userManager = ServiceProvider.GetRequiredService<UserManager<User>>();
            var userDataContext = ServiceProvider.GetRequiredService<UserDataContext>();

            var searchEngine = ServiceProvider.GetRequiredService<ISearchEngine<User>>();


            searchEngine.ClearIndexes();
            string data = File.ReadAllText("Data/UserSeedData.json");

            var users = JsonConvert.DeserializeObject<List<User>>(data, new IsoDateTimeConverter() { DateTimeFormat = "d/MM/yyyy" });

            const string passportTemplate = "https://randomuser.me/api/portraits/{0}/{1}.jpg";
            List<int> maleList = Enumerable.Range(0, 99).OrderBy(o => Guid.NewGuid()).ToList();
            List<int> femaleList = Enumerable.Range(0, 99).OrderBy(o => Guid.NewGuid()).ToList();

            List<User> addedUsers = new List<User>();

            for (int i = 0; i < users.Count; i++)
            {
                var user = users[i];

                bool male = user.Gender.ToLower() == "Male".ToLower();
                List<int> list = male ? maleList : femaleList;
                if (!list.Any()) list = Enumerable.
                        Range(0, 99).OrderBy(o => Guid.NewGuid()).ToList();


                user.UserName = user.FileNo.ToString();
                await userManager.AddPasswordAsync(user, user.PhoneNumber.ToString());

                var creationResult = await userManager.CreateAsync(user);

                string gender = male ? "men" : "women";
                int photoIndex = list.First();
                string url = string.Format(passportTemplate, gender, photoIndex);
                list.Remove(photoIndex);

                user.Activities.Add(new AccountActivity() { ActivityType = Models.Entities.Activity.AccountCreated });

                user.Photos.Add(new Models.Entities.Photo()
                {
                    Active = true,
                    Type = Models.Entities.PhotoType.Passport,
                    Url = url,
                    DateAdded = DateTime.Now,
                    PublicID = Guid.NewGuid().ToString()
                });
                
                if (creationResult.Succeeded)
                    await userManager.AddToRoleAsync(user, UserRole.RegularUser.ToString());



                addedUsers.Add(user);
            }
            searchEngine.Index(addedUsers);

            watch.Stop();
            Core.Log.Debug("*****\tSeeding Session Complete\t******");
            Core.Log.Debug($"Session Start: {sessionStart}");
            Core.Log.Debug($"Session End: {DateTime.Now}");
            Core.Log.Debug($"Elapsed Time: {watch.Elapsed}");
        }

        #region Helpers
        /// <summary>
        /// Easy and safe way to create multiple directories. 
        /// </summary>
        /// <param name="directories">The set of directories to create</param>
        public static void CreateDirectories(params string[] directories)
        {
            if (directories == null || directories.Length <= 0) return;

            foreach (var directory in directories)
                try
                {
                    if (Directory.Exists(directory)) continue;

                    Directory.CreateDirectory(directory);
                    Log.Info("A new directory has been created ({0})", directory);
                }
                catch (Exception e)
                {
                    Log.Error("Error while creating directory {0} - {1}", directory, e);
                }
        }

        public static void ClearDirectory(string directory, bool removeDirectory = false)
        {
            if (string.IsNullOrWhiteSpace(directory)) return;

            foreach (var d in Directory.EnumerateDirectories(directory))
                ClearDirectory(d, true);

            foreach (var file in Directory.EnumerateFiles(directory, "*"))
                try { File.Delete(file); }
                catch (Exception e) { Log.Error("Failed to delete {0}\n", file, e); }

            if (removeDirectory)
                try { Directory.Delete(directory); }
                catch (Exception ex) { Log.Error("An error occured while attempting to remove a directory ({0})\n{1}", directory, ex); }
        }
        #endregion

        static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();

#if DEBUG
            var debugConsole = new ColoredConsoleTarget()
            {
                Name = Core.CONSOLE_LOG_NAME,
                Layout = Core.LOG_LAYOUT,
                Header = $"{PRODUCT_NAME} Debugger"
            };

            var debugRule = new LoggingRule("*", LogLevel.Debug, debugConsole);
            config.LoggingRules.Add(debugRule);
#endif
            

            var errorFileTarget = new FileTarget()
            {
                Name = Core.ERROR_LOG_NAME,
                FileName = Core.ERROR_LOG_PATH,
                Layout = Core.LOG_LAYOUT
            };

            config.AddTarget(errorFileTarget);

            var errorRule = new LoggingRule("*", LogLevel.Error, errorFileTarget);
            config.LoggingRules.Add(errorRule);

            LogManager.Configuration = config;

            LogManager.ReconfigExistingLoggers();
        }
        #endregion
    }
}
