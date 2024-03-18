using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using BulkyWebBook.Models.ViewModels;
using BulkyWebBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWebBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        //We need to add Unit Of Work 
        private readonly IUnitOfWork _unitOfWork;

        private readonly OrderVM orderVM;

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

        //We have to create order details action which passes the orderid 
        // based on the order id it retrive the order detail and order header --- imformantion about the order
        //order header ---> information about the user --- so include the application user property 
        // orderdetails -->>> information about the order and order may be more than one also -- so include the prodct property 

        public IActionResult Details( int orderId)
        {
            
            //once view model propery created then no need to create new OrderVM 
            OrderVM orderVM = new OrderVM();

            orderVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeproperties: "ApplicationUser");

            if (orderVM.OrderHeader == null)
            {
                return NotFound(); // Return a 404 Not Found response if the order is not found
            }

            orderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeproperties: "product");


            return View(orderVM);
        }

        // SO In  Update Order DEtails We have to Retrive the Order Header and that Order Header We get From View Model 
        // So We will create the ViewModel Private Proverty and Then we get the OrderVM FRom
        // That We get the Order Header and Order Header 

        // Now We have To Rtrive the OrderDetail FRom Database and want to update properties Explicitly
       


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail(OrderVM orderVM)
        {
            if (orderVM == null || orderVM.OrderHeader == null)
            {
                return BadRequest("Invalid order data");
            }

            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

            if (orderHeaderFromDb == null)
            {
                return NotFound(); // Return a 404 Not Found response if the order header is not found
            }

            orderHeaderFromDb.Name = orderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.OrderHeader.City;
            orderHeaderFromDb.State = orderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.OrderHeader.PostalCode;

            // Only update Carrier and TrackingNumber if they are not null or empty
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
            }

            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.update(orderHeaderFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }





        // If anything want to Retrive the Order --- The we have to retrive the OrderHeader and OrderDetail
        // For That It Crete new model OrderVM --- Who Have the OrderDetail and OrderHeader 

        #region API Calls

        // As per status we heve to do filtering so the stats gets in paramentar 
        // After retrive all the order we can add the switch condition and then based on the switch condition we will do the filtering 

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeader;

            // so when logged application user is Admin and Employee need to display all the orders 
            // and if the appication user Pogged with the Customer or the Company Then It should be DIsplay Only Their Orders
            // First Retrive the Order From Database 
            // apply role and display 
            // and if the user is company and Customer then it should be disply ony this order for that filter out the order as per user id 
            // user id will get from Identity 
            //if (!User.Identity.IsAuthenticated)
            //{
            //    // Handle the case where the user is not logged in
            //    // Return an appropriate response, such as a 401 Unauthorized status code
            //    return Unauthorized();
            //}


            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeader = _unitOfWork.OrderHeader.GetAll(includeproperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeader = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeproperties: "ApplicationUser");
            }

            switch (status?.ToLower())
            {
                case "inprocess":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "pending":
                    objOrderHeader = objOrderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "completed":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = objOrderHeader });
        }


        #endregion
    }
}
