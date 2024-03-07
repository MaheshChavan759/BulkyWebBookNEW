using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyWebBook.Models
{
    public class OrderHeader
    {
        //Every Order have unique id
        public int Id { get; set; }
        //every order have user 
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippingDate { get; set; }
        public double OrderTotal { get; set; }
        //Each Order have order status
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        //Once Order shipping requied the TrackingNumber and carrier number 
        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }
        //For company user have 30 days for payment 
        public DateTime PaymentDate { get; set; }
        //dateonly and time only -- ef core generate date only 
        public DateOnly PaymentDueDate { get; set; }
        // for paymnet from credit card and things 

        // For sesstion create required session 

        public string? SessionId { get; set; } 
        public string? PaymentIntentId { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? StreetAddress { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

    }
}
