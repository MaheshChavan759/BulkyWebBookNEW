using BulkyWebBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.DataAccess.Repository.IRepository
{
    public interface IcategoryRepository:IRepository<Category>
    {
        void update(Category obj);
        //void save();
    }
}
