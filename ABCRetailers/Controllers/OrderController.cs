using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ABCRetailers.Services;

namespace ABCRetailers.Controllers
{
   
    public class OrderController : Controller
    {
        private readonly IFunctionsApi _api;

        public OrderController(IFunctionsApi api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _api.GetOrdersAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id)
        {
            await _api.UpdateOrderStatusAsync(id, "Processing");
            TempData["Success"] = "Order marked as PROCESSED.";
            return RedirectToAction("Index");
        }
    }
}