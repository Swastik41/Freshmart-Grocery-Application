using FreshMart.Models;
using FreshMart.Middleware;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.AddEventSourceLogger();
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Environment.EnvironmentName = "Development";

QuestPDF.Settings.License = LicenseType.Community;

// Register DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Add global exception handling middleware in development
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();





app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------------- Seed Data -----------------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Clear existing products and categories to reseed with enhanced data
    // IMPORTANT: Comment out this section after first run to preserve data
    /*
    if (db.Products.Any())
    {
        db.Products.RemoveRange(db.Products);
        db.SaveChanges();
    }

    if (db.Categories.Any())
    {
        db.Categories.RemoveRange(db.Categories);
        db.SaveChanges();
    }
    */

    // Categories Seed
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(
            new Category { CategoryName = "Fruits", Description = "Fresh fruits" },
            new Category { CategoryName = "Vegetables", Description = "Fresh vegetables" },
            new Category { CategoryName = "Dairy", Description = "Milk and dairy items" },
            new Category { CategoryName = "Bakery", Description = "Breads and bakery items" }
        );

        db.SaveChanges();
    }

    // Seed Admin Users
    if (!db.Users.Any())
    {
        // Hash function for passwords
        string HashPassword(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        db.Users.AddRange(
            new User
            {
                FullName = "Admin User",
                Email = "admin@freshmart.com",
                PasswordHash = HashPassword("Admin123"),
                Role = "Admin",
                CreatedAt = DateTime.Now
            },
            new User
            {
                FullName = "John Doe",
                Email = "john@example.com",
                PasswordHash = HashPassword("Test123"),
                Role = "Customer",
                CreatedAt = DateTime.Now
            }
        );

        db.SaveChanges();
    }

    // Products Seed
    if (!db.Products.Any())
    {
        var fruitsId = db.Categories.First(c => c.CategoryName == "Fruits").CategoryId;
        var vegetablesId = db.Categories.First(c => c.CategoryName == "Vegetables").CategoryId;
        var dairyId = db.Categories.First(c => c.CategoryName == "Dairy").CategoryId;
        var bakeryId = db.Categories.First(c => c.CategoryName == "Bakery").CategoryId;

        db.Products.AddRange(
            // FRUITS
            new Product
            {
                Name = "Fresh Apples",
                Description = "Crisp and sweet red apples, perfect for snacking or baking",
                Price = 3.99m,
                DiscountPrice = 2.99m,
                Weight = "1kg",
                Stock = 50,
                CategoryId = fruitsId,
                ImagePath = "apples.jpg"
            },
            new Product
            {
                Name = "Organic Bananas",
                Description = "Naturally ripened organic bananas, rich in potassium",
                Price = 2.49m,
                Weight = "1kg",
                Stock = 75,
                CategoryId = fruitsId,
                ImagePath = "bananas.jpg"
            },
            new Product
            {
                Name = "Fresh Strawberries",
                Description = "Sweet and juicy strawberries, freshly harvested",
                Price = 5.99m,
                DiscountPrice = 4.99m,
                Weight = "500g",
                Stock = 30,
                CategoryId = fruitsId,
                ImagePath = "strawberries.jpg"
            },
            new Product
            {
                Name = "Blueberries",
                Description = "Antioxidant-rich fresh blueberries",
                Price = 6.99m,
                Weight = "250g",
                Stock = 40,
                CategoryId = fruitsId,
                ImagePath = "blueberries.jpg"
            },
            new Product
            {
                Name = "Sweet Oranges",
                Description = "Juicy Valencia oranges, high in Vitamin C",
                Price = 4.49m,
                Weight = "1kg",
                Stock = 60,
                CategoryId = fruitsId,
                ImagePath = "oranges.jpg"
            },
            new Product
            {
                Name = "Red Grapes",
                Description = "Seedless red grapes, perfect for snacking",
                Price = 5.49m,
                Weight = "500g",
                Stock = 35,
                CategoryId = fruitsId,
                ImagePath = "grapes.jpg"
            },
            new Product
            {
                Name = "Fresh Mango",
                Description = "Sweet tropical mango, ready to eat",
                Price = 3.99m,
                Weight = "Each",
                Stock = 25,
                CategoryId = fruitsId,
                ImagePath = "mango.jpg"
            },
            new Product
            {
                Name = "Watermelon",
                Description = "Large, juicy watermelon perfect for summer",
                Price = 7.99m,
                Weight = "Each",
                Stock = 15,
                CategoryId = fruitsId,
                ImagePath = "watermelon.jpg"
            },
            new Product
            {
                Name = "Fresh Pineapple",
                Description = "Sweet and tangy fresh pineapple",
                Price = 4.99m,
                Weight = "Each",
                Stock = 20,
                CategoryId = fruitsId,
                ImagePath = "pineapple.jpg"
            },

            // VEGETABLES
            new Product
            {
                Name = "Fresh Broccoli",
                Description = "Nutrient-rich fresh broccoli crowns",
                Price = 2.99m,
                Weight = "500g",
                Stock = 45,
                CategoryId = vegetablesId,
                ImagePath = "broccoli.jpg"
            },
            new Product
            {
                Name = "Organic Carrots",
                Description = "Sweet and crunchy organic carrots",
                Price = 2.49m,
                Weight = "1kg",
                Stock = 55,
                CategoryId = vegetablesId,
                ImagePath = "carrots.jpg"
            },
            new Product
            {
                Name = "Fresh Tomatoes",
                Description = "Vine-ripened tomatoes, perfect for salads",
                Price = 3.49m,
                Weight = "500g",
                Stock = 50,
                CategoryId = vegetablesId,
                ImagePath = "tomatoes.jpg"
            },
            new Product
            {
                Name = "Baby Spinach",
                Description = "Fresh baby spinach leaves, pre-washed",
                Price = 3.99m,
                Weight = "250g",
                Stock = 40,
                CategoryId = vegetablesId,
                ImagePath = "spinach.jpg"
            },
            new Product
            {
                Name = "Bell Peppers Mix",
                Description = "Colorful mix of red, yellow, and green bell peppers",
                Price = 4.99m,
                Weight = "3 Pack",
                Stock = 35,
                CategoryId = vegetablesId,
                ImagePath = "bellpeppers.jpg"
            },
            new Product
            {
                Name = "Russet Potatoes",
                Description = "Perfect for baking, frying, or mashing",
                Price = 3.99m,
                Weight = "2kg",
                Stock = 60,
                CategoryId = vegetablesId,
                ImagePath = "potatoes.jpg"
            },
            new Product
            {
                Name = "Fresh Cucumber",
                Description = "Crisp and refreshing cucumbers",
                Price = 1.99m,
                Weight = "Each",
                Stock = 45,
                CategoryId = vegetablesId,
                ImagePath = "cucumber.jpg"
            },

            // DAIRY
            new Product
            {
                Name = "Whole Milk",
                Description = "Fresh whole milk, locally sourced",
                Price = 4.49m,
                Weight = "1L",
                Stock = 70,
                CategoryId = dairyId,
                ImagePath = "c2f98112-cb02-4ccb-af56-bb9241d1f03f_milk.jpg"
            },
            new Product
            {
                Name = "Greek Yogurt",
                Description = "Creamy Greek yogurt, high in protein",
                Price = 5.99m,
                Weight = "500g",
                Stock = 40,
                CategoryId = dairyId,
                ImagePath = "yogurt.jpg"
            },
            new Product
            {
                Name = "Cheddar Cheese",
                Description = "Sharp cheddar cheese, aged to perfection",
                Price = 7.99m,
                Weight = "400g",
                Stock = 30,
                CategoryId = dairyId,
                ImagePath = "cheddar.jpg"
            },
            new Product
            {
                Name = "Organic Butter",
                Description = "Creamy organic butter, unsalted",
                Price = 5.49m,
                Weight = "250g",
                Stock = 45,
                CategoryId = dairyId,
                ImagePath = "butter.jpg"
            },
            new Product
            {
                Name = "Cream Cheese",
                Description = "Smooth and spreadable cream cheese",
                Price = 4.99m,
                Weight = "250g",
                Stock = 35,
                CategoryId = dairyId,
                ImagePath = "creamcheese.jpg"
            },

            // BAKERY
            new Product
            {
                Name = "Fresh Croissants",
                Description = "Buttery and flaky French croissants",
                Price = 6.99m,
                Weight = "6 Pack",
                Stock = 25,
                CategoryId = bakeryId,
                ImagePath = "croissant.jpg"
            },
            new Product
            {
                Name = "Burger Buns",
                Description = "Soft sesame seed burger buns",
                Price = 3.99m,
                Weight = "8 Pack",
                Stock = 40,
                CategoryId = bakeryId,
                ImagePath = "burgerbuns.jpg"
            },
            new Product
            {
                Name = "Blueberry Muffins",
                Description = "Fresh-baked blueberry muffins",
                Price = 7.99m,
                Weight = "6 Pack",
                Stock = 30,
                CategoryId = bakeryId,
                ImagePath = "muffins.jpg"
            },
            new Product
            {
                Name = "Garlic Bread",
                Description = "Crispy garlic bread with herbs",
                Price = 4.49m,
                Weight = "Each",
                Stock = 35,
                CategoryId = bakeryId,
                ImagePath = "garlicbread.jpg"
            }
        );

        db.SaveChanges();
    }
}

app.Run();
