using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Licenta.Common.Entities
{
    public class SubCategory
    {
        [Key]
        public int SubCategoryId { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Numele subcategoriei este obligatoriu!")]
        [Display(Name = "Nume Subcategorie")]
        public string SubCategoryName { get; set; }

        public virtual ICollection<Product> Product { get; set; }

        [Required(ErrorMessage = "Selectati categoria!")]
        [Display(Name = "Categorie")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}