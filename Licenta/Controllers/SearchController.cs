using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Licenta.Common.Models;
using Licenta.DataAccess;

namespace Licenta.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Search
        public ActionResult Index(string search, int? categoryId, DateTime? dateMin, int? fromCity, float? priceMin, float? priceMax, int? state, string sortType)
        {
            ViewBag.Cities = GetAllCities(); //used to load cities in dropdown
            ViewBag.ProductStates = GetAllProductStates(); 

            // TempData["Search"] = search;
            ViewBag.NoResult = true;
            //ViewBag.Products = null;
            var model = new ProductViewModel { Products = null };
            ViewBag.Search = search;

            if (!String.IsNullOrEmpty(search) || fromCity!=null || categoryId!=null)
            {
                //var products = db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User");

                var subCatId = (from subcategs in _db.SubCategories
                                where subcategs.SubCategoryName.Contains(search)
                                select subcategs.SubCategoryId).FirstOrDefault();

                var cityId = (from cities in _db.Cities
                              where cities.CityName.Contains(search)
                              select cities.CityId).FirstOrDefault();

                var stateId = (from states in _db.ProductState
                               where states.ProductStateName.Contains(search)
                               select states.ProductStateId).FirstOrDefault();

                var deliveryCompId = (from companies in _db.DeliveryCompanies
                                      where companies.DeliveryCompanyName.Contains(search)
                                      select companies.DeliveryCompanyId).FirstOrDefault();

                var catId = (from categs in _db.Categories
                             where categs.CategoryName.Contains(search)
                             select categs.CategoryId).FirstOrDefault();

                //when a category name is typed inside the search box, subcategories coresponding to that category are being searched
                var subCatsIdOfCat = (from subcategs in _db.SubCategories
                                      where subcategs.CategoryId.Equals(catId)
                                      select subcategs.SubCategoryId).ToList();

                var subCatsIdOfCatForm = (from subcategs in _db.SubCategories
                                      where subcategs.CategoryId == categoryId
                                      select subcategs.SubCategoryId).ToList();

                var products = _db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User")
                                          .Where(s => s.Title.Contains(search) ||
                                                      s.Description.Contains(search) ||
                                                      s.SubCategoryId == subCatId ||
                                                      s.CityId == cityId ||
                                                      s.CityId == fromCity ||
                                                      subCatsIdOfCatForm.Any(x => x == s.SubCategoryId) ||
                                                      s.ProductStateId == stateId ||
                                                      s.DeliveryCompanyId == deliveryCompId ||
                                                      subCatsIdOfCat.Any(x => x == s.SubCategoryId));

                products = products.Where(p => p.Active == true);
                //Filtrarea
                if (priceMin != null)
                    products = products.Where(s => s.Price >= priceMin);
                if (priceMax != null)
                    products = products.Where(s => s.Price <= priceMax);
                if (fromCity != null)
                    products = products.Where(s => s.CityId == fromCity);
                if (state != null)
                    products = products.Where(s => s.ProductStateId == state);
                if (dateMin != null)
                    products = products.Where(s => s.Date >= dateMin);

                //Sortarea
                if (sortType == "Title")
                    products = products.OrderBy(x => x.Title);
                else
                if (sortType == "Date")
                    products = products.OrderByDescending(x => x.Date);
                else
                if (sortType == "PriceAsc")
                    products = products.OrderBy(x => x.Price);
                else
                if (sortType == "PriceDesc")
                    products = products.OrderByDescending(x => x.Price);

                //var model = new ProductViewModel { Products = products.ToList() };
                model = new ProductViewModel { Products = products.ToList() };

                //ViewBag.products = products;
                if (products.Count() > 0)
                {
                    ViewBag.NoResult = false;
                }

            }

            return View(model);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCities()
        {

            //generate empty list
            var selectList = new List<SelectListItem>();

            var cities = from cit in _db.Cities select cit;
            foreach (var city in cities)
            {
                selectList.Add(new SelectListItem
                {
                    Value = city.CityId.ToString(),
                    Text = city.CityName.ToString()
                });
            }

            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllProductStates()
        {

            //generate empty list
            var selectList = new List<SelectListItem>();

            var states = from st in _db.ProductState select st;
            foreach (var state in states)
            {
                selectList.Add(new SelectListItem
                {
                    Value = state.ProductStateId.ToString(),
                    Text = state.ProductStateName.ToString()
                });
            }

            return selectList;
        }


    }
}