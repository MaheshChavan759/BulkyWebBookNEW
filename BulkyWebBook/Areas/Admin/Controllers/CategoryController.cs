using BulkyWebBook.DataAccess.Data;
using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using BulkyWebBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWebBook.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitofwork)
        {
            _unitOfWork = unitofwork;
        }
        public IActionResult Index()
        {
            List<Category> categoryList = _unitOfWork.category.GetAll().ToList();
            return View(categoryList);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Created Successfully.";
                return RedirectToAction("Index", "Category");
            }

            return View();
        }

        public IActionResult Edit(int? CategoryID)
        {
            if (CategoryID == null || CategoryID == 0)
            {
                return NotFound();
            }
            Category CategoryFromDb = _unitOfWork.category.Get(u => u.CategoryId == CategoryID);
            //Category CategoryFromDb1 = _db.Categories.FirstOrDefault(u=> u.CategoryId==CategoryID);
            //Category CategoryFromDb2= _db.Categories.Where(u=>u.CategoryId==CategoryID).FirstOrDefault();   

            if (CategoryFromDb == null)
            {
                return NotFound();
            }


            return View(CategoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.category.update(obj);
                _unitOfWork.Save();
                TempData["Success"] = "Category Updated Successfully.";

                return RedirectToAction("Index", "Category");
            }

            return View();
        }

        public IActionResult Delete(int? CategoryID)
        {
            if (CategoryID == null || CategoryID == 0)
            {
                return NotFound();
            }
            Category CategoryFromDb = _unitOfWork.category.Get(u => u.CategoryId == CategoryID);
            //Category CategoryFromDb1 = _db.Categories.FirstOrDefault(u=> u.CategoryId==CategoryID);
            //Category CategoryFromDb2= _db.Categories.Where(u=>u.CategoryId==CategoryID).FirstOrDefault();   

            if (CategoryFromDb == null)
            {
                return NotFound();
            }
            return View(CategoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? CategoryId)
        {
            Category categoryobj = _unitOfWork.category.Get(u => u.CategoryId == CategoryId); ;
            if (categoryobj == null) { return NotFound(); }
            _unitOfWork.category.Remove(categoryobj);
            _unitOfWork.Save();
            TempData["Success"] = "Category Updated Successfully.";

            return RedirectToAction("Index", "Category");
        }
    }
}

