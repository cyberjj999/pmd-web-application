using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMDWebApplication.Models
{
    public class RoleViewModel
    {
        //To display list of roles
        public IEnumerable<string> RoleNames { get; set; }
        public string UserName { get; set; }
        //
        public RoleViewModel() { }
        public RoleViewModel(ApplicationRole role)
        {
            Id = role.Id;
            Name = role.Name;
     
        }
        
        public string Id { get; set; }
        public string Name { get; set; }

        //To store lockout end date
        public DateTime LockoutEndDate { get; set; }

        public string Email { get; set; }

    }





}