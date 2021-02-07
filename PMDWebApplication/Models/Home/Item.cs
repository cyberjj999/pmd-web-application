using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PMDWebApplication.DB;

namespace PMDWebApplication.Models.Home
{
    public class Item
    {
        public AspNetProduct Product { get; set; }

        public int Quantity { get; set; }
    }
}