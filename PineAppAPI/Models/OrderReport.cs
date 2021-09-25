using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class OrderReport
    {
        public string Created { get; set; }
        public string Validity { get; set; }
        public string Name { get; set; }
        public Int32 Id { get; set; }
        public string Status { get; set; }
        public string OrderID { get; set; }
        public string Userid { get; set; }
        public decimal Amount { get; set; }
        public string EnrptCardNo { get; set; }
        public string EnrptCardPin { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
         public string refno { get; set; }
        public string UserName { get; set; }
        public string CompanyUserName { get; set; }
    }
    public class MyOrderDetail
    {
        public string Validity { get; set; }
        public string EnrptCardNo { get; set; }
        public string EnrptCardPin { get; set; }
        public string ProductName { get; set; }
        public string ProductSku { get; set; }
        public decimal Amount { get; set; }
    }
}