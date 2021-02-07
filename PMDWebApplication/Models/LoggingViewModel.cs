using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMDWebApplication.Models
{
    public class LoggingViewModel
    {
        [Key]
        public int Id { get; set; }

        public string Date { get; set; }

        public string Message { get; set; }

        public string Type { get; set; }

        public string Roles { get; set; }

        public string Username { get; set; }

        public string ControllerName { get; set; }



    }
}