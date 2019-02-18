using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Licenta.Models.Categories
{
    public class SubCategory
    {
        [Key]
        public int SubCategoryId { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Numele subcategoriei este obligatoriu!")]
        [Display(Name = "Nume Subcategorie")]
        public string SubCategoryName { get; set; }

        public int CategoryId { get; set; }

        public virtual ICollection<Product> Product { get; set; }

    }
}