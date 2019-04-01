using Licenta.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Licenta.Common.Models
{
    public class SubCategoryViewModel
    {
        [Key]
        public int SubCategoryId { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Numele subcategoriei este obligatoriu!")]
        [Display(Name = "Nume Subcategorie")]
        public string SubCategoryName { get; set; }

        [Required(ErrorMessage = "Selectati categoria!")]
        [Display(Name = "Categorie")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public IList<Product> Products { get; set; }
    }
}
