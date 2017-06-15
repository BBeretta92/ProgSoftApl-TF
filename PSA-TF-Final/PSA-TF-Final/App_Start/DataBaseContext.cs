using PSA_TF_Final.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;


namespace PSA_TF_Final.App_Start
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext() : base()
        { }
        public DbSet<DAOTicket> Tickets { get; set; }
    }
}