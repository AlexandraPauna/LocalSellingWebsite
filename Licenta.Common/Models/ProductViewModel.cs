using System.Collections.Generic;
using Licenta.Common.Entities;
using PagedList;

namespace Licenta.Common.Models
{
    public class ProductViewModel
    {
        public IPagedList<Product> Products { get; set; }
    }
}