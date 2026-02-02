using FreshMart.Helpers;
using FreshMart.Models;
using FreshMart.Services;
using Microsoft.AspNetCore.Mvc;

namespace FreshMart.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // SHOW CHECKOUT PAGE
        // =========================
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            var user = _context.Users.Find(userId);
            if (user == null)
                return RedirectToAction("Login", "User");

            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new();

            ViewBag.Cart = cart;
            ViewBag.CartTotal = cart.Sum(c => c.Price * c.Quantity);

            var model = new CheckoutViewModel
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                
            };

            return View(model);
        }

        // =========================
        // PLACE ORDER (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(CheckoutViewModel model)
        {
            // NEVER trust hidden fields
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null)
                return RedirectToAction("Login", "User");

            model.UserId = sessionUserId.Value;

            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new();
            if (!cart.Any())
                return RedirectToAction("Index", "Cart");

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                ViewBag.CartTotal = cart.Sum(c => c.Price * c.Quantity);
                return View("Index", model);
            }

            var order = new Order
            {
                UserId = model.UserId,
                OrderDate = DateTime.Now,
                PaymentMethod = model.PaymentMethod,
                TotalAmount = cart.Sum(c => c.Price * c.Quantity)
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            _context.SaveChanges();
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Success", new { id = order.OrderId });
        }

        // =========================
        // SUCCESS PAGE
        // =========================
        public IActionResult Success(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // =========================
        // RECEIPT (PDF)
        // =========================
        public IActionResult Receipt(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
                return NotFound();

            var items = _context.OrderItems.Where(i => i.OrderId == id).ToList();
            var products = _context.Products.ToList();

            var service = new ReceiptService();
            var pdfBytes = service.GenerateReceipt(order, items, products);

            return File(pdfBytes, "application/pdf", $"Receipt_{order.OrderId}.pdf");
        }

        // =========================
        // MY ORDERS
        // =========================
        public IActionResult MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
    }
}