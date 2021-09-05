using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class OrderContainer
    {
        public BillingAddress billingAddress { get; set; }

        public decimal UserId { get; set; }
        public string orderId { get; set; }
    }
}