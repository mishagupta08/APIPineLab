using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class TokenResponce
    {
        public string authorizationCode { get; set; }

        public string token { get; set; }
    }
}