using System.ComponentModel.DataAnnotations;

namespace ProgressTrackerUserService.ViewModels
{
    public class UserRegisterModel
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [MaxLength(100)]
        public string University { get; set; }
    }
}