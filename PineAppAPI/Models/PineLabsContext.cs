using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PineAppAPI.Models
{
    public class PineLabsContext : DbContext
    {
        public PineLabsContext()
            : base("PineLabs")
        {
            this.Database.Connection.ConnectionString = ("data source=103.71.99.8;initial catalog=LivePineLabs;persist security info=True;user id=usrPine;password=P2345566;MultipleActiveResultSets=True;App=EntityFramework");
        }
    }
}