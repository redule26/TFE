using System.ComponentModel.DataAnnotations;

namespace VWA_TFE.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Uid { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
