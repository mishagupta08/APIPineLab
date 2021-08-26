using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class Filters
    {
        public int Id { get; set; }

        public string Operation { get; set; }

        public string Action { get; set; }

        public string Sku { get; set; }

        public int pageIndex { get; set; }

        public int RecordCount { get; set; }

        public int CompanyId { get; set; }
    }
}