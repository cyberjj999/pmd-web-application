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
    
    public partial class AspNetInsurance
    {
        public int InsuranceId { get; set; }
        public string InsuranceName { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> CategoryId { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public string InsuranceImage { get; set; }
    }
}