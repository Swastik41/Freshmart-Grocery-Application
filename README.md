# 🛒 FreshMart - E-Commerce Platform

> A production-ready, full-featured e-commerce application built with ASP.NET Core MVC and SQL Server, showcasing industry-standard validation, error handling, and security best practices.

## 📋 Project Overview

FreshMart is a modern e-commerce platform featuring comprehensive product management, shopping cart functionality, order processing, and a professional admin dashboard. The application demonstrates enterprise-grade architecture with robust error handling, extensive logging, and security-first design principles.

### ✨ Key Features

- **User Authentication & Authorization**
  - Secure registration and login system
  - Role-based access control (Admin/Customer)
  - SHA256 password hashing
  - Session management with secure cookies

- **Product Management**
  - Browse products by category
  - Detailed product pages
  - Product search and filtering
  - Image upload with validation
  - Stock management

- **Shopping Cart**
  - Add/remove items
  - Quantity management with limits
  - Real-time cart updates
  - Session-based persistence

- **Order Management**
  - Secure checkout process
  - Order history
  - PDF receipt generation
  - Order details viewing

- **Admin Dashboard**
  - Product CRUD operations
  - User management
  - Category management
  - Sales analytics
  - Order tracking

- **Validation & Error Handling** ⭐
  - Comprehensive input validation (model + business logic)
  - Global exception handling middleware
  - Structured logging with context
  - File upload security (size & type validation)
  - User-friendly error messages

---

## 🏗️ Technology Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | ASP.NET Core 8 MVC |
| **Database** | SQL Server with Entity Framework Core |
| **Authentication** | Session-based with SHA256 hashing |
| **Logging** | Structured logging (Console, Debug) |
| **PDF Generation** | QuestPDF |
| **Frontend** | HTML5, CSS3, Bootstrap 5, JavaScript |
| **Security** | CSRF tokens, secure cookies, input validation |

---

## 📁 Project Structure

```
FreshMart/
├── Controllers/              # MVC Controllers with error handling
│   ├── UserController.cs    # Authentication & authorization
│   ├── CartController.cs    # Shopping cart operations
│   ├── AdminProductController.cs  # Product management
│   ├── CheckoutController.cs      # Order processing
│   └── ...
├── Models/                  # Data models with validation
│   ├── User.cs             # User entity with password hashing
│   ├── Product.cs          # Product with price/stock validation
│   ├── CartItem.cs         # Cart with quantity validation
│   ├── Order.cs            # Order management
│   └── ApiResponse.cs      # Standard response wrapper
├── Views/                   # MVC Views
│   ├── User/               # Authentication forms
│   ├── Products/           # Product display pages
│   ├── Cart/               # Shopping cart view
│   ├── Checkout/           # Order checkout
│   ├── Admin/              # Admin dashboard
│   └── Shared/             # Layout & shared components
├── Middleware/             # Custom middleware
│   └── GlobalExceptionHandlerMiddleware.cs  # Error handling
├── Helpers/                # Utility classes
│   ├── ValidationHelper.cs       # Input validation
│   └── SessionExtensions.cs      # Session utilities
├── wwwroot/                # Static files (CSS, JS, images)
└── Properties/             # Launch settings

Key Configuration Files:
├── Program.cs              # Startup & middleware configuration
├── appsettings.json        # Connection strings & settings
└── appsettings.Development.json  # Development settings
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK or later
- SQL Server 2019 or later
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/FreshMart.git
   cd FreshMart
   ```

