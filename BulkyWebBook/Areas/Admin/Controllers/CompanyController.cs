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
   // [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
       
        public CompanyController(IUnitOfWork unitofwork)
        {
            _unitOfWork = unitofwork;
            
        }
        public IActionResult Index()
        {
            List<Company> CompanyList = _unitOfWork.Company.GetAll().ToList();

            //return Json(new { data = CompanyList });
           // return View(CompanyList);

            return View(CompanyList);
        }
        public IActionResult Upsert(int? id)
        {
            

            if (id == null || id == 0)
            {
                //Create functionality 
                return View(new Company());
            }
            else
            {
                // For Update 

                //Company p = _unitOfWork.Company.Get(u => u.Id == id);
                //List<Company> prods = (List<Company>)_unitOfWork.Company.GetAll();
                // Company CompanyObj = prods.FirstOrDefault(b => b.Id == id);
                //return View(CompanyObj);



                //Company company = _unitOfWork.Company.Get(u => u.Id == id);


                List<Company> comp = (List<Company>)_unitOfWork.Company.GetAll();

                Company companyobj = comp.FirstOrDefault(u=>u.Id == id);


                return View(companyobj);
         


                //return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company companyobj)
        {
            if (ModelState.IsValid)
            {
                


                if (companyobj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyobj);
                }

                else
                {
                    _unitOfWork.Company.update(companyobj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Company Created Successfully.";
                return RedirectToAction("Index", "Company");
            }
            else
            {
                
                return View(companyobj);
            }
        }



        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> obj = _unitOfWork.Company.GetAll().ToList();

            return Json(new { data = obj }); 
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var CompanyToDelete = _unitOfWork.Company.Get(u => u.Id == id);

            if(CompanyToDelete == null) 
            {
                return Json(new {success = false,message = "Error While Deleting"});
            }
            // if Above is valid need to delete image also 
            
            _unitOfWork.Company.Remove(CompanyToDelete);

            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}

