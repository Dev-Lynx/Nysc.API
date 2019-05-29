using Microsoft.AspNetCore.Mvc;
using Nysc.API.Data.Interfaces;
using Nysc.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Nysc.API.Data
{
    public class ResourceRepository : IResourceRepository
    {
        #region Properties

        #region Internals
        UserDataContext UserDataContext { get; }
        #endregion

        #endregion

        #region Constructors
        public ResourceRepository(UserDataContext userDataContext)
        {
            UserDataContext = userDataContext;
        }
        #endregion

        #region Methods
        public async Task<Photo> GetPhoto(string id)
        {
            var resource = await UserDataContext.Resources.OfType<Photo>().FirstOrDefaultAsync(p => p.Id == id);
            if (resource == null) throw new InvalidOperationException("The requested photo was not found. Are you sure it wasn't deleted?");
            return resource;
        }
        #endregion
    }
}
