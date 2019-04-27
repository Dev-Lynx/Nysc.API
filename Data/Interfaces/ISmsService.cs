using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nysc.API.Data.Interfaces
{
    public interface ISmsService
    {
        Task<HttpContent> GetBalance();
        Task<bool> SendMessage(string phoneNumber, string message);
    }
}
