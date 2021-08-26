using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class RequestModel
    {
        public int CompanyId { get; set; }

        public int CategoryId { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        public string Sku { get; set; }

        public string OrderRefno { get; set; }

        public OrderRequestModel orderRequestDetail { get; set; }
         public string orderId { get; set; }
    }
}