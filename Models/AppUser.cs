using Microsoft.AspNetCore.Identity;

namespace VWA_TFE.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public JobPosition Position { get; set; }
        public float Salary { get; set; }
    }
}
