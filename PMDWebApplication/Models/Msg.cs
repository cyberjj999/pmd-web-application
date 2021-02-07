using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMDWebApplication.Models
{
    public class Msg
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }
        public string FromUser { get; set; }
        public DateTime DatePosted { get; set; }
    }
}