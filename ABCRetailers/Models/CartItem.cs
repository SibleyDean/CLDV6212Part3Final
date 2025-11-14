using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABCRetailers.Models
{
    [Table("Cart")] // This maps to your existing "Cart" table in the database
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string ProductId { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; } = 1;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        // Navigation property to User
        [ForeignKey("UserId")]
        public User? User { get; set; }

        // These are NotMapped because we'll load product details from the API
        [NotMapped]
        public string? ProductName { get; set; }

        [NotMapped]
        public decimal? Price { get; set; }

        [NotMapped]
        public string? ImageUrl { get; set; }

        [NotMapped]
        public string? Description { get; set; }

        [NotMapped]
        public decimal TotalPrice => (Price ?? 0) * Quantity;
    }
}