using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using BulkyWebBook.Models.ViewModels;
using BulkyWebBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWebBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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

        public IActionResult Details(int orderId)
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



        // When Admin Hit the Start Processing Button then the ---- Order Status -- will be INprocess
        // Now Addd the start processing Action Method 
        // then we  have to update the status
        // then save 
        // redirct to another method



        // ************ Now We have To Rtrive the OrderDetail FRom Database and want to update properties Explicitly ***************

        // ------------------------ *************** Below Are Condition Need to Work But NOt  *********************   -------------------------------
        // 1) After Clicking StartProcession Order Status Will Be IN Process ----------
        // 2) and Payment Status Will Be Approved
        // 
        // ********************************* For Shift Order *******************************

        // 3) If carrier and Tracking Empty Then Will show Erroe on UI side 
        // 4) once tracking  and Carried added and CLick On shit Button 
        //   COntition 1 ) For Company User ------ a) order status b) payment Status c) Paymnet due date upated 
        //                                          b) Show Pay Button ON UP 

        // Condition 2) for Normal User --------  a) order status b) payment Status c) Paymnet due date upated 

        // ****************************** Cancel Order ********************************

        //***************************** Payment For Delayed ******************************
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(OrderVM orderVM)
        {
            try
            {
                // Retrieve the order header from the database
                var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

                if (orderHeaderFromDb == null)
                {
                    return NotFound("Order not found");
                }

                // Update payment status to "Approved"
                //orderHeaderFromDb.PaymentStatus = SD.StatusApproved;

                // Update order status to "In Process"
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusInProcess, orderHeaderFromDb.PaymentStatus);

                // Save changes to the database
                _unitOfWork.Save();

                // Redirect to the details page of the updated order
                return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
            }
            catch (Exception ex)
            {
                // Log the exception (you can replace Console.WriteLine with your preferred logging mechanism)
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Return 500 Internal Server Error with a generic message to the user
                return StatusCode(500, "An error occurred while processing the request. Please try again later.");
            }
        }

        //Need To Create the shiftordermethod ----logic is must be populate the carrier and tracking number and order status changed
        //1) First retrive Order From Database 
        //2) Then Update the Tracking and Carrier 
        //3) If the the order for the coompany then we have to give the payment due date  -- here if payment status is delayed payment then we have to update the paymnet due date.


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShiftOrder(OrderVM orderVM)
        {
            try
            {
                if (orderVM == null || orderVM.OrderHeader == null)
                {
                    return BadRequest("Invalid order data");
                }
                var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);
                if (orderHeaderFromDb == null)
                {
                    return NotFound("Order not found");
                }
                if (string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber) || string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
                {
                    return BadRequest("Tracking number and carrier are required fields");
                }
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
                orderHeaderFromDb.Carrier = orderVM.OrderHeader.Carrier;
                orderHeaderFromDb.OrderStatus = SD.StatusShipped;
                orderHeaderFromDb.ShippingDate = DateTime.Now;
                if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
                {
                    orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
                }
                _unitOfWork.OrderHeader.update(orderHeaderFromDb);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, "An error occurred while processing the request. Please try again later.");
            }
        }


        // Now We Want To Work On cancel order Button
        //once click on cancel button the status must  update as cancel 
        // Refend must prpcessed 
        //Payment inten id also we get 
        // First Retrive Order 
        // First Check Payment Status If It is Approved then it means Payment Is Already Done. then we have to give refund
        // inside stripe class have refend options. then call refend Service
        // then update the status and save at database.
        // if payment not done then directly cancel the order.



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            try
            {
                var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == orderVM.OrderHeader.Id);

                if (orderHeaderFromDb == null)
                {
                    return NotFound(); // Handle case where order is not found
                }

                if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderHeaderFromDb.PaymentIntentId
                    };

                    var service = new RefundService();
                    Refund refund = service.Create(options);

                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
                }
                else
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
                }

                _unitOfWork.Save();

                // Add TempData message
                TempData["Message"] = "Order has been cancelled.";

                return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
            }
            catch (Exception ex)
            {
                // Log the exception
                // You might also want to return a specific view indicating the error to the user
                // For simplicity, this example just returns a generic error view
                TempData["ErrorMessage"] = "An error occurred while cancelling the order.";
                return View("Error");
            }
        }



        // Now We Have To Work on Delayed Payment. if order shifted by Company then they need to do paymnet.
        //create post action method -- on details method
        // As we are Postion So we need to Populate the Order Header and Order Details.

        // After Get THe Order Header And Order Details Let Work With The Stripe Payment --->>

        // Create New Page For Payment Confiramation 




        [ActionName("Details")]
        [HttpPost]
       public IActionResult Details_Pay_Now_company(int orderId)
        {
            OrderVM orderVM = new OrderVM();
            orderVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeproperties: "ApplicationUser");
            if (orderVM.OrderHeader == null)
            {
                return NotFound(); // Return a 404 Not Found response if the order is not found
            }
            orderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeproperties: "product");
            var domain = "https://localhost:7213";
            var successUrl = $"{domain}/admin/order/PaymentConfirmation?id={Uri.EscapeDataString(orderVM.OrderHeader.Id.ToString())}";
            var cancelUrl = $"{domain}/admin/order/details?id={Uri.EscapeDataString(orderVM.OrderHeader.Id.ToString())}";

            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)((decimal)item.Prise * 100), // Corrected typo in 'Prise' to 'Price'
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.product.Title,
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }



            var service = new Stripe.Checkout.SessionService();
            try
            {
                Session session = service.Create(options);

                // Update the order with the Stripe Session ID
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                // Redirect to the new URL from the session
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            catch (StripeException stripeException)
            {
                Console.WriteLine($"Stripe Exception: {stripeException.Message}");
                // Handle the exception as needed for your application
                return BadRequest("Error processing payment. Please try again.");
            }

           
        }



        // Create Payment Confiramation 

        public IActionResult PaymentConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeproperties: "ApplicationUser");

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                // then this is order by Company 
                // we have tp go and retrive the stripe session 

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                // go to stipe ducumnetetion and check the enum
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    //Here we get the paymentIntenetid
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
          
            return View(id);
        }




        #region API Calls



        // ------------------------------------  Below Are Condition Need to Work But NOt ------------------------------------------------------------------------------------
        // 1) After Clicking StartProcession Order Status Will Be IN Process 
        // 2) and Payment Status Will Be Approved 
        // 3) If carrier and Tracking Empty Then Will show Erroe on UI side 
        // 4) once tracking  and Carried added and CLick On shit Button 
        //   COntition 1 ) For Company User ------ a) order status b) payment Status c) Paymnet due date upated 
        //                                          b) Show Pay Button ON UP 

        // Condition 2) for Normal User --------  a) order status b) payment Status c) Paymnet due date upated 



        // If anything want to Retrive the Order --- The we have to retrive the OrderHeader and OrderDetail
        // For That It Crete new model OrderVM --- Who Have the OrderDetail and OrderHeader 


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
