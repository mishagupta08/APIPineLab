using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    //public class Images
    //{
    //    public object image { get; set; }
    //    public object thumbnail { get; set; }
    //}

    public class Subcategory
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public object description { get; set; }
        public Images images { get; set; }
        public int subcategoriesCount { get; set; }
    }

    public class CategoryResponce
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public object description { get; set; }
        public Images images { get; set; }
        public int subcategoriesCount { get; set; }
        public List<Subcategory> subcategories { get; set; }
    }


}