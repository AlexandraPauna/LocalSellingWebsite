using Licenta.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Common.Models
{
    public class CategoryViewModel
    {
        [Key]
        public int CategoryId { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Numele categoriei este obligatoriu!")]
        [Display(Name = "Nume Categorie")]
        public string CategoryName { get; set; }

        [Display(Name = "Poza Categorie")]
        public byte[] CategoryPhoto { get; set; }

        public IList<SubCategory> SubCategories { get; set; }
    }
}
