using IdentityServer.Models.Enums;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer.Configurations
{
    public class TestUsers
    {
        public static List<TestUser> Get()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "SuperAdmin",
                    Password = "SuperAdmin",
                    
                    Claims = new List<Claim>
                    {
                        new Claim( "role", EnumRoles.SuperAdmin.ToString() )
                    }
                }
            };
        }
    }
}
