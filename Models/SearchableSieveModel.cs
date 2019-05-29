using Sieve.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Models
{
    public class SearchableSieveModel : SieveModel
    {
        #region Properties
        public string SearchQuery { get; set; }
        #endregion
    }
}
