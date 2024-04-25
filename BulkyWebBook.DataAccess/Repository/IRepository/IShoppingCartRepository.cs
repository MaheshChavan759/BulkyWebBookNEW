using BulkyWebBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void update(ShoppingCart obj);
        //void save();

        int Count(Expression<Func<ShoppingCart, bool>> filter);
    }
}
