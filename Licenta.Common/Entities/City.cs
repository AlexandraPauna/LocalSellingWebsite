using System.ComponentModel.DataAnnotations;

namespace Licenta.Common.Entities
{
    public class City
    {
        [Key]
        public int CityId { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu")]
        [Display(Name = "Nume Oras")]
        public string CityName { get; set; }
    }
}