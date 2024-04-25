using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork 
    {
        IcategoryRepository category { get; }
        IProductRepository Product { get; }
       ICompanyRepository  Company { get; }
        IShoppingCartRepository ShoppingCart { get; }

        IApplicationUserRepository ApplicationUser { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }

        void Save();

        void Detach<T>(T entity) where T : class;
    }
}
