using Licenta.Common.Entities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Common.Models
{
    public class InterestViewModel
    {
        public IPagedList<Interest> Interests { get; set; }
    }
}
