using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Common.Entities
{
    public class Statistic
    {
        [Key]
        public int StatisticId { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required]
        public int SubCategoryId { get; set; }
        public virtual SubCategory SubCategory { get; set; }

        [Required]
        public int ViewCounter { get; set; }
    }
}