2. **Configure Database Connection**
   
   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=FreshMart;Trusted_Connection=true;"
     }
   }
   ```

3. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Application: https://localhost:54986 (HTTPS) or http://localhost:54987 (HTTP)

### Default Test Accounts

**Admin Account:**
- Email: `admin@freshmart.com`
- Password: `Admin123`

**Customer Account:**
- Email: `john@example.com`
- Password: `Test123`

---

## 📚 Documentation

### Core Documentation Files

| Document | Purpose |
|----------|---------|
| [VALIDATION_ERROR_HANDLING.md](VALIDATION_ERROR_HANDLING.md) | Comprehensive validation & error handling guide |
| [ERROR_HANDLING_SUMMARY.md](ERROR_HANDLING_SUMMARY.md) | Summary of error handling implementation |
| [BEST_PRACTICES_GUIDE.md](BEST_PRACTICES_GUIDE.md) | Code standards & best practices |

### Quick References

- **Validation Rules:** See [VALIDATION_ERROR_HANDLING.md - Validation Rules Summary](VALIDATION_ERROR_HANDLING.md#10-validation-rules-summary)
- **Error Handling:** See [VALIDATION_ERROR_HANDLING.md - Error Handling Examples](VALIDATION_ERROR_HANDLING.md#error-messages)
- **Security Features:** See [BEST_PRACTICES_GUIDE.md - Security Standards](BEST_PRACTICES_GUIDE.md#security-standards)

---

## 🔒 Security Features

### Input Validation
- ✅ Email format validation
- ✅ Password strength validation (6-50 characters)
- ✅ File upload validation (size & type)
- ✅ XSS prevention with input sanitization
- ✅ SQL injection protection (EF Core parameterized queries)

### Authentication & Authorization
- ✅ SHA256 password hashing
- ✅ Secure session cookies (HttpOnly, Secure, SameSite)
- ✅ 30-minute session timeout
- ✅ CSRF token protection on all POST methods
- ✅ Role-based access control (Admin/Customer)

### Error Handling
- ✅ Global exception handling middleware
- ✅ Structured logging with context
- ✅ User-friendly error messages
- ✅ Proper HTTP status codes
- ✅ Database error recovery

### File Upload Security
- ✅ Maximum 5 MB file size limit
- ✅ Allowed extensions: jpg, jpeg, png, gif, webp
- ✅ GUID-based filenames (prevent overwrites)
- ✅ Automatic directory creation with validation

---

## 📊 Validation & Error Handling

### Validation Layers

```
Request
   ↓
1. Model State Validation (Data Annotations)
   ↓
2. Business Logic Validation (ValidationHelper)
   ↓
3. Authorization Check (Role-based)
   ↓
4. Database Operation with Error Handling
   ↓
Response with Error/Success Message
```

### Validation Examples

| Scenario | Validation | Response |
|----------|-----------|----------|
| Weak password (< 6 chars) | Password strength | "Password must be at least 6 characters" |
| Duplicate email | Business logic | "An account with this email already exists" |
| Oversized file (> 5 MB) | File upload | "File size cannot exceed 5 MB" |
| Invalid product ID | Input validation | BadRequest (400) |
| Out of stock product | Business logic | "Product out of stock" |
| Quantity > 100 | Cart validation | "Cannot add more than 100 items" |

### Logging Examples

All operations are logged with detailed context:

```
[INFO] Login: Successful login - Email: user@example.com, Role: Customer
[WARN] Register: Duplicate email attempt - Email: existing@email.com
[ERROR] Cart Decrease: Error decreasing quantity - Exception details...
[INFO] AdminProductController Create: Product created successfully - ProductId: 5
[WARN] Cart Add: Max quantity exceeded - ProductId: 10
```

---

## 🧪 Testing Guide

### Authentication Tests
```
✓ Register with valid credentials
✓ Register with duplicate email (error handling)
✓ Register with weak password (validation)
✓ Login with correct credentials
✓ Login with incorrect password (error message)
✓ Session timeout after 30 minutes
```

### Cart Tests
```
✓ Add product to cart
✓ Add same product again (increases quantity)
✓ Prevent adding > 100 items (validation limit)
✓ Remove item from cart
✓ Increase/decrease quantity
✓ Try to add out-of-stock product (business logic)
```

### Product Management Tests
```
✓ Create product with valid data
✓ Upload image < 5 MB
✓ Reject image > 5 MB (file validation)
✓ Reject non-image files (type validation)
✓ Edit product
✓ Delete product
✓ Prevent admin actions for non-admin users (authorization)
```

### Error Handling Tests
```
✓ Check console logs for error details
✓ Verify error messages are specific
✓ Verify validation errors appear in forms
✓ Verify TempData error messages display
✓ Verify global exception handling works
```

---

## 📈 Performance

### Optimization Features
- Eager loading to prevent N+1 queries
- Pagination for large datasets
- Asynchronous database operations
- Session-based cart caching
- Static file compression

### Load Testing Recommendations
- Test with 100+ concurrent users
- Monitor database connection pool
- Check logging performance impact
- Verify session timeout behavior

---

## 🐛 Troubleshooting

### Common Issues

**Issue:** Login not working
- **Solution:** Check password hashing in UserController. Verify PasswordHash column is 500 chars.
- **Reference:** [VALIDATION_ERROR_HANDLING.md - User Authentication](VALIDATION_ERROR_HANDLING.md#usercontroller---authentication)

**Issue:** File upload failing
- **Solution:** Verify uploads folder exists and is writable. Check file size and type validation.
- **Reference:** [VALIDATION_ERROR_HANDLING.md - File Upload Security](VALIDATION_ERROR_HANDLING.md#11-file-upload-security)

**Issue:** Session timing out too quickly
- **Solution:** Check session configuration in Program.cs. Default is 30 minutes.
- **Reference:** [Program.cs - Session Configuration](Program.cs)

**Issue:** Error messages too generic
- **Solution:** Review error handling in controllers. Add specific error messages in try-catch blocks.
- **Reference:** [BEST_PRACTICES_GUIDE.md - Error Handling Standards](BEST_PRACTICES_GUIDE.md#error-handling-standards)

---

## 🔄 Deployment

### Production Checklist

- [ ] Update connection string for production database
- [ ] Set `app.Environment.IsDevelopment()` to false
- [ ] Configure HTTPS certificates
- [ ] Set up error monitoring (Application Insights)
- [ ] Review and adjust logging levels
- [ ] Configure session timeout appropriately
- [ ] Test all validation rules in production environment
- [ ] Set up automated backups
- [ ] Configure rate limiting on authentication endpoints
- [ ] Test failover and recovery procedures

### Database Migration
```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update

