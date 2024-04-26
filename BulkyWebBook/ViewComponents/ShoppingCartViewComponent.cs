using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWebBook.ViewComponents
{
    public class ShoppingCartViewComponent :ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitofwork )
        {
            _unitOfWork = unitofwork;
        }
        public async Task<IViewComponentResult> InvokeAsync() 
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var ClaimuserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (ClaimuserId != null)
            {
                var cartItems = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == ClaimuserId.Value);

                HttpContext.Session.SetInt32(SD.SessionCart, cartItems.Count());

                return View( HttpContext.Session.GetInt32(SD.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            } 
        }
    }
}
