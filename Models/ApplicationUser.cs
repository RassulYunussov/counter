using Microsoft.AspNetCore.Identity;

namespace counter.Models
{
    public class ApplicationUser :IdentityUser, IBusinessObject
    {
        public ApplicationUser Owner { get; set; }
    }
}