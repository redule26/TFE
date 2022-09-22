using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace VWA_TFE.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Uid { get; set; }

        [Required]
        [MaxLength(50)]
        [BindProperty(Name = "username")]
        public string Username { get; set; }
        [MaxLength(80)]
        [BindProperty(Name = "password")]
        public string Password { get; set; }
    }
}
