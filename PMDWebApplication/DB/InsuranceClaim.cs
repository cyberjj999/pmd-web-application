//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PMDWebApplication.DB
{
    using System;
    using System.Collections.Generic;
    
    public partial class InsuranceClaim
    {
        public int Id { get; set; }
        public string PolicyNo { get; set; }
        public string PlanType { get; set; }
        public string VictimName { get; set; }
        public string VictimNRIC { get; set; }
        public string NomineeName { get; set; }
        public string NomineeNRIC { get; set; }
        public string Relationship { get; set; }
        public string Address { get; set; }
        public string ContactNo { get; set; }
        public Nullable<System.DateTime> DateOfClaim { get; set; }
        public string UserID { get; set; }
        public byte[] Signature { get; set; }
        public System.Guid ClaimNo { get; set; }
        public string Status { get; set; }
        public string DenialMessage { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
    }
}