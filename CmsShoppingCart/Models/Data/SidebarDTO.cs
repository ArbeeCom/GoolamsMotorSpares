using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Entity;
using System.ComponentModel;


namespace CmsShoppingCart.Models.Data
{ 
    [Table("tblSidebar")]
    public class SidebarDTO
    {
        [Key]
        public int Id{ get; set; }

        public string Body { get; set; }
    }
}