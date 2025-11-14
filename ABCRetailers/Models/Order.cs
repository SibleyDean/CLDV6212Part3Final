using System.ComponentModel.DataAnnotations;

namespace ABCRetailers.Models
{
    public enum OrderStatus
    {
        Submitted,
        Processing,
        Completed,
        Cancelled
    }

    public class Order
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public string ProductId { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount => UnitPrice * Quantity;

        public DateTimeOffset? OrderDateUtc { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Submitted;
    }
}
