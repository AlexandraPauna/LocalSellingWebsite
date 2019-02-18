using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Licenta.Models.Data
{
    public class DeliveryCompany
    {
        [Key]
        public int DeliveryCompanyId { get; set; }

        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Numele companiei este obligatoriu!")]
        [Display(Name = "Nume Companie curierat")]
        public string DeliveryCompanyName { get; set; }
    }
}