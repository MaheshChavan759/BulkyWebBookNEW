using BulkyWebBook.DataAccess.Data;
using BulkyWebBook.DataAccess.Repository.IRepository;
using BulkyWebBook.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }
        //public void save()
        //{
        //    _db.SaveChanges();  
        //}
        public void update(ShoppingCart obj)
        {
            _db.shoppingCarts.Update(obj);
        }

        public int Count(Expression<Func<ShoppingCart, bool>> filter)
        {
            return _db.Set<ShoppingCart>().Count(filter);
        }
    }
}
