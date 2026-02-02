

//using FreshMart.Models;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Cryptography;
//using System.Text;

//namespace FreshMart.Controllers
//{
//    public class UserController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public UserController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // ---------- REGISTER ----------
//        public IActionResult Register()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult Register(User model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            // Hash password
//            model.PasswordHash = HashPassword(model.PasswordHash);

//            // Use selected role OR default
//            if (string.IsNullOrEmpty(model.Role))
//                model.Role = "Customer";

//            _context.Users.Add(model);
//            _context.SaveChanges();

//            TempData["Success"] = "Account created successfully! Please login.";
//            return RedirectToAction("Login");
//        }


//        // ---------- LOGIN ----------
//        public IActionResult Login()
//        {
//            return View();
//        }

//        [HttpPost]
//        public IActionResult Login(string Email, string PasswordHash)
//        {
//            string hashed = HashPassword(PasswordHash);

//            var user = _context.Users
//                .FirstOrDefault(u => u.Email == Email && u.PasswordHash == hashed);

//            if (user == null)
//            {
//                TempData["Error"] = "Invalid email or password";
//                return View();
//            }

//            // STORE USER SESSION VALUES
//            HttpContext.Session.SetInt32("UserId", user.UserId);
//            HttpContext.Session.SetString("UserRole", user.Role);
//            HttpContext.Session.SetString("UserName", user.FullName);   // 🔥 Required for navbar greeting

//            // REDIRECT BASED ON ROLE
//            if (user.Role == "Admin")
//                return RedirectToAction("Dashboard", "Admin");

//            return RedirectToAction("Index", "Home");
//        }




//        // ---------- LOGOUT ----------
//        public IActionResult Logout()
//        {
//            HttpContext.Session.Clear();
//            return RedirectToAction("Login");
//        }


//        // ---------- HASH ----------
//        private string HashPassword(string password)
//        {
//            using (var sha = SHA256.Create())
//            {
//                var bytes = Encoding.UTF8.GetBytes(password);
//                var hash = sha.ComputeHash(bytes);
//                return Convert.ToBase64String(hash);
//            }
//        }
//    }
//}


using FreshMart.Models;
using FreshMart.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FreshMart.Controllers
{
    /// <summary>
    /// User authentication controller with comprehensive error handling
    /// </summary>
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// GET: Display registration form
        /// </summary>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST: Register new user with validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User model)
        {
            try
            {
                // SERVER-SIDE MODEL VALIDATION
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Register: Model state invalid. Errors: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors)));
                    return View(model);
                }

                // VALIDATE EMAIL FORMAT
                if (!ValidationHelper.IsValidEmail(model.Email))
                {
                    ModelState.AddModelError("Email", "Please enter a valid email address");
                    _logger.LogWarning("Register: Invalid email format - {Email}", model.Email);
                    return View(model);
                }

                // VALIDATE PASSWORD STRENGTH
                var (isValid, message) = ValidationHelper.ValidatePassword(model.Password);
                if (!isValid)
                {
                    ModelState.AddModelError("Password", message);
                    _logger.LogWarning("Register: Weak password attempt - {Message}", message);
                    return View(model);
                }

                // VALIDATE FULL NAME
                if (!ValidationHelper.IsValidFullName(model.FullName))
                {
                    ModelState.AddModelError("FullName", "Name must be 3-40 characters and contain only letters");
                    _logger.LogWarning("Register: Invalid full name - {FullName}", model.FullName);
                    return View(model);
                }

                // CHECK FOR DUPLICATE EMAIL
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists");
                    _logger.LogWarning("Register: Duplicate email attempt - {Email}", model.Email);
                    return View(model);
                }

                // HASH PASSWORD
                model.PasswordHash = HashPassword(model.Password);

                // SET DEFAULT ROLE
                if (string.IsNullOrEmpty(model.Role))
                    model.Role = "Customer";

                // SAVE USER TO DATABASE
                _context.Users.Add(model);
                _context.SaveChanges();

                _logger.LogInformation("Register: New user created successfully - {Email}", model.Email);
                TempData["Success"] = "Account created successfully! Please login.";
                return RedirectToAction("Login");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Register: Argument exception - {Message}", ex.Message);
                ModelState.AddModelError("", "Invalid input provided");
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Register: Invalid operation - {Message}", ex.Message);
                ModelState.AddModelError("", "An error occurred during registration");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register: Unexpected error - {Message}", ex.Message);
                TempData["Error"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("Register");
            }
        }

        /// <summary>
        /// GET: Display login form
        /// </summary>
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// POST: Authenticate user with credentials
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string Email, string PasswordHash)
        {
            try
            {
                // VALIDATE INPUT
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(PasswordHash))
                {
                    _logger.LogWarning("Login: Missing email or password");
                    TempData["Error"] = "Please enter both email and password";
                    return View();
                }

                // VALIDATE EMAIL FORMAT
                if (!ValidationHelper.IsValidEmail(Email))
                {
                    _logger.LogWarning("Login: Invalid email format - {Email}", Email);
                    TempData["Error"] = "Please enter a valid email address";
                    return View();
                }

                // HASH PROVIDED PASSWORD
                string hashedPassword = HashPassword(PasswordHash);

                // QUERY DATABASE FOR USER
                var user = _context.Users.FirstOrDefault(u => 
                    u.Email == Email && u.PasswordHash == hashedPassword);

                if (user == null)
                {
                    _logger.LogWarning("Login: Failed attempt for email - {Email}", Email);
                    TempData["Error"] = "Invalid email or password";
                    return View();
                }

                // SET SESSION VALUES
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserName", user.FullName);

                _logger.LogInformation("Login: Successful login - {Email}, Role: {Role}", 
                    Email, user.Role);

                // REDIRECT BASED ON ROLE
                if (user.Role == "Admin")
                {
                    _logger.LogInformation("Login: Admin user redirected to dashboard");
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login: Unexpected error - {Message}", ex.Message);
                TempData["Error"] = "An error occurred during login. Please try again.";
                return View();
            }
        }

        /// <summary>
        /// Logout user and clear session
        /// </summary>
        public IActionResult Logout()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var userName = HttpContext.Session.GetString("UserName");
                
                HttpContext.Session.Clear();
                
                _logger.LogInformation("Logout: User logged out - UserId: {UserId}, Name: {UserName}", 
                    userId, userName);
                
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout: Error during logout - {Message}", ex.Message);
                return RedirectToAction("Login");
            }
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            try
            {
                using (var sha = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(password);
                    var hash = sha.ComputeHash(bytes);
                    return Convert.ToBase64String(hash);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HashPassword: Error hashing password - {Message}", ex.Message);
                throw;
            }
        }
    }
}
