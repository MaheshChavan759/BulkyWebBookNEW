using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using BulkyWebBook.Models.ViewModels;
using BulkyWebBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Identity.Client;
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
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u=> u.Id == userId);

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
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // as we get directly the shoppingcartVM from the bind property so  no need to explisitlyy use new here

            ShoppingCartVM.shoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "product");

                // need to populate ordate data and userid.

                ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


			// We have tp Populate Application user Properties 
			//520. never Populate the navigation property when you are trying to insert the record In the EF core always remembr
            // most likly the navigation property insert --populate 
			//ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

		      ApplicationUser ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);



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

			if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)

			{
				// we will add the strip logic --- cpayment can done by card etc
			}

           

			return RedirectToAction(nameof(OrderConfirmation),new {id = ShoppingCartVM.OrderHeader.Id });
		}

        // order confirmation method return --->> id so here model is ID 
        public IActionResult OrderConfirmation(int id )
        {

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
        public IActionResult minus(int cartId)
        {
            var carFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            if (carFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(carFromDb);
            }
            else
            {
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
