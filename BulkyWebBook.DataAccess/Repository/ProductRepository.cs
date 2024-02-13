using BulkyWebBook.DataAccess.Data;
using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        //public void save()
        //{
        //    _db.SaveChanges();  
        //}

        public void update(Product obj)
        {
            var ObjFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);
            if (ObjFromDb != null)
            {
                ObjFromDb.Title = obj.Title;
                ObjFromDb.Description = obj.Description;
                ObjFromDb.Author = obj.Author;
                ObjFromDb.ListPrise = obj.ListPrise;
                ObjFromDb.Prise50 = obj.Prise50;
                ObjFromDb.Prise100 = obj.Prise100;
                ObjFromDb.CategoryId = obj.CategoryId;

                if (ObjFromDb.ImageUrl != null)
                {
                    ObjFromDb.ImageUrl = obj.ImageUrl;
                }

            }
        }
    }
}
