using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.DAL.Entities
{
    public class AppUser : IdentityUser
    {
        public AppUser( )          
        {
        }
        public AppUser(string userName)
            :base (userName)
        {
        }
        public DateTime AccountExpires { get; set; }
    }
}
