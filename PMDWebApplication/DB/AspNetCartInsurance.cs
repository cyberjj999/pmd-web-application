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
    
    public partial class AspNetCartInsurance
    {
        public int CartId { get; set; }
        public Nullable<int> InsuranceId { get; set; }
        public string MemberId { get; set; }
        public Nullable<int> CartStatusId { get; set; }
        public System.Guid PolicyId { get; set; }
        public string PlanType { get; set; }
    
        public virtual AspNetCartInsurance AspNetCartInsurance1 { get; set; }
        public virtual AspNetCartInsurance AspNetCartInsurance2 { get; set; }
        public virtual AspNetCartInsurance AspNetCartInsurance11 { get; set; }
        public virtual AspNetCartInsurance AspNetCartInsurance3 { get; set; }
    }
}
