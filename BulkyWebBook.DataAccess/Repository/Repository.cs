using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BulkyWebBook.DataAccess.Data;
using BulkyWebBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;


namespace BulkyWebBook.DataAccess.Repository
{
    
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;

        internal DbSet<T> dbset;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbset=_db.Set<T>();
            //_db.Categories == dbset;

            _db.Products.Include(u => u.Category).Include(u=>u.CategoryId);
        }
        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeproperties = null)
        {
            IQueryable<T> query = dbset;
            query.Where(filter);
            if (!string.IsNullOrEmpty(includeproperties))
            {
                foreach (var item in includeproperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))

                {
                    query = query.Include(item);
                }
            }

            return query.FirstOrDefault();
        }


        //Category , CoverType User want to include
        public IEnumerable<T> GetAll( string? includeproperties = null)
        {
            IQueryable<T> query = dbset;
            if(!string.IsNullOrEmpty(includeproperties))
            {
                foreach (var item in includeproperties
                    .Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries))
                    
                {
                    query=query.Include(item);
                }
            }

             return query.ToList();
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbset.RemoveRange(entity);
        }
    }
}
