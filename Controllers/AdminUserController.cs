using FreshMart.Models;
using Microsoft.AspNetCore.Mvc;

namespace FreshMart.Controllers
{
    public class AdminUserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminUserController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // ---------- EDIT USER (GET) ----------
        public IActionResult Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "User");

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "User");

            // REMOVE PASSWORD VALIDATION (IMPORTANT FOR EDITING)
            ModelState.Remove("PasswordHash");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.UserId == model.UserId);
            if (user == null)
                return NotFound();

            // Check duplicate email
            if (_context.Users.Any(u => u.Email == model.Email && u.UserId != model.UserId))
            {
                ModelState.AddModelError("Email", "Email is already in use.");
                return View(model);
            }

            // Update existing fields
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.Role = model.Role;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        // ---------- LIST USERS ----------
        public IActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "User");

            var users = _context.Users.ToList();
            return View(users);
        }

        // ---------- CREATE USER ----------
        public IActionResult Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "User");

            return View();
        }

        [HttpPost]
        public IActionResult Create(User model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "User");

            if (!ModelState.IsValid)
                return View(model);

            // Hash password
            model.PasswordHash = HashPassword(model.Password);
    

            if (string.IsNullOrEmpty(model.Role))
                model.Role = "Customer";

            _context.Users.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ---------- DELETE USER ----------
        public IActionResult Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "User");

            var user = _context.Users.FirstOrDefault(u => u.UserId == id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // ---------- HASH ----------
        private string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
