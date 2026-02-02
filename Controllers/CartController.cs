using FreshMart.Models;
using FreshMart.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMart.Controllers
{
    /// <summary>
    /// Shopping cart controller with comprehensive validation
    /// </summary>
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;
        private const int MAX_QUANTITY_PER_ITEM = 100;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Display shopping cart
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                _logger.LogInformation("Cart view accessed. Items in cart: {ItemCount}", cart.Count);
                return View(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing cart - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while loading your cart";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Add product to cart with validation
        /// </summary>
        public IActionResult Add(int id)
        {
            try
            {
                // VALIDATE PRODUCT ID
                if (id <= 0)
                {
                    _logger.LogWarning("Cart Add: Invalid product ID - {ProductId}", id);
                    TempData["Error"] = "Invalid product ID";
                    return RedirectToAction("Index", "Home");
                }

                // RETRIEVE PRODUCT
                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    _logger.LogWarning("Cart Add: Product not found - {ProductId}", id);
                    TempData["Error"] = "Product not found";
                    return NotFound();
                }

                // CHECK STOCK
                if (product.Stock <= 0)
                {
                    _logger.LogWarning("Cart Add: Product out of stock - {ProductId}", id);
                    TempData["Error"] = "This product is out of stock";
                    return RedirectToAction("Details", "Products", new { id });
                }

                // GET OR CREATE CART
                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();

                // CHECK IF ITEM ALREADY IN CART
                var existingItem = cart.FirstOrDefault(c => c.ProductId == id);

                if (existingItem == null)
                {
                    // ADD NEW ITEM
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Name = product.Name,
                        Price = product.Price,
                        ImagePath = product.ImagePath,
                        Quantity = 1
                    });
                    _logger.LogInformation("Cart Add: New item added - ProductId: {ProductId}, Name: {ProductName}", 
                        product.ProductId, product.Name);
                }
                else
                {
                    // VALIDATE QUANTITY LIMIT
                    if (existingItem.Quantity >= MAX_QUANTITY_PER_ITEM)
                    {
                        _logger.LogWarning("Cart Add: Max quantity exceeded - ProductId: {ProductId}", id);
                        TempData["Error"] = $"Cannot add more than {MAX_QUANTITY_PER_ITEM} of this item";
                        return RedirectToAction("Index");
                    }

                    // INCREASE QUANTITY
                    existingItem.Quantity++;
                    _logger.LogInformation("Cart Add: Quantity increased - ProductId: {ProductId}, NewQuantity: {Quantity}", 
                        id, existingItem.Quantity);
                }

                // SAVE CART TO SESSION
                HttpContext.Session.SetObject("Cart", cart);
                TempData["Success"] = "Product added to cart successfully";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Cart Add: Invalid argument - {Message}", ex.Message);
                TempData["Error"] = "Invalid input provided";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cart Add: Unexpected error - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while adding to cart";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        public IActionResult Remove(int id)
        {
            try
            {
                // VALIDATE PRODUCT ID
                if (id <= 0)
                {
                    _logger.LogWarning("Cart Remove: Invalid product ID - {ProductId}", id);
                    TempData["Error"] = "Invalid product ID";
                    return RedirectToAction("Index");
                }

                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var item = cart.FirstOrDefault(c => c.ProductId == id);

                if (item != null)
                {
                    cart.Remove(item);
                    HttpContext.Session.SetObject("Cart", cart);
                    _logger.LogInformation("Cart Remove: Item removed - ProductId: {ProductId}, ProductName: {ProductName}", 
                        id, item.ProductName);
                    TempData["Success"] = "Item removed from cart";
                }
                else
                {
                    _logger.LogWarning("Cart Remove: Item not found in cart - {ProductId}", id);
                    TempData["Warning"] = "Item not found in cart";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cart Remove: Error removing item - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while removing the item";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Increase item quantity with validation
        /// </summary>
        public IActionResult Increase(int id)
        {
            try
            {
                // VALIDATE PRODUCT ID
                if (id <= 0)
                {
                    _logger.LogWarning("Cart Increase: Invalid product ID - {ProductId}", id);
                    TempData["Error"] = "Invalid product ID";
                    return RedirectToAction("Index");
                }

                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var item = cart.FirstOrDefault(c => c.ProductId == id);

                if (item != null)
                {
                    // CHECK QUANTITY LIMIT
                    if (item.Quantity >= MAX_QUANTITY_PER_ITEM)
                    {
                        _logger.LogWarning("Cart Increase: Max quantity exceeded - ProductId: {ProductId}", id);
                        TempData["Warning"] = $"Cannot add more than {MAX_QUANTITY_PER_ITEM} of this item";
                        return RedirectToAction("Index");
                    }

                    item.Quantity++;
                    HttpContext.Session.SetObject("Cart", cart);
                    _logger.LogInformation("Cart Increase: Quantity increased - ProductId: {ProductId}, NewQuantity: {Quantity}", 
                        id, item.Quantity);
                }
                else
                {
                    _logger.LogWarning("Cart Increase: Item not found - {ProductId}", id);
                    TempData["Warning"] = "Item not found in cart";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cart Increase: Error increasing quantity - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while updating quantity";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Decrease item quantity with validation
        /// </summary>
        public IActionResult Decrease(int id)
        {
            try
            {
                // VALIDATE PRODUCT ID
                if (id <= 0)
                {
                    _logger.LogWarning("Cart Decrease: Invalid product ID - {ProductId}", id);
                    TempData["Error"] = "Invalid product ID";
                    return RedirectToAction("Index");
                }

                var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart") ?? new List<CartItem>();
                var item = cart.FirstOrDefault(c => c.ProductId == id);

                if (item != null)
                {
                    if (item.Quantity > 1)
                    {
                        item.Quantity--;
                        _logger.LogInformation("Cart Decrease: Quantity decreased - ProductId: {ProductId}, NewQuantity: {Quantity}", 
                            id, item.Quantity);
                    }
                    else
                    {
                        cart.Remove(item);
                        _logger.LogInformation("Cart Decrease: Item removed (quantity was 1) - ProductId: {ProductId}", id);
                    }

                    HttpContext.Session.SetObject("Cart", cart);
                }
                else
                {
                    _logger.LogWarning("Cart Decrease: Item not found - {ProductId}", id);
                    TempData["Warning"] = "Item not found in cart";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cart Decrease: Error decreasing quantity - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while updating quantity";
                return RedirectToAction("Index");
            }
        }
    }
}
