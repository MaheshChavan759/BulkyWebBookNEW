using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            Product ProductList = _unitOfWork.Product.Get(u=>u.Id==id,includeproperties: "Category");
            return View(ProductList);
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
