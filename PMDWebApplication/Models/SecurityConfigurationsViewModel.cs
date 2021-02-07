using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PMDWebApplication.Models
{
    //Needed to map table to model
    [Table("SecurityConfigurations")]
    public class SecurityConfigurationsViewModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter Minimum Password Length")]
        [Display(Name = "Minimum Password Length")]
        public int Password_Length { get; set; }

        [Required(ErrorMessage = "Please enter Maximum Password Age")]
        [Display(Name = "Maximum Password Age (Days)")]
        public int PasswordExpiry { get; set; }

        [Display(Name = "Require Non Letter/Digit")]
        public bool RequireNonLetterOrDigit { get; set; }

        [Display(Name = "Require Digit")]
        public bool RequireDigit { get; set; }

        [Display(Name = "Require Lowercase")]
        public bool RequireLowercase { get; set; }

        [Display(Name = "Require Uppercase")]
        public bool RequireUppercase { get; set; }

        [Required(ErrorMessage = "Please enter Number of Password Attempts Allowed")]
        [Display(Name = "Number of Password Attempts Allowed")]
        public int Password_Attempts { get; set; }

        [Required(ErrorMessage = "Please enter Lockout Duration")]
        [Display(Name = "Lockout Duration (Minutes)")]
        public int LockoutDuration { get; set; }

        [Required(ErrorMessage = "Please enter Inactive Duration")]
        [Display(Name = "Inactive Duration (Minutes)")]
        public int InactiveDuration { get; set; }

    }
}