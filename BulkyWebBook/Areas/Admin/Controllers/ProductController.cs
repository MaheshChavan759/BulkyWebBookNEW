using BulkyWebBook.DataAccess.Data;
using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using BulkyWebBook.Models.ViewModels;
using BulkyWebBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;

namespace BulkyWebBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitofwork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitofwork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> ProductList = _unitOfWork.Product.GetAll(includeproperties: "Category").ToList();

            return View(ProductList);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.category.GetAll()
               .Select(u => new SelectListItem
               {
                   Text = u.CategoryName,
                   Value = u.CategoryId.ToString()
               }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                //Create functionality 
                return View(productVM);
            }
            else
            {
                // For Update 

                //Product p = _unitOfWork.Product.Get(u => u.Id == id);
                List<Product> prods = (List<Product>)_unitOfWork.Product.GetAll();
                productVM.Product = prods.FirstOrDefault(b => b.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwroot = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string ProductPath = Path.Combine(wwwroot, @"images\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        //Delete the old Image
                        var oldimage = Path.Combine(wwwroot, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldimage))
                        {
                            System.IO.File.Delete(oldimage);

                        }
                    }

                    using (var filestream = new FileStream(Path.Combine(ProductPath, filename), FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + filename;

                }


                if (obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                }

                else
                {
                    _unitOfWork.Product.update(obj.Product);
                }

                _unitOfWork.Save();
                TempData["success"] = "Product Created Successfully.";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                obj.CategoryList = _unitOfWork.category.GetAll()
             .Select(u => new SelectListItem
             {
                 Text = u.CategoryName,
                 Value = u.CategoryId.ToString()
             });
                return View(obj);
            }
        }



        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeproperties: "Category").ToList();

            return Json(new { data = objProductList });

        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var ProductToDelete = _unitOfWork.Product.Get(u => u.Id == id);

            if(ProductToDelete == null) 
            {
                return Json(new {success = false,message = "Error While Deleteing"});
            }
            // if Above is valid need to delete image also 
            var oldimage = Path.Combine(_webHostEnvironment.WebRootPath, 
                ProductToDelete.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldimage))
            {
                System.IO.File.Delete(oldimage);
            }

            _unitOfWork.Product.Remove(ProductToDelete);

            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}

