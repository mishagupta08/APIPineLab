using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class AuthCodeRequestParameter
    {
        public string clientId { get; set; }
        public string password { get; set; }
        public string username { get; set; }
    }

    public class AuthTokenRequestParameter
    {
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string authorizationCode { get; set; }
    }
}