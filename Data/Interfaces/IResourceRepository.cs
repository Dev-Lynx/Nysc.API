using Nysc.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Data.Interfaces
{
    public interface IResourceRepository
    {
        #region Methods
        Task<Photo> GetPhoto(int id);
        #endregion
    }
}
