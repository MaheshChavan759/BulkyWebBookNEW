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
    public class CategoryRepository : Repository<Category>,IcategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }
        //public void save()
        //{
        //    _db.SaveChanges();  
        //}

        public void update(Category obj)
        {
            _db.Categories.Update(obj);
        }
    }
}
