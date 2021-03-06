﻿using System.ComponentModel.DataAnnotations;

namespace Licenta.Common.Entities
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