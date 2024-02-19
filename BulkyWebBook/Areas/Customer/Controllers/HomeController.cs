using BulkyWebBook.DataAccess.Data;
using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWebBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> ProductList  = _unitOfWork.Product.GetAll(includeproperties : "Category");
            return View(ProductList);
        }

        public IActionResult Details( int id )
        {
            //Product ProductList = _unitOfWork.Product.Get(u=>u.Id==id,includeproperties: "Category");
            //return View(ProductList);
            //      List<Product> Prod = (List<Product>)_unitOfWork.Product.GetAll();
            //      Product ProdObj = Prod.FirstOrDefault(u => u.Id == id);
            //ShoppingCart cart = new() { ProductId = id,Count=1 };
            //return View(cart);

            List<Product> Prod = (List<Product>)_unitOfWork.Product.GetAll();
            Product ProdObj = Prod.FirstOrDefault(u => u.Id == id);

            if (ProdObj != null)
            {
                ShoppingCart cart = new ShoppingCart
                {
                    ProductId = ProdObj.Id,
                    Count = 1,
                    product = ProdObj
                };
                return View(cart);
            }
            else
            {
                return RedirectToAction("ProductNotFound");
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            // Retrieve the user ID from the claims
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Set the user ID and ensure the ID property is not explicitly set
            shoppingCart.ApplicationUserId = userId;
            shoppingCart.Id = 0; // Assuming ID is the identity column, set it to 0 to let the database generate a new value.

            List<ShoppingCart> SHOP = (List<ShoppingCart>)_unitOfWork.ShoppingCart.GetAll();
            ShoppingCart SHOPObj = SHOP.FirstOrDefault(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if (SHOPObj != null)
            {
                SHOPObj.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.update(SHOPObj);
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
            }
            // Save changes to the database
            _unitOfWork.Save();

            // Redirect to the index action
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
