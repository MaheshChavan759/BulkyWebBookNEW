using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using BulkyWebBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWebBook.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

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
