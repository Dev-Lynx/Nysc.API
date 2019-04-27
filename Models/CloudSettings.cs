using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Models
{
    public class CloudSettings
    {
        #region Properties
        public string CloudName { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        #endregion
    }
}
