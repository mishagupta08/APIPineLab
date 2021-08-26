using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{

    public class Address
    {
        public bool billToThis { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string postcode { get; set; }
        public string region { get; set; }
        public string telephone { get; set; }
    }

    public class Billing
    {
        public string city { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string postcode { get; set; }
        public string region { get; set; }
        public string telephone { get; set; }
    }

   
    public class Product1
    {
        public int currency { get; set; }
        public string giftMessage { get; set; }
        public int price { get; set; }
        public int qty { get; set; }
        public string sku { get; set; }
        public string theme { get; set; }
    }

    public class OrderRequestModel
    {
        public Address address { get; set; }
        public Billing billing { get; set; }
        public string couponCode { get; set; }
        public string deliveryMode { get; set; }
        public List<Payment1> payments { get; set; }
        public List<Product1> products { get; set; }
        public string refno { get; set; }        
        public bool syncOnly { get; set; }
    }

    public class Payment1
    {
        public string amount { get; set; }
        public string code { get; set; }
    }
}