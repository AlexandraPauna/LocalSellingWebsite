using Licenta.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Controllers
{
    public class SearchController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Search
        public ActionResult Index(string search)
        {
           // TempData["Search"] = search;
            ViewBag.NoResult = true;
            ViewBag.Products = null;
            ViewBag.Search = search;

            if (!String.IsNullOrEmpty(search))
            {
                var products = db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User").OrderByDescending(a => a.Date);

                var subCatId = (from subcategs in db.SubCategories
                                where subcategs.SubCategoryName.Contains(search)
                                select subcategs.SubCategoryId).FirstOrDefault();

                var cityId = (from cities in db.Cities
                              where cities.CityName.Contains(search)
                              select cities.CityId).FirstOrDefault();
          
                var stateId = (from states in db.ProductState
                               where states.ProductStateName.Contains(search)
                               select states.ProductStateId).FirstOrDefault();

                var deliveryCompId = (from companies in db.DeliveryCompanies
                                     where companies.DeliveryCompanyName.Contains(search)
                                     select companies.DeliveryCompanyId).FirstOrDefault();

                var catId = (from categs in db.Categories
                             where categs.CategoryName.Contains(search)
                             select categs.CategoryId).FirstOrDefault();
                
                //when a category name is typed inside the search box, subcategories coresponding to that category are being searched
                var subCatsIdOfCat = (from subcategs in db.SubCategories
                                      where subcategs.CategoryId.Equals(catId)
                                      select subcategs.SubCategoryId).ToList();


                ViewBag.Products = products.Where(s => s.Title.Contains(search) ||
                                                       s.Description.Contains(search) ||
                                                       s.SubCategoryId == subCatId ||
                                                       s.CityId == cityId ||
                                                       s.ProductStateId == stateId ||
                                                       s.DeliveryCompanyId == deliveryCompId ||
                                                       subCatsIdOfCat.Any(x => x == s.SubCategoryId)
                                                  );
                if (ViewBag.Products != null)
                {
                    ViewBag.NoResult = false;
                }


            }
           
            return View();
        }

        [HttpPost]
        public ActionResult Index(string search, string sortType)
        {
           
            ViewBag.NoResult = true;
            ViewBag.Products = null;
            ViewBag.Search = search;

            if (!String.IsNullOrEmpty(search))
            {
                //var products = db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User").OrderByDescending(a => a.Date);

                var subCatId = (from subcategs in db.SubCategories
                                where subcategs.SubCategoryName.Contains(search)
                                select subcategs.SubCategoryId).FirstOrDefault();

                var cityId = (from cities in db.Cities
                              where cities.CityName.Contains(search)
                              select cities.CityId).FirstOrDefault();

                var stateId = (from states in db.ProductState
                               where states.ProductStateName.Contains(search)
                               select states.ProductStateId).FirstOrDefault();

                var deliveryCompId = (from companies in db.DeliveryCompanies
                                      where companies.DeliveryCompanyName.Contains(search)
                                      select companies.DeliveryCompanyId).FirstOrDefault();

                var catId = (from categs in db.Categories
                             where categs.CategoryName.Contains(search)
                             select categs.CategoryId).FirstOrDefault();

                //when a category name is typed inside the search box, subcategories coresponding to that category are being searched
                var subCatsIdOfCat = (from subcategs in db.SubCategories
                                      where subcategs.CategoryId.Equals(catId)
                                      select subcategs.SubCategoryId).ToList();


                var products = db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User")
                                          .Where(s => s.Title.Contains(search) ||
                                                       s.Description.Contains(search) ||
                                                       s.SubCategoryId == subCatId ||
                                                       s.CityId == cityId ||
                                                       s.ProductStateId == stateId ||
                                                       s.DeliveryCompanyId == deliveryCompId ||
                                                       subCatsIdOfCat.Any(x => x == s.SubCategoryId)
                                                  ).OrderByDescending(a => a.Date); ;
                if (sortType == "Title")
                    products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").OrderBy(x => x.Title);
                else
                if (sortType == "Date")
                    products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").OrderByDescending(x => x.Date);
                else
                if (sortType == "PriceAsc")
                    products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").OrderBy(x => x.Price);
                else
                if (sortType == "PriceDesc")
                    products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").OrderByDescending(x => x.Price);

                ViewBag.Products = products;

                if (ViewBag.Products != null)
                {
                    ViewBag.NoResult = false;
                }


            }

            return View();
        }


    }
}