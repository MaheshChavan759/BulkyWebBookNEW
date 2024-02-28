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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }
        //public void save()
        //{
        //    _db.SaveChanges();  
        //}

        public void update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
