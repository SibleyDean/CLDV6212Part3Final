using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string UserName { get; set; } = "";

        [EmailAddress, MaxLength(256)]
        public string? Email { get; set; }

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = ""; // Change back to PasswordHash

        [Required, MaxLength(50)]
        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Remove IsActive property since it doesn't exist in database
    }
}