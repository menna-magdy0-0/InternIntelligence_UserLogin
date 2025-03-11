using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UserLogin.Models
{
    public class LoginContext:IdentityDbContext<ApplicationUser>
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options)
        {

        }
    }
}
