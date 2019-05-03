using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Common.Entities
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public string RatedUserId { get; set; }
        public virtual ApplicationUser RatedUser { get; set; }

        [Required]
        [Display(Name = "Comunicarea vanzatorului")]
        public int Communication { get; set; }

        [Required]
        [Display(Name = "Acuratetea anuntului")]
        public int Accuracy { get; set; }

        [Required]
        [Display(Name = "Timp livrare")]
        public int Time { get; set; }

        [Required]
        [Display(Name = "Medie")]
        public float Average { get; set; }

        [Display(Name = "Descrieti-va experienta (optional)")]
        [MinLength(10, ErrorMessage = "Descrierea este prea scurta! Va rugam adaugati detalii!")]
        [MaxLength(100, ErrorMessage = "Descrierea este prea lunga!")]
        public string Text { get; set; }

    }
}
