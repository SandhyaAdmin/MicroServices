﻿using System.ComponentModel.DataAnnotations;

namespace Mango.Services.OrderAPI.Models
{
    public class OrderHeader
    {
        [Key]
        public int OrderHeaderId { get; set; }
        public string? UserId { get; set; } 
        public string? CouponCode { get; set; }
        public double Discount { get; set; }
        public double OrderTotal { get; set; }


        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email{ get; set; }

        public DateTime OrderTime { get; set; }
        public string? Status { get; set; }


        // Payment gateway related fields:
        public string? PaymentIntentId { get; set; }    
        public string? StripeSessionId { get; set; }    

        public IEnumerable<OrderHeader>? OrderHeaders { get; set; }

    }
}