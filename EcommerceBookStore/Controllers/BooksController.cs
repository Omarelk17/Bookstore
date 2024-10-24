using EcommerceBookStore.Data;
using EcommerceBookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EcommerceBookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // GET: Books/Add (Only Manager can access)
        [Authorize(Roles = "Manager")]
        public IActionResult Add()
        {
            return View();
        }

        // POST: Books/Add (Only Manager can add books)
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Add(Book book)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                book.CreatedBy = user.UserName;
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Sell (Only Sales Executive can access)
        [Authorize(Roles = "Sales Executive")]
        public async Task<IActionResult> Sell(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Sell (Only Sales Executive can sell books)
        [HttpPost]
        [Authorize(Roles = "Sales Executive")]
        public async Task<IActionResult> Sell(int id, int quantity)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            if (quantity > book.Quantity || quantity <= 0)
            {
                ModelState.AddModelError("", "Invalid quantity. Ensure the quantity is available and greater than zero.");
                return View(book); // Return to the view with validation error
            }

            book.Quantity -= quantity;
            _context.Update(book);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



    }
}
