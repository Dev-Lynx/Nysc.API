using Nysc.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.ViewModels
{
    public class SimpleUserActivity
    {
        public Activity ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
    }
}
