using FreshMart.Models;
using Microsoft.AspNetCore.Mvc;

namespace FreshMart.Controllers
{
    public class AdminCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool AdminCheck()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }


        public IActionResult Index()
        {
            if (!AdminCheck()) return RedirectToAction("Login", "Admin");

            var categories = _context.Categories.ToList();
            return View(categories);
        }

        public IActionResult Create()
        {
            if (!AdminCheck()) return RedirectToAction("Login", "Admin");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (!AdminCheck()) return RedirectToAction("Login", "Admin");

            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        public IActionResult Edit(int id)
        {
            if (!AdminCheck()) return RedirectToAction("Login", "Admin");

            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (!AdminCheck()) return RedirectToAction("Login", "Admin");

            if (ModelState.IsValid)
            {
                _context.Categories.Update(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        public IActionResult Delete(int id)
        {
            if (!AdminCheck()) return RedirectToAction("Login", "Admin");

            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);

            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
