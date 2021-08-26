using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class Currency
    {
        public string code { get; set; }
        public string numericCode { get; set; }
        public string sku { get; set; }        
        public string symbol { get; set; }        
    }

    public class Product
    {
        public string categoryId { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public Currency currency { get; set; }
        public string url { get; set; }
        public string minPrice { get; set; }
        public string maxPrice { get; set; }
        public Images images { get; set; }
        public productname CNPIN { get;set;}

    }

     public class productname
    {
        public string sku { get; set; }
        public string name { get; set; }
         public string balanceEnquiryInstruction { get; set; }
        public string specialInstruction { get; set; }
        public Images images { get; set; }
        public string cardBehaviour { get; set; }
    }

    public class ProductListContainer
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public object description { get; set; }
        public Images images { get; set; }
        public int productsCount { get; set; }
        public List<Product> products { get; set; }
    }

    public class Price
    {
        public string price { get; set; }
        public string type { get; set; }
        public decimal min { get; set; }
        public decimal max { get; set; }
        public List<string> denominations { get; set; }
        public Currency currency { get; set; }
    }

    public class Page
    {
        public object title { get; set; }
    }

    public class Meta
    {
        public object title { get; set; }
        public object keywords { get; set; }
        public object description { get; set; }
    }

    public class Canonical
    {
        public object url { get; set; }
    }

    public class MetaInformation
    {
        public Page page { get; set; }
        public Meta meta { get; set; }
        public Canonical canonical { get; set; }
    }

    public class Images
    {
        public string sku { get; set; }
        public string thumbnail { get; set; }
        public string mobile { get; set; }
        public string @base { get; set; }
        public string small { get; set; }
        public string image { get; set; }
    }

    public class Tnc
    {
        public string link { get; set; }
        public string content { get; set; }
    }

    public class Discount
    {
        public string type { get; set; }
        public int amount { get; set; }
        public string desc { get; set; }
    }

    public class Coupon
    {
        public string code { get; set; }
    }

    public class Discounts
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public Discount discount { get; set; }
        public Coupon coupon { get; set; }
        public int priority { get; set; }
    }

    public class ProductDetail
    {
        public string id { get; set; }
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Price price { get; set; }
        public string kycEnabled { get; set; }
        public object additionalForm { get; set; }
        public MetaInformation metaInformation { get; set; }
        public string type { get; set; }
        public bool schedulingEnabled { get; set; }
        public string currency { get; set; }
        public Images images { get; set; }
        public Tnc tnc { get; set; }
        public List<string> categories { get; set; }
        public List<Theme> themes { get; set; }
        public List<object> handlingCharges { get; set; }
        public bool reloadCardNumber { get; set; }
        public object expiry { get; set; }
        public string formatExpiry { get; set; }
        public List<Discounts> discounts { get; set; }
        public List<RelatedProduct> relatedProducts { get; set; }
        public object storeLocatorUrl { get; set; }
    }

    public class RelatedProduct
    {
        public string sku { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public Images images { get; set; }
    }

    public class Theme
    {
        public string sku { get; set; }
        public string price { get; set; }
        public string image { get; set; }
    }
}