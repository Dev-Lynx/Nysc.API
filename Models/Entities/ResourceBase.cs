using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Models.Entities
{
    public abstract class ResourceBase
    {
        #region Properties
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Url { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public bool Active { get; set; } = true;
        public User User { get; set; }
        #endregion
    }
}
