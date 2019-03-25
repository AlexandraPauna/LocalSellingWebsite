using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Licenta.Common.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Numele categoriei este obligatoriu!")]
        [Display(Name = "Nume Categorie")]
        public string CategoryName { get; set; }

        [Display(Name = "Poza Categorie")]
        public byte[] CategoryPhoto { get; set; }

        //public virtual ICollection<Product> Product { get; set; }

        public virtual ICollection<SubCategory> SubCategory { get; set; }
    }
}