using Nysc.API.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Models.Entities
{
    public enum PhotoType
    {
        [Description("Any")]
        Any,
        [Description("Passport")]
        Passport,
        [Description("Signature")]
        Signature
    }

    public class Photo : ResourceBase
    {
        #region Properties
        public string PublicID { get; set; }
        public PhotoType Type { get; set; }
        #endregion
    }
}
