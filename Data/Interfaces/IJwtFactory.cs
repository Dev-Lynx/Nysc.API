using Nysc.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nysc.API.Data.Interfaces
{
    public interface IJwtFactory
    {
        JwtIssuerOptions JwtOptions { get; }
        Task<string> GenerateToken(User user);
    }
}
