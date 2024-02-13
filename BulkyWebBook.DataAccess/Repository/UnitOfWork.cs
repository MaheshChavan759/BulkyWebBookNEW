using BulkyWebBook.DataAccess.Data;
using BulkyWebBook.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public IcategoryRepository category {  get; private set; }
        public IProductRepository Product { get; private set; }


        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            
            category =new CategoryRepository(_db);
            Product = new ProductRepository(_db);


        }

        public void Save()
        {
           _db.SaveChanges();
        }
    }
}
