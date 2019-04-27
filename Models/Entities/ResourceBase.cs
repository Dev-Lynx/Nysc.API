using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Models.Entities
{
    public abstract class UserResource
    {
        #region Properties
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime DateAdded { get; set; }

        public bool Active { get; set; } = true;

        public User User { get; set; }
        #endregion
    }
}
