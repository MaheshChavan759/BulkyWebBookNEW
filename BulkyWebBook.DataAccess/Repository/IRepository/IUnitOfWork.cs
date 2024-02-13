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

        void Save();
    }
}
