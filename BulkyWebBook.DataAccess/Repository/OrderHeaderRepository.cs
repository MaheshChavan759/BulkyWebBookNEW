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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        //public void save()
        //{
        //    _db.SaveChanges();  
        //}

        public void update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }
        //bases on the id both retrive from database
        //if the orderfromdb is not null then update the order status  and if payment status not null update the paymnet status 
        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(orderStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
        }
        // session id generated when user triyes o payment 
        // if session genaerated  successful then a pyamnetintent id generated 
        public void UpdateStripePaymentId(int id, string sessionId, string stripePaymentId)
        {
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(stripePaymentId))
            {
                orderFromDb.PaymentIntentId = stripePaymentId;
                orderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
