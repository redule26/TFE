using Microsoft.AspNetCore.Identity;

namespace VWA_TFE.Models
{

    //rajoute quattre propriétés supplémentaires en plus de celles reçues de IndentityUser (Héritage)
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public JobPosition Position { get; set; }
        public float Salary { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
