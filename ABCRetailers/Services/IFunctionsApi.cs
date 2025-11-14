using ABCRetailers.Models;

namespace ABCRetailers.Services
{
    public interface IFunctionsApi
    {
        // Products 
        Task<List<Product>> GetProductsAsync();
        Task<Product?> GetProductAsync(string id);
        Task<Product> CreateProductAsync(Product p, IFormFile? imageFile);
        Task<Product> UpdateProductAsync(string id, Product p, IFormFile? imageFile);
        Task DeleteProductAsync(string id);

        // Orders
        Task<Order?> GetOrderAsync(string id);
        Task<List<Order>> GetOrdersAsync();
        Task<Order> CreateOrderAsync(string customerId, string productId, int quantity);
        Task UpdateOrderStatusAsync(string id, string newStatus);
        Task DeleteOrderAsync(string id);

        /// Customers
        Task<List<Customer>> GetCustomersAsync();
        Task<Customer?> GetCustomerAsync(string id);
        Task<Customer> CreateCustomerAsync(Customer c);  // Fixed: CreateCustomerAsync
        Task<Customer> UpdateCustomerAsync(string id, Customer c);  // Fixed: UpdateCustomerAsync
        Task DeleteCustomerAsync(string id); //

        // Uploads
        Task<string> UploadProofOfPaymentAsync(IFormFile file, string? orderId, string? customerName);
    }
}
