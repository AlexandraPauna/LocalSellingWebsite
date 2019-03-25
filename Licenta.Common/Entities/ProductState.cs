using System.ComponentModel.DataAnnotations;

namespace Licenta.Common.Entities
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