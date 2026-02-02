//using FreshMart.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace FreshMart.Controllers
//{
//    public class ProductsController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public ProductsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public IActionResult Index()
//        {
//            var products = _context.Products.Include(x => x.Category).ToList();
//            return View(products);
//        }

//        public IActionResult Details(int id)
//        {
//            var product = _context.Products
//                .Include(p => p.Category)
//                .FirstOrDefault(p => p.ProductId == id);

//            if (product == null)
//                return NotFound();

//            return View(product);
//        }


//        // CATEGORY FILTER
//        public IActionResult Category(int id)
//        {
//            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);
//            if (category == null) return NotFound();

//            ViewBag.CategoryName = category.CategoryName;

//            var products = _context.Products
//                .Include(p => p.Category)
//                .Where(p => p.CategoryId == id)
//                .ToList();

//            return View(products);
//        }
//    }
//}

using FreshMart.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FreshMart.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 🔥 Load all categories (needed for category buttons/menu)
            ViewBag.Categories = _context.Categories.ToList();

            var products = _context.Products
                .Include(x => x.Category)
                .ToList();

            return View(products);
        }

        public IActionResult Details(int id)
        {
            // 🔥 Categories for sidebar/menu
            ViewBag.Categories = _context.Categories.ToList();

            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // CATEGORY FILTER
        public IActionResult Category(int id)
        {
            // 🔥 Load all categories including new ones
            ViewBag.Categories = _context.Categories.ToList();

            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null) return NotFound();

            ViewBag.CategoryName = category.CategoryName;

            var products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id)
                .ToList();

            return View("Index", products);
        }
    }
}
