//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PineAppAPI.Models
{
    using System;
    
    public partial class GetEmailQue_Result
    {
        public int ID { get; set; }
        public Nullable<int> Formno { get; set; }
        public string Body { get; set; }
        public string EmailType { get; set; }
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public Nullable<bool> IsMailSent { get; set; }
        public Nullable<int> CompanyID { get; set; }
        public string Subject { get; set; }
        public string SMTPServer { get; set; }
        public string SMSUserName { get; set; }
        public string SMSPassword { get; set; }
    }
}