# Revert migration
dotnet ef database update PreviousMigrationName
```

---

## 📝 Code Quality

### Standards Implemented

- **Validation:** ✅ Multi-layer validation (annotations + business logic)
- **Error Handling:** ✅ Try-catch-finally on all controllers
- **Logging:** ✅ Structured logging with context information
- **Security:** ✅ CSRF tokens, secure cookies, input sanitization
- **Documentation:** ✅ XML comments on key classes
- **Code Style:** ✅ Consistent naming, proper constants, meaningful variable names

### Metrics

| Metric | Status | Notes |
|--------|--------|-------|
| Error Handling | ✅ Complete | 100% of controllers |
| Input Validation | ✅ Complete | Model + business logic |
| Security | ✅ Enhanced | CSRF, secure cookies, sanitization |
| Logging | ✅ Complete | All major operations |
| Documentation | ✅ Comprehensive | 400+ lines of documentation |

---

## 🚀 Future Enhancements

### Planned Features
1. **Rate Limiting** - Prevent brute force attacks
2. **Email Verification** - Verify user email on registration
3. **Password Reset** - Secure password recovery
4. **Two-Factor Authentication** - Enhanced security
5. **Audit Logging** - Track sensitive operations
6. **API Layer** - RESTful API with Swagger docs
7. **Notifications** - Email/SMS notifications
8. **Analytics** - Detailed sales reports
9. **Data Encryption** - Encrypt sensitive data at rest
10. **GDPR Compliance** - Data retention policies

---

## 📞 Support

### Getting Help

1. **Review Documentation**
   - [VALIDATION_ERROR_HANDLING.md](VALIDATION_ERROR_HANDLING.md) - Comprehensive guide
   - [BEST_PRACTICES_GUIDE.md](BEST_PRACTICES_GUIDE.md) - Code standards

2. **Check Console Logs**
   - Run application with logging enabled
   - Monitor console output for error details

3. **Review Code Examples**
   - See controller examples in [Controllers/](Controllers/) directory
   - Check model validation in [Models/](Models/) directory

4. **Test with Sample Data**
   - Use provided test accounts (see Getting Started)
   - Test validation with different input scenarios

---

## 📄 License

This project is open source and available under the MIT License.

---

## 👨‍💻 Author

**FreshMart Development Team**
- GitHub: https://github.com/Swastik41
- Email: swastikpathak.107@gmail.com

---

## 🎓 Educational Value

This project is designed as a portfolio showcase demonstrating:

✅ **Enterprise Architecture**
- MVC pattern with proper separation of concerns
- Dependency injection and service layer
- Database abstraction with Entity Framework Core

✅ **Security Best Practices**
- Authentication and authorization
- Input validation and sanitization
- CSRF protection and secure cookies
- File upload security

✅ **Error Handling & Logging**
- Global exception handling middleware
- Structured logging with context
- User-friendly error messages
- Comprehensive error recovery

✅ **Code Quality**
- Clean code principles
- SOLID design patterns
- Comprehensive documentation
- Industry-standard practices

---

## 📊 Project Statistics

- **Lines of Code:** 5,000+
- **Controllers:** 9
- **Models:** 8
- **Views:** 20+
- **Database Tables:** 6
- **Validation Rules:** 50+
- **Error Handling Paths:** 100+
- **Logged Operations:** 40+

---

**Last Updated:** February 2026  
**Version:** 2.0 - Production Ready ✅

---

<div align="center">

### ⭐ If this project helped you, please consider giving it a star!

[GitHub](https://github.com/Swastik41/Freshmart-Grocery-Application) | [Documentation](./VALIDATION_ERROR_HANDLING.md) | [Best Practices](./BEST_PRACTICES_GUIDE.md)

</div>
