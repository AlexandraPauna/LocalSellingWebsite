using Licenta.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Common.Models
{
    public class RatingViewModel
    {
        //public ApplicationUser RatedUser { get; set; }
        public IList<Rating> Ratings { get; set; }
    }
}
