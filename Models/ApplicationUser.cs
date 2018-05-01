using Microsoft.AspNetCore.Identity;

namespace counter.Models
{
    public class ApplicationUser :IdentityUser
    {
        public ApplicationUser Owner { get; set; }
    }
}