//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Ichup.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class image
    {
        public long id { get; set; }
        public string keywords { get; set; }
        public Nullable<int> cat_id { get; set; }
        public string link { get; set; }
        public string link_thumbail_big { get; set; }
        public string link_thumbail_small { get; set; }
        public string link_big { get; set; }
        public string link_small { get; set; }
        public Nullable<int> total_views { get; set; }
        public Nullable<int> total_buy { get; set; }
        public Nullable<int> total_download { get; set; }
        public Nullable<int> total_download_big { get; set; }
        public Nullable<int> total_download_small { get; set; }
        public Nullable<int> member_id { get; set; }
        public Nullable<System.DateTime> date_post { get; set; }
        public string filter_1 { get; set; }
        public string filter_2 { get; set; }
        public string filter_3 { get; set; }
        public string filter_4 { get; set; }
        public string filter_5 { get; set; }
        public Nullable<decimal> price { get; set; }
        public string token { get; set; }
        public Nullable<int> stt { get; set; }
        public string code { get; set; }
        public Nullable<byte> status { get; set; }
        public Nullable<int> width { get; set; }
        public Nullable<int> height { get; set; }
        public Nullable<byte> sale_type { get; set; }
    }
}
