using Licenta.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Common.Models
{
    public class UserProfileViewModel
    {
        [Key]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public IList<Product> Products { get; set; }

        public IList<Rating> Ratings { get; set; }

    }
}
