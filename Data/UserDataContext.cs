using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nysc.API.Models;
using Nysc.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Data
{
    public class UserDataContext : IdentityDbContext<User>
    {
        #region Properties

        #region DbSets
        public DbSet<OneTimePassword> OneTimePasswords { get; set; }
        public DbSet<UserResource> Resources { get; set; }
        #endregion

        #endregion

        #region Construtctors
        public UserDataContext(DbContextOptions<UserDataContext> options) : base(options) { }
        #endregion

        #region Methods

        #region Overrides
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Photo>().Property(u => u.Type)
                .HasConversion(new EnumToStringConverter<PhotoType>());

            builder.Entity<User>().HasOne(u => u.OneTimePassword).WithOne(p => p.User)
                .HasForeignKey<OneTimePassword>(p => p.UserId);
        }
        #endregion

        #endregion
    }
}
