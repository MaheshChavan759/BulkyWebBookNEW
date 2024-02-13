using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //T-category 

        // For Return All Category 
        IEnumerable<T> GetAll(string? includeproperties = null);

        //For Return Single Category 

        T Get(Expression<Func<T, bool>> filter, string? includeproperties = null);

        void Add(T entity);

        // update and save changes need to perticular for type like category so not required implement globally.

        // void Update(T entity);
      //  void savechanges();
        void Remove(T entity); 
        void RemoveRange(IEnumerable<T> entity);





    }
}