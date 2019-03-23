using Licenta.Models;
using Licenta.Models.Categories;
using Licenta.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Poze Produs")]
        //public List<byte[]> ProductPhotos { get; set; }
        public virtual ICollection<ProductImage> ProductImages { set; get; }

        [Required(ErrorMessage = "Introduceti titlul articolului!")]
        [Display(Name = "Titlu")]
        [StringLength(70, ErrorMessage = "Titlul este prea lung")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Introduceti pretul!")]
        [Display(Name = "Pret")]
        public float Price { get; set; }

        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Adaugati descrierea produsului!")]
        [MinLength(30, ErrorMessage = "Descrierea este prea scurta! Va rugam adaugati detalii!")]
        [Display(Name = "Descriere")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Va rugam selectati orasul!")]
        [Display(Name = "Oras")]
        public int CityId { get; set; }
        public virtual City City { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }

        /*[Required(ErrorMessage = "Va rugam selectati categoria!")]
        [Display(Name = "Categorie")]
        public int? CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        */
        [Required(ErrorMessage = "Va rugam selectati subcategoria!")]
        [Display(Name = "Subcategorie")]
        public int SubCategoryId { get; set; }
        public virtual SubCategory SubCategory { get; set; }
        public IEnumerable<SelectListItem> SubCategories { get; set; }

        [Required(ErrorMessage = "Va rugam selectati starea produsului!")]
        [Display(Name = "Starea produsului")]
        public int ProductStateId { get; set; }
        public virtual ProductState ProductState { get; set; }
        public IEnumerable<SelectListItem> ProductStateTypes { get; set; }

        [Display(Name = "Site")]
        public string Site { get; set; }

        [Display(Name = "Predare personala")]
        public bool PersonalDelivery { get; set; }

        [Display(Name = "Modalitate de Livrare")]
        public int? DeliveryCompanyId { get; set; }
        public virtual DeliveryCompany DeliveryCompany { get; set; }
        public IEnumerable<SelectListItem> DeliveryCompanies { get; set; }

        [Display(Name = "Cost Livrare")]
        public float? DeliveryPrice { get; set; }

        [Display(Name = "Detalii Livrare")]
        public string DeliveryDetails { get; set; }

        [Display(Name = "Politica Retur")]
        public string ReturnPolicy { get; set; }

        [Display(Name = "Garantie")]
        public string Warranty { get; set; }
    }

    public class ProductImage
    {
        public int Id { set; get; }
        public byte[] ImageData { set; get; }
        public int ProductId { set; get; }
    }
}