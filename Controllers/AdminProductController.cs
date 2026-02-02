using FreshMart.Models;
using FreshMart.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMart.Controllers
{
    /// <summary>
    /// Admin product management controller with comprehensive validation and error handling
    /// </summary>
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdminProductController> _logger;
        private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB

        public AdminProductController(ApplicationDbContext context, IWebHostEnvironment env, ILogger<AdminProductController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Check if user is authenticated as admin
        /// </summary>
        private bool AdminCheck()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                _logger.LogWarning("AdminProductController: Unauthorized access attempt by user with role: {Role}", role ?? "null");
                return false;
            }
            return true;
        }

        /// <summary>
        /// List all products
        /// </summary>
        public IActionResult Index()
        {
            try
            {
                if (!AdminCheck())
                {
                    _logger.LogWarning("AdminProductController Index: Unauthorized access");
                    return RedirectToAction("Login", "Admin");
                }

                var products = _context.Products.Include(p => p.Category).ToList();
                ViewBag.Categories = _context.Categories.ToList();
                _logger.LogInformation("AdminProductController Index: Retrieved {ProductCount} products", products.Count);
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminProductController Index: Error retrieving products - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while loading products";
                return View(new List<Product>());
            }
        }

        /// <summary>
        /// Display create product form
        /// </summary>
        public IActionResult Create()
        {
            try
            {
                if (!AdminCheck())
                    return RedirectToAction("Login", "Admin");

                ViewBag.Categories = _context.Categories.ToList();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminProductController Create GET: Error - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while loading the create form";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Create new product with file upload validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, IFormFile? imageFile)
        {
            try
            {
                if (!AdminCheck())
                    return RedirectToAction("Login", "Admin");

                // MODEL VALIDATION
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("AdminProductController Create: Invalid model state");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }

                // VALIDATE PRODUCT DATA
                if (!ValidationHelper.IsValidProductName(product.Name))
                {
                    ModelState.AddModelError("Name", "Product name must be between 1-150 characters");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }

                if (!ValidationHelper.IsValidPrice(product.Price))
                {
                    ModelState.AddModelError("Price", "Price must be between 0.01 and 100000");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }

                if (!ValidationHelper.IsValidStock(product.Stock))
                {
                    ModelState.AddModelError("Stock", "Stock must be between 0 and 100000");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }

                // IMAGE FILE UPLOAD VALIDATION
                if (imageFile != null && imageFile.Length > 0)
                {
                    // CHECK FILE SIZE
                    if (imageFile.Length > MAX_FILE_SIZE)
                    {
                        ModelState.AddModelError("imageFile", $"File size cannot exceed {MAX_FILE_SIZE / (1024 * 1024)} MB");
                        _logger.LogWarning("AdminProductController Create: File too large - {FileSize} bytes", imageFile.Length);
                        ViewBag.Categories = _context.Categories.ToList();
                        return View(product);
                    }

                    // CHECK FILE TYPE
                    if (!ValidationHelper.IsValidImageFile(imageFile.FileName))
                    {
                        ModelState.AddModelError("imageFile", "Only image files (JPG, PNG, GIF, WEBP) are allowed");
                        _logger.LogWarning("AdminProductController Create: Invalid file type - {FileName}", imageFile.FileName);
                        ViewBag.Categories = _context.Categories.ToList();
                        return View(product);
                    }

                    // SAVE FILE
                    try
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                        
                        // CREATE UPLOADS FOLDER IF NOT EXISTS
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            imageFile.CopyTo(fileStream);
                        }

                        product.ImagePath = uniqueFileName;
                        _logger.LogInformation("AdminProductController Create: Image uploaded - {FileName}", uniqueFileName);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "AdminProductController Create: File upload error - {Message}", ex.Message);
                        ModelState.AddModelError("", "An error occurred while uploading the image");
                        ViewBag.Categories = _context.Categories.ToList();
                        return View(product);
                    }
                }

                // SAVE PRODUCT
                _context.Products.Add(product);
                _context.SaveChanges();

                _logger.LogInformation("AdminProductController Create: Product created successfully - {ProductId}: {ProductName}", 
                    product.ProductId, product.Name);
                TempData["Success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "AdminProductController Create: Database error - {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while saving the product");
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminProductController Create: Unexpected error - {Message}", ex.Message);
                TempData["Error"] = "An unexpected error occurred while creating the product";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Display edit product form
        /// </summary>
        public IActionResult Edit(int id)
        {
            try
            {
                if (!AdminCheck())
                    return RedirectToAction("Login", "Admin");

                if (id <= 0)
                {
                    _logger.LogWarning("AdminProductController Edit GET: Invalid product ID - {ProductId}", id);
                    return BadRequest("Invalid product ID");
                }

                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    _logger.LogWarning("AdminProductController Edit GET: Product not found - {ProductId}", id);
                    return NotFound();
                }

                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminProductController Edit GET: Error - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while loading the product";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Update product with validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product product, IFormFile? imageFile)
        {
            try
            {
                if (!AdminCheck())
                    return RedirectToAction("Login", "Admin");

                // VALIDATE ID
                if (product.ProductId <= 0)
                {
                    _logger.LogWarning("AdminProductController Edit: Invalid product ID - {ProductId}", product.ProductId);
                    return BadRequest("Invalid product ID");
                }

                // MODEL VALIDATION
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("AdminProductController Edit: Invalid model state for product {ProductId}", product.ProductId);
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }

                var existing = _context.Products.AsNoTracking().FirstOrDefault(p => p.ProductId == product.ProductId);
                if (existing == null)
                {
                    _logger.LogWarning("AdminProductController Edit: Product not found - {ProductId}", product.ProductId);
                    return NotFound();
                }

                // VALIDATE PRODUCT DATA
                if (!ValidationHelper.IsValidProductName(product.Name))
                {
                    ModelState.AddModelError("Name", "Product name must be between 1-150 characters");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }

                if (!ValidationHelper.IsValidPrice(product.Price))
                {
                    ModelState.AddModelError("Price", "Price must be between 0.01 and 100000");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(product);
                }

                // HANDLE IMAGE UPLOAD
                if (imageFile != null && imageFile.Length > 0)
                {
                    // CHECK FILE SIZE
                    if (imageFile.Length > MAX_FILE_SIZE)
                    {
                        ModelState.AddModelError("imageFile", $"File size cannot exceed {MAX_FILE_SIZE / (1024 * 1024)} MB");
                        ViewBag.Categories = _context.Categories.ToList();
                        return View(product);
                    }

                    // CHECK FILE TYPE
                    if (!ValidationHelper.IsValidImageFile(imageFile.FileName))
                    {
                        ModelState.AddModelError("imageFile", "Only image files (JPG, PNG, GIF, WEBP) are allowed");
                        ViewBag.Categories = _context.Categories.ToList();
                        return View(product);
                    }

                    try
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            imageFile.CopyTo(fileStream);
                        }

                        product.ImagePath = uniqueFileName;
                        _logger.LogInformation("AdminProductController Edit: Image updated - {FileName}", uniqueFileName);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "AdminProductController Edit: File upload error - {Message}", ex.Message);
                        ModelState.AddModelError("", "An error occurred while uploading the image");
                        ViewBag.Categories = _context.Categories.ToList();
                        return View(product);
                    }
                }
                else
                {
                    // KEEP OLD IMAGE
                    product.ImagePath = existing.ImagePath;
                }

                // UPDATE PRODUCT
                _context.Products.Update(product);
                _context.SaveChanges();

                _logger.LogInformation("AdminProductController Edit: Product updated - {ProductId}: {ProductName}", 
                    product.ProductId, product.Name);
                TempData["Success"] = "Product updated successfully";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "AdminProductController Edit: Database error - {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred while updating the product");
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminProductController Edit: Unexpected error - {Message}", ex.Message);
                TempData["Error"] = "An unexpected error occurred while updating the product";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        public IActionResult Delete(int id)
        {
            try
            {
                if (!AdminCheck())
                    return RedirectToAction("Login", "Admin");

                if (id <= 0)
                {
                    _logger.LogWarning("AdminProductController Delete: Invalid product ID - {ProductId}", id);
                    return BadRequest("Invalid product ID");
                }

                var product = _context.Products.FirstOrDefault(p => p.ProductId == id);
                if (product == null)
                {
                    _logger.LogWarning("AdminProductController Delete: Product not found - {ProductId}", id);
                    return NotFound();
                }

                _context.Products.Remove(product);
                _context.SaveChanges();

                _logger.LogInformation("AdminProductController Delete: Product deleted - {ProductId}: {ProductName}", 
                    id, product.Name);
                TempData["Success"] = "Product deleted successfully";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "AdminProductController Delete: Database error - {Message}", ex.Message);
                TempData["Error"] = "An error occurred while deleting the product";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminProductController Delete: Unexpected error - {Message}", ex.Message);
                TempData["Error"] = "An unexpected error occurred while deleting the product";
                return RedirectToAction("Index");
            }
        }
    }
}
