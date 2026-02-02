# FreshMart - Best Practices & Code Standards Guide

## Table of Contents
1. [Error Handling Standards](#error-handling-standards)
2. [Validation Patterns](#validation-patterns)
3. [Logging Best Practices](#logging-best-practices)
4. [Security Standards](#security-standards)
5. [Code Style Guide](#code-style-guide)
6. [Common Patterns](#common-patterns)
7. [Anti-Patterns to Avoid](#anti-patterns-to-avoid)
8. [Performance Considerations](#performance-considerations)

---

## Error Handling Standards

### ✅ DO: Use Try-Catch-Finally

```csharp
public IActionResult SomeAction(int id)
{
    try
    {
        // Validate input
        if (id <= 0)
            throw new ArgumentException("Invalid ID");
        
        // Business logic
        var item = _context.Items.FirstOrDefault(i => i.Id == id);
        if (item == null)
            throw new KeyNotFoundException("Item not found");
        
        // Database operation
        _context.Items.Remove(item);
        _context.SaveChanges();
        
        _logger.LogInformation("Item deleted - {ItemId}", id);
        return RedirectToAction("Index");
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning(ex, "Invalid argument - {Message}", ex.Message);
        TempData["Error"] = "Invalid input provided";
        return View();
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error - {Message}", ex.Message);
        TempData["Error"] = "An error occurred while saving data";
        return View();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error - {Message}", ex.Message);
        TempData["Error"] = "An unexpected error occurred";
        return RedirectToAction("Index");
    }
}
```

### ❌ DON'T: Ignore Exceptions

```csharp
// BAD - Silent failure
try {
    _context.SaveChanges();
} catch { } // Never do this!

// GOOD - Log and handle
try {
    _context.SaveChanges();
} catch (Exception ex) {
    _logger.LogError(ex, "Save failed");
    throw;
}
```

### ✅ DO: Validate Before Operations

```csharp
public IActionResult AddToCart(int productId, int quantity)
{
    // Validate input first
    if (productId <= 0)
        return BadRequest("Invalid product ID");
    
    if (quantity < 1 || quantity > 100)
        return BadRequest("Invalid quantity");
    
    // Then proceed with business logic
    var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
    if (product == null)
        return NotFound("Product not found");
    
    // Check business rules
    if (quantity > product.Stock)
        return BadRequest("Insufficient stock");
    
    // Perform operation
    // ...
}
```

### ❌ DON'T: Return Generic Errors

```csharp
// BAD - Generic message
catch (Exception ex) {
    return View("Error", "An error occurred");
}

// GOOD - Specific message
catch (DbUpdateException ex) {
    _logger.LogError(ex, "Database update failed");
    return View("Error", "Failed to save data. Please try again.");
}
catch (ArgumentException ex) {
    _logger.LogWarning(ex, "Invalid input: {Message}", ex.Message);
    ModelState.AddModelError("", "Please enter valid data");
    return View(model);
}
```

---

## Validation Patterns

### ✅ DO: Multi-Layer Validation

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Register(User model)
{
    // Layer 1: Model State (Data Annotations)
    if (!ModelState.IsValid)
        return View(model);
    
    // Layer 2: Business Logic Validation
    if (!ValidationHelper.IsValidEmail(model.Email))
    {
        ModelState.AddModelError("Email", "Invalid email format");
        return View(model);
    }
    
    var (isValid, msg) = ValidationHelper.ValidatePassword(model.Password);
    if (!isValid)
    {
        ModelState.AddModelError("Password", msg);
        return View(model);
    }
    
    // Layer 3: Duplicate Check
    if (_context.Users.Any(u => u.Email == model.Email))
    {
        ModelState.AddModelError("Email", "Email already registered");
        return View(model);
    }
    
    // Layer 4: Entity Creation
    model.PasswordHash = HashPassword(model.Password);
    _context.Users.Add(model);
    _context.SaveChanges();
    
    TempData["Success"] = "Account created successfully";
    return RedirectToAction("Login");
}
```

### ✅ DO: Use Data Annotations

```csharp
public class Product
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(150, MinimumLength = 3, 
        ErrorMessage = "Name must be 3-150 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100000")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Range(0, 100000, ErrorMessage = "Stock must be 0-100000")]
    public int Stock { get; set; }
}
```

### ❌ DON'T: Skip Validation

```csharp
// BAD - No validation
public IActionResult Create(Product product)
{
    _context.Products.Add(product);
    _context.SaveChanges();
    return RedirectToAction("Index");
}

// GOOD - Validates before saving
public IActionResult Create(Product product)
{
    if (!ModelState.IsValid)
        return View(product);
    
    if (product.Price <= 0)
    {
        ModelState.AddModelError("Price", "Price must be positive");
        return View(product);
    }
    
    _context.Products.Add(product);
    _context.SaveChanges();
    return RedirectToAction("Index");
}
```

---

## Logging Best Practices

### ✅ DO: Log with Context

```csharp
// GOOD - Include context
_logger.LogInformation("User registered - Email: {Email}, Role: {Role}", 
    user.Email, user.Role);

_logger.LogWarning("Failed login attempt - Email: {Email}, Timestamp: {Timestamp}", 
    email, DateTime.UtcNow);

_logger.LogError(ex, "Database save failed - Entity: {EntityType}, Message: {Message}", 
    typeof(Product).Name, ex.Message);
```

### ❌ DON'T: Vague Log Messages

```csharp
// BAD
_logger.LogInformation("Operation completed");
_logger.LogError("Error occurred");
_logger.LogWarning("Something went wrong");

// GOOD
_logger.LogInformation("Product created - ProductId: {ProductId}, Name: {Name}", 
    product.ProductId, product.Name);
_logger.LogError(ex, "Product creation failed - Message: {Message}", ex.Message);
_logger.LogWarning("Stock low - ProductId: {ProductId}, Stock: {Stock}", 
    product.ProductId, product.Stock);
```

### ✅ DO: Use Appropriate Log Levels

| Level | Usage | Example |
|-------|-------|---------|
| **Trace** | Detailed flow | Entering method, variable values |
| **Debug** | Development info | Query parameters, calculated values |
| **Information** | Business events | User login, order created |
| **Warning** | Unexpected but recoverable | Duplicate email, stock low |
| **Error** | Error conditions | Database error, validation failed |
| **Critical** | System failure | Out of memory, file system error |

```csharp
_logger.LogDebug("Cart item quantity: {Quantity}", cart.Count);
_logger.LogInformation("Order placed - OrderId: {OrderId}", order.Id);
_logger.LogWarning("Product nearing out of stock - ProductId: {ProductId}", id);
_logger.LogError(ex, "Payment processing failed - {Message}", ex.Message);
```

---

## Security Standards

### ✅ DO: Implement CSRF Protection

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(Product product)
{
    // CSRF token automatically validated
    _context.Products.Add(product);
    _context.SaveChanges();
    return RedirectToAction("Index");
}
```

```html
<!-- In View -->
<form method="post" action="/Admin/Create">
    @Html.AntiForgeryToken()
    <!-- Form fields -->
</form>
```

### ✅ DO: Validate File Uploads

```csharp
private const long MAX_FILE_SIZE = 5 * 1024 * 1024;

if (file != null && file.Length > 0)
{
    // Check file size
    if (file.Length > MAX_FILE_SIZE)
        throw new ArgumentException("File too large");
    
    // Check file type
    var ext = Path.GetExtension(file.FileName).ToLower();
    if (!new[] { ".jpg", ".png", ".gif" }.Contains(ext))
        throw new ArgumentException("Invalid file type");
    
    // Use GUID for filename
    string filename = Guid.NewGuid().ToString() + ext;
    string path = Path.Combine(_env.WebRootPath, "uploads", filename);
    
    using (var stream = System.IO.File.Create(path))
    {
        file.CopyTo(stream);
    }
}
```

### ✅ DO: Secure Session Cookies

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;           // Prevent JavaScript access
    options.Cookie.IsEssential = true;        // Always set cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Lax;  // CSRF protection
});
```

### ❌ DON'T: Trust User Input

```csharp
// BAD - Direct usage
string query = "SELECT * FROM Products WHERE Name = '" + userInput + "'";

// GOOD - Use parameterized queries (EF Core does this automatically)
var products = _context.Products.Where(p => p.Name == userInput).ToList();

// GOOD - Use ValidationHelper to sanitize
string cleanInput = ValidationHelper.SanitizeInput(userInput);
```

### ❌ DON'T: Store Passwords in Plain Text

```csharp
// BAD
user.Password = "MyPassword123";  // Never do this!

// GOOD
user.PasswordHash = HashPassword(userProvidedPassword);

private string HashPassword(string password)
{
    using (var sha = SHA256.Create())
    {
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}
```

---

## Code Style Guide

### ✅ DO: Use Meaningful Names

```csharp
// GOOD
public IActionResult AddProductToCart(int productId, int quantity)
{
    var cartItems = HttpContext.Session.GetObject<List<CartItem>>("Cart");
    var existingItem = cartItems?.FirstOrDefault(c => c.ProductId == productId);
    
    if (existingItem != null)
        existingItem.Quantity += quantity;
}

// BAD
public IActionResult Add(int id, int qty)
{
    var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart");
    var item = cart?.FirstOrDefault(c => c.ProductId == id);
    if (item != null)
        item.Quantity += qty;
}
```

### ✅ DO: Add XML Comments

```csharp
/// <summary>
/// Validates and registers a new user account
/// </summary>
/// <param name="model">User registration model with email, password, etc.</param>
/// <returns>Redirect to login on success, or returns view with errors</returns>
/// <exception cref="ArgumentException">Thrown if validation fails</exception>
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Register(User model)
{
    // Implementation
}
```

### ✅ DO: Use Constants for Magic Values

```csharp
public class CartController : Controller
{
    private const int MAX_QUANTITY_PER_ITEM = 100;
    private const string CART_SESSION_KEY = "Cart";
    private const int SESSION_TIMEOUT_MINUTES = 30;
    
    public IActionResult Add(int productId, int quantity)
    {
        if (quantity > MAX_QUANTITY_PER_ITEM)
            throw new ArgumentException($"Max quantity is {MAX_QUANTITY_PER_ITEM}");
        
        var cart = HttpContext.Session.GetObject<List<CartItem>>(CART_SESSION_KEY);
        // ...
    }
}
```

---

## Common Patterns

### Pattern 1: Authorization Check

```csharp
private bool IsAdmin()
{
    return HttpContext.Session.GetString("UserRole") == "Admin";
}

public IActionResult AdminOnly()
{
    if (!IsAdmin())
    {
        _logger.LogWarning("Unauthorized admin access attempt - User: {UserId}", 
            HttpContext.Session.GetInt32("UserId"));
        return RedirectToAction("Login", "User");
    }
    
    // Continue with admin logic
    return View();
}
```

### Pattern 2: Entity Not Found

```csharp
public IActionResult Edit(int id)
{
    if (id <= 0)
    {
        _logger.LogWarning("Invalid ID: {Id}", id);
        return BadRequest("Invalid ID");
    }
    
    var item = _context.Items.FirstOrDefault(i => i.Id == id);
    if (item == null)
    {
        _logger.LogWarning("Item not found - {Id}", id);
        return NotFound();
    }
    
    return View(item);
}
```

### Pattern 3: Successful Operation

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Save(Product product)
{
    try
    {
        if (!ModelState.IsValid)
            return View(product);
        
        _context.Products.Add(product);
        _context.SaveChanges();
        
        _logger.LogInformation("Product saved - {ProductId}: {ProductName}", 
            product.ProductId, product.Name);
        
        TempData["Success"] = "Product saved successfully";
        return RedirectToAction("Index");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Product save failed - {Message}", ex.Message);
        TempData["Error"] = "Failed to save product";
        return View(product);
    }
}
```

---

## Anti-Patterns to Avoid

### ❌ Anti-Pattern 1: Catching All Exceptions

```csharp
// BAD - Hides all errors
try {
    ComplexOperation();
} catch { }

// GOOD - Handle specific exceptions
try {
    ComplexOperation();
} catch (ArgumentException ex) {
    _logger.LogWarning(ex, "Invalid argument");
    // Handle validation error
} catch (DbUpdateException ex) {
    _logger.LogError(ex, "Database error");
    // Handle database error
} catch (Exception ex) {
    _logger.LogError(ex, "Unexpected error");
    // Handle unexpected error
}
```

### ❌ Anti-Pattern 2: Using Exception for Flow Control

```csharp
// BAD
public bool EmailExists(string email)
{
    try {
        var user = _context.Users.First(u => u.Email == email);
        return true;
    } catch (InvalidOperationException) {
        return false;
    }
}

// GOOD
public bool EmailExists(string email)
{
    return _context.Users.Any(u => u.Email == email);
}
```

### ❌ Anti-Pattern 3: Hardcoding Values

```csharp
// BAD
if (role == "Admin") { } // What if role changes?

// GOOD
private const string ADMIN_ROLE = "Admin";
if (role == ADMIN_ROLE) { }
```

### ❌ Anti-Pattern 4: No Error Response

```csharp
// BAD
public IActionResult Delete(int id)
{
    var item = _context.Items.Find(id);
    _context.Items.Remove(item);  // Crashes if null
    _context.SaveChanges();
    return Ok();
}

// GOOD
public IActionResult Delete(int id)
{
    var item = _context.Items.Find(id);
    if (item == null)
        return NotFound("Item not found");
    
    _context.Items.Remove(item);
    _context.SaveChanges();
    return Ok("Item deleted");
}
```

---

## Performance Considerations

### ✅ DO: Use Async Operations

```csharp
public async Task<IActionResult> GetProduct(int id)
{
    var product = await _context.Products
        .FirstOrDefaultAsync(p => p.ProductId == id);
    
    if (product == null)
        return NotFound();
    
    return View(product);
}
```

### ✅ DO: Use Pagination

```csharp
public IActionResult Products(int page = 1)
{
    const int pageSize = 10;
    var totalItems = _context.Products.Count();
    var products = _context.Products
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
    
    ViewBag.TotalPages = Math.Ceiling((double)totalItems / pageSize);
    ViewBag.CurrentPage = page;
    
    return View(products);
}
```

### ✅ DO: Use Eager Loading

```csharp
// GOOD - Loads related data in one query
var products = _context.Products
    .Include(p => p.Category)
    .Include(p => p.OrderItems)
    .ToList();

// BAD - N+1 query problem
var products = _context.Products.ToList();
foreach (var p in products) {
    var category = _context.Categories.Find(p.CategoryId);  // Extra queries!
}
```

### ✅ DO: Use Caching

```csharp
public IActionResult GetCategories()
{
    const string cacheKey = "AllCategories";
    
    if (_cache.TryGetValue(cacheKey, out List<Category> categories))
        return View(categories);
    
    categories = _context.Categories.ToList();
    _cache.Set(cacheKey, categories, TimeSpan.FromHours(1));
    
    return View(categories);
}
```

---

## Summary Checklist

### Before Committing Code:
- [ ] All public methods have try-catch blocks
- [ ] All logging includes context information
- [ ] Input validation is performed
- [ ] CSRF tokens are on all POST methods
- [ ] No sensitive data in logs
- [ ] Error messages are user-friendly
- [ ] All files are properly documented
- [ ] No hardcoded values (use constants)
- [ ] No generic exception handling
- [ ] Database queries use Entity Framework

### Before Production:
- [ ] Security review completed
- [ ] All tests pass
- [ ] Error handling tested
- [ ] Performance tested
- [ ] Logging configured correctly
- [ ] Database backups working
- [ ] Deployment documented
- [ ] Rollback plan prepared

---

**Last Updated:** February 2025
**Version:** 1.0
