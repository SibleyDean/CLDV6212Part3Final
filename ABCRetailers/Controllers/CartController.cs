using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ABCRetailers.Data;
using ABCRetailers.Models;
using ABCRetailers.Services;
using System.Security.Claims;

namespace ABCRetailers.Controllers
{ // Add this line - Only Customers can access cart
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IFunctionsApi _api;
        private readonly ILogger<CartController> _logger;

        public CartController(AppDbContext context, IFunctionsApi api, ILogger<CartController> logger)
        {
            _context = context;
            _api = api;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    TempData["Error"] = "Please log in to view your cart";
                    return RedirectToAction("Login", "Auth");
                }

                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                // Load product details for each cart item
                foreach (var item in cartItems)
                {
                    try
                    {
                        var product = await _api.GetProductAsync(item.ProductId);
                        if (product != null)
                        {
                            item.ProductName = product.ProductName;
                            item.Price = product.Price;
                            item.ImageUrl = product.ImageUrl;
                            item.Description = product.Description ?? string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not load product details for {ProductId}", item.ProductId);
                    }
                }

                return View(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading cart");
                TempData["Error"] = "Error loading cart";
                return View(new List<CartItem>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, int quantity = 1)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    TempData["Error"] = "Please log in to add items to cart";
                    return RedirectToAction("Login", "Auth");
                }

                if (string.IsNullOrEmpty(productId))
                {
                    TempData["Error"] = "Invalid product";
                    return RedirectToAction("Index", "Product");
                }

                // Get product details from API
                var product = await _api.GetProductAsync(productId);
                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction("Index", "Product");
                }

                // Check stock
                if (product.StockAvailable < quantity)
                {
                    TempData["Error"] = $"Not enough stock. Only {product.StockAvailable} available";
                    return RedirectToAction("Index", "Product");
                }

                // Check if item already exists in cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);

                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Quantity += quantity;
                    _context.CartItems.Update(existingItem);
                }
                else
                {
                    // Create new cart item
                    var cartItem = new CartItem
                    {
                        UserId = userId.Value,
                        ProductId = productId,
                        Quantity = quantity,
                        DateAdded = DateTime.UtcNow
                    };
                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"{product.ProductName} added to cart!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart");
                TempData["Error"] = "Error adding product to cart";
            }

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    TempData["Error"] = "Please log in to update cart";
                    return RedirectToAction("Login", "Auth");
                }

                if (quantity <= 0)
                    return await RemoveFromCart(id);

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cart updated!";
                }
                else
                {
                    TempData["Error"] = "Item not found in your cart";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity");
                TempData["Error"] = "Error updating cart";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    TempData["Error"] = "Please log in to update cart";
                    return RedirectToAction("Login", "Auth");
                }

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Item removed from cart";
                }
                else
                {
                    TempData["Error"] = "Item not found in your cart";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart");
                TempData["Error"] = "Error removing item from cart";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    TempData["Error"] = "Please log in to clear cart";
                    return RedirectToAction("Login", "Auth");
                }

                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cart cleared!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                TempData["Error"] = "Error clearing cart";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> GetCartCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Json(new { count = 0 });
                }

                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                return Json(new { count = cartItems.Sum(item => item.Quantity) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return Json(new { count = 0 });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return null;
        }
    }
}