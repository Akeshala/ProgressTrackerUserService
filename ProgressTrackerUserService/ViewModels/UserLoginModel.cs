using System.ComponentModel.DataAnnotations;

namespace ProgressTrackerUserService.ViewModels
{
    public class UserLoginModel
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}