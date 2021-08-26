using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class Cancel
    {
        public bool allowed { get; set; }
        public string allowedWithIn { get; set; }
    }

    public class Payment
    {
        public string balance { get; set; }
        public string code { get; set; }
    }

    public class Labels
    {
        public string activationCode { get; set; }
        public string cardNumber { get; set; }
        public string cardPin { get; set; }
        public string validity { get; set; }
    }

    public class Sms
    {
        public string reason { get; set; }
        public string status { get; set; }
    }

    public class Status
    {
        public Sms email { get; set; }
        public Sms sms { get; set; }
    }

    public class Delivery
    {
        public string mode { get; set; }
        public Status status { get; set; }
    }

    public class RecipientDetails
    {
        public Delivery delivery { get; set; }
        public string email { get; set; }
        public string failureReason { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public object mobileNumber { get; set; }
        public string name { get; set; }
        public string status { get; set; }
    }

    public class Card
    {
        public object activationUrl { get; set; }
        public object activationCode { get; set; }
        public string amount { get; set; }
        public object barcode { get; set; }
        public int cardId { get; set; }
        public string cardNumber { get; set; }
        public string cardPin { get; set; }
        public Labels labels { get; set; }
        public string productName { get; set; }
        public RecipientDetails recipientDetails { get; set; }
        public string sku { get; set; }
        public string theme { get; set; }
        public string validity { get; set; }
    }

    public class OderProductResponce
    {
        //public EGCGBSHR001 EGCGBSHR001 { get; set; }

        public string sku { get; set; }
        public string name { get; set; }
        public object balanceEnquiryInstruction { get; set; }
        public string specialInstruction { get; set; }
        public Images images { get; set; }
        public string cardBehaviour { get; set; }
    }

    public class OderProduct
    {
        List<object> productDetail { get; set; }
    }

    public class OrderRoot
    {
        public string status { get; set; }
        public string orderId { get; set; }
        public string refno { get; set; }
        public Cancel cancel { get; set; }
        public Currency currency { get; set; }
        public List<Payment> payments { get; set; }
        public List<Card> cards { get; set; }
        public OderProduct products { get; set; }
        public List<object> additionalTxnFields { get; set; }
    }
}