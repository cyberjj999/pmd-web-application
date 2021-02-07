using PMDWebApplication.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMDWebApplication.Models
{
    public class PurchaseInsuranceViewModel
    {
        [Key]
        public int InsuranceID { get; set; }
        public string InsuranceName { get; set; }

        public string InsuranceType { get; set; }

        public int Price { get; set; }


    }

    public class ClaimInsuranceViewModel
    {
        [Key]
        public int Id { get; set; }

        public Guid ClaimNo { get; set; }

        [Required]
        public string InsuranceNo { get; set; }

        [Required]
        public string PlanType { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Invalid Victim NRIC!")]
        public string VictimNRIC { get; set; }

        [Required]
        public string Nominee { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Invalid Claimant NRIC!")]
        public string NomineeNRIC { get; set; }

        [Required]
        public string Relationship { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Invalid Claimant NRIC!")]
        public string PhoneNumber { get; set; }

        [Required]
        public DateTime? Date { get; set; }

        [UIHint("SignaturePad")]
        public byte[] Signature { get; set; }

        public string DenialMessage { get; set; }

        public string Status { get; set; }
    }

}