using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Models
{
    public class SMSOptions
    {
        public string SenderId { get; set; }
        public string GatewayId { get; set; }
        public string SecretKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string MessageTemplate { get; set; }
        public string BalanceTemplate { get; set; }
    }
}
