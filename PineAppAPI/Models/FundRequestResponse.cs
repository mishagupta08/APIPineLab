using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class FundRequestResponse
    {
        public Int32 ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public decimal Amount { get; set; }
        public string RequestDate  { get; set; }
        public string RequestBy { get; set; }
        public string HostIP { get; set; }
        public bool IsFundApprove { get; set; }
        public Int32 RequestedUserId { get; set; }
        public Int32 RequestedToUserId { get; set; }
        public string Mobile { get; set; }
        public string Remark { get; set; }
    }
    public class FundRequest
    {
        public Int32 ID { get; set; }
        public Int32 UserID { get; set; }
        public decimal Amount { get; set; }
        public Int32 CreatedBy { get; set; }
        public Int32 ModeID { get; set; }        
        public string Remark { get; set; }
        public string HostIP { get; set; }
    }
}