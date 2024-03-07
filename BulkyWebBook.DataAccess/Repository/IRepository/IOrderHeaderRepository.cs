using BulkyWebBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void update(OrderHeader obj);
        //void save();
        // We waant to  update order status and paymentstatus based on Id 
        //order status required and paytional status Optional 

        void UpdateStatus(int id , string orderStatus, string? paymentStatus = null);

        //based on orderheader id we can update stripe 

        void UpdateStripePaymentId(int id , string sessionId, string stripePaymentId);
    }
}
