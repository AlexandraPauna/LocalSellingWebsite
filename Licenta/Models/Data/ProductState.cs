using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Licenta.Models.Data
{
    public class ProductState
    {
        [Key]
        public int ProductStateId { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Numele starii produsului este obligatoriu!")]
        [Display(Name = "Denumire stare produs")]
        public string ProductStateName { get; set; }
    }
}