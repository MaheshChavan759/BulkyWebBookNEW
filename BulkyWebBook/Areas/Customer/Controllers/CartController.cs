using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using BulkyWebBook.Models.ViewModels;
using BulkyWebBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SQLitePCL;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWebBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitofwork)
        {
            _unitOfWork = unitofwork;

        }

        public IActionResult Index()
        {
            // Here we have to retrive shopping cart for user -- for that we have required user id 

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {

                shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "product"),
                OrderHeader = new()

            };

            foreach (var item in ShoppingCartVM.shoppingCartList)
            {
                item.Prise = GetPriseBasedOnQuantity(item);

                ShoppingCartVM.OrderHeader.OrderTotal += item.Prise * item.Count;
            }

            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "product"),
                OrderHeader = new()

            };
            // We have tp Populate Application user Properties 
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            //Manually Updated All property
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var item in ShoppingCartVM.shoppingCartList)
            {
                item.Prise = GetPriseBasedOnQuantity(item);

                ShoppingCartVM.OrderHeader.OrderTotal += item.Prise * item.Count;
            }
            return View(ShoppingCartVM);
        }

        //we can retrive the shoppingcartVM as paramator of SummaryPost as summary get uses that model

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            if (claimsIdentity == null)
            {
                // Handle the case where User.Identity is null
                return RedirectToAction("Login", "Account"); // or any other appropriate action
            }
            var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                // Handle the case where userIdClaim is null or empty
                return RedirectToAction("Login", "Account"); // or any other appropriate action
            }

            var userId = userIdClaim.Value;

            // as we get directly the shoppingcartVM from the bind property so  no need to explisitlyy use new here

            ShoppingCartVM.shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeproperties: "product");

            // Other existing code...
            // need to populate ordate data and userid.

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


            // We have tp Populate Application user Properties 
            //520. never Populate the navigation property when you are trying to insert the record In the EF core always remembr
            // most likly the navigation property insert --populate 
            //ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            ApplicationUser ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            if (ApplicationUser == null)
            {
                // Handle the case where ApplicationUser is null
                return RedirectToAction("Error", "Home"); // or any other appropriate action
            }


            foreach (var item in ShoppingCartVM.shoppingCartList)
            {
                item.Prise = GetPriseBasedOnQuantity(item);

                ShoppingCartVM.OrderHeader.OrderTotal += item.Prise * item.Count;
            }

            // With the help of Application user we can check the any company user associate with the user or not 
            // here GetValueOrDefault used beacuse the companyID may be Null
            // Now If The Company ID is null means the Customer is regukar so need to captue payment
            // and if there is company id present meants --- for payment need to wait
            // for company -companyID --present--- paymentstatus -->> PaymentstatusDElayedPayment and orderstatus Approved
            // for Normal user ---companyID --0 ---- paymentstatus -->> Paymentstatuspending and orderstatus pending


            if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            // now we can create the orderheader

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            // OrderDetails
            foreach (var item in ShoppingCartVM.shoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = item.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Prise = item.Prise,
                    Count = item.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }


            // Next step is to place Order 
            // To get All item we have to itrate it through shopping cart list
            // session line item option -- we will configure the  prise and prise data --
            // prise data is basically create new prise object 
            // IN SessionLineItemPriceDataOptions we have to configure unit amout
            //Here We have to add stripe LOGIC -- Which is done by user with card 
            //Line item is basically have all Product Details 
            // order confirmation method return --->> id so here model is ID
            // 
            if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7213";

                var successUrl = $"{domain}/customer/cart/OrderConfirmation?id={Uri.EscapeDataString(ShoppingCartVM.OrderHeader.Id.ToString())}";
                var cancelUrl = $"{domain}/customer/cart/index";

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.shoppingCartList)
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
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
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
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        //first based on Id get complete Orderheader
        //once the order header we get then only care about the order header
        // if the payment status not paymentstatusdelayed then the paymnet done by user/customer not the company 

        //we have to check weather the payment done or not ---Payment  successful or not 
        // For that we have to get back session and check the status
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeproperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                // then this is order by Customer
                // we have tp go and retrive the stripe session 

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                // go to stipe ducumnetetion and check the enum
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    //Here we get the paymentIntenetid
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }


        public IActionResult plus(int cartId)
        {
            var carFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            carFromDb.Count += 1;
            _unitOfWork.ShoppingCart.update(carFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));




        }







        //public IActionResult minus(int cartId)
        //{
        //    var carFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId, tracked: true);

        //    if (carFromDb.Count <= 1)
        //    {

        //        HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
        //            .GetAll(u => u.ApplicationUserId == carFromDb.ApplicationUserId).Count() - 1);
        //        _unitOfWork.ShoppingCart.Remove(carFromDb);
        //    }
        //    else
        //    {
        //        carFromDb.Count -= 1;
        //        _unitOfWork.ShoppingCart.update(carFromDb);
        //    }

        //    _unitOfWork.Save();
        //    return RedirectToAction(nameof(Index));
        //}



        public IActionResult minus(int cartId)
        {
            var carFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            if (carFromDb == null)
            {
                return NotFound();
            }

            if (carFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(carFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
             .GetAll(u => u.ApplicationUserId == carFromDb.ApplicationUserId).Count() - 1);
            }
            else
            {
                // Detach the entity from the context
                _unitOfWork.Detach(carFromDb);
                carFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.update(carFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult remove(int cartId)
        {
            var carFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

         

            _unitOfWork.ShoppingCart.Remove(carFromDb);
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart
             .GetAll(u => u.ApplicationUserId == carFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }
        private double GetPriseBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.product.ListPrise;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.product.Prise50;
                }
                else
                {
                    return shoppingCart.product.Prise100;
                }
            }
        }
    }
}
