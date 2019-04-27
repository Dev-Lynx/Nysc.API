using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nysc.API.Data
{
    public class SmsService : ISmsService
    {
        #region Properties

        #region Internals
        IConfiguration Configuration { get; }
        SMSOptions Options { get; }
        #endregion

        #endregion

        #region Constructors
        public SmsService(IConfiguration configuration)
        {
            Configuration = configuration;
            Options = Configuration.GetSection("Authentication:SMS").Get<SMSOptions>();
        }
        #endregion

        #region Methods
        public async Task<HttpContent> GetBalance()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Uri uri = new Uri(string.Format(Options.BalanceTemplate, Options.Username, Options.Password));
                    var response = await client.GetAsync(uri);

                    return response.Content;
                }
                catch { }
                return null;
            }
        }

        public async Task<bool> SendMessage(string phoneNumber, string message)
        {
            bool success = false;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    Uri uri = new Uri(string.Format(Options.MessageTemplate, Options.Username, Options.Password, Options.SenderId, phoneNumber, message));
                    var response = await client.GetAsync(uri);

                    success = response.IsSuccessStatusCode;
                }
                catch (Exception ex) { Console.WriteLine(ex); }
                return success;
            }
        }
        #endregion
    }
}
