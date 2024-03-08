using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWebBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        //We need to add Unit Of Work 
        private readonly IUnitOfWork _unitOfWork;

        // Need to pass it through constructor as a parameter -->> Constructor Injection 

        public OrderController(IUnitOfWork unitOfWork)
        {
                _unitOfWork = unitOfWork;
        }

        //Now We have to Diaplay All The drders in the index page
        public IActionResult Index()
        {
            return View();
        }



        // If anything want to Retrive the Order --- The we have to retrive the OrderHeader and OrderDetail For That It Crete new model OrderVM --- Who Have the OrderDetail and OrderHeader 

        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<OrderHeader> objOrderHeader = _unitOfWork.OrderHeader.GetAll(includeproperties: "ApplicationUser").ToList();

            return Json(new { data = objOrderHeader });

        }

        #endregion
    }
}
