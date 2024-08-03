using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgressTrackerUserService.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

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
        public string PasswordHash { get; set; }

        [MaxLength(100)]
        public string University { get; set; }

        public DateTime DateCreated { get; set; }

        public User()
        {
            DateCreated = DateTime.UtcNow;
        }

        public User(int userId, string firstName, string lastName, string email, string passwordHash, string university)
        {
            UserId = userId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PasswordHash = passwordHash;
            University = university;
            DateCreated = DateTime.UtcNow;
        }
    }
}