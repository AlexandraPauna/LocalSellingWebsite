using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Licenta.Common.Entities;
using Licenta.Common.Models;
using Licenta.DataAccess;
using PagedList;

namespace Licenta.Controllers
{
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: SubCategories
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult New()
        {
            SubCategory subCategory = new SubCategory();
            subCategory.Categories = GetAllCategories();

            return View(subCategory);
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult New(SubCategory subCategory)
        {
            subCategory.Categories = GetAllCategories();
            try
            {
                if (ModelState.IsValid)
                {
                    _db.SubCategories.Add(subCategory);
                    _db.SaveChanges();
                    TempData["message"] = "Subcategoria a fost adaugata!";
                    return RedirectToAction("Show", "Categories", new { id = subCategory.CategoryId });

                }
                else
                {
                    return View(subCategory);
                }
            }
            catch (Exception e)
            {
                return View();
            }
        }

        //vizibil pt toata lumea
        public ActionResult Show(int id, DateTime? dateMin, int? fromCity, float? priceMin, float? priceMax, int? state, string sortType, int? page)
        {
            /*if(dateMin != null || fromCity != null || priceMin != null || priceMax!= null || state != null || sortType != null)
            {
                page = 1;
            }*/

            ViewBag.Cities = GetAllCities();
            ViewBag.ProductStates = GetAllProductStates();
            ViewBag.Categories = GetAllCategories();

            SubCategory subCategory = _db.SubCategories.Find(id);
            var userId = User.Identity.GetUserId();

            ViewBag.Allow = _db.Roles.Any(x => x.Users.Any(y => y.UserId == userId) && x.Name == "Administrator");

            var products = _db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Where(a => a.SubCategoryId == id);
            products = products.Where(p => p.Active == true);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            var currentUser = User.Identity.GetUserId();
            var interests = _db.Interests.Where(i => i.UserId == currentUser);

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
            ViewBag.PriceMin = priceMin;
            ViewBag.PriceMax = priceMax;
            ViewBag.FromCity = fromCity;
            ViewBag.State = state;
            ViewBag.DateMin = dateMin;
            ViewBag.SortType = sortType;

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

            var subcategories = from subc in _db.SubCategories
                                where subc.CategoryId == subCategory.CategoryId && subc.SubCategoryId != subCategory.SubCategoryId
                                select subc;

            int pageIndex = page ?? 1;
            int dataCount = 2;

            //ViewBag.Products = products;
            var model = new SubCategoryViewModel { SubCategoryId = subCategory.SubCategoryId,
                                                   SubCategoryName = subCategory.SubCategoryName,
                                                   CategoryId = subCategory.CategoryId,
                                                   Category = subCategory.Category,
                                                   Products = products.ToList().ToPagedList(pageIndex, dataCount),
                                                   NrProducts = products.ToList().Count(),
                                                   SubCategories = subcategories.ToList(),
                                                   Interests = interests.ToList()};

            return View(model);
        }

        

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int id)
        {
            SubCategory subCategory = _db.SubCategories.Find(id);
            ViewBag.SubCategory = subCategory;
            subCategory.Categories = GetAllCategories();
            
            return View(subCategory);
        }

        [HttpPut]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int id, SubCategory requestSubCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SubCategory subCategory = _db.SubCategories.Find(id);
                    if (TryUpdateModel(subCategory))
                    {
                        subCategory.SubCategoryName = requestSubCategory.SubCategoryName;
                        _db.SaveChanges();
                        TempData["message"] = "Subcategoria a fost modificata!";
                    }
                    return RedirectToAction("Show", "Categories", new { id = subCategory.CategoryId });
                }
                else
                {
                    return View(requestSubCategory);
                }

            }
            catch (Exception e)
            {
                return View(requestSubCategory);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Delete(int id)
        {
            SubCategory subCategory = _db.SubCategories.Find(id);
            var products = _db.Products.Where(x => x.SubCategoryId == subCategory.SubCategoryId);
            foreach(var product in products)
            {
                var productImages = _db.ProductImages.Where(x => x.ProductId == product.ProductId);
                _db.ProductImages.RemoveRange(productImages);

                var conversations = _db.Conversations.Where(x => x.ProductId == product.ProductId);
                foreach (var conversation in conversations)
                {
                    var messages = _db.Messages.Where(x => x.ConversationId == conversation.ConversationId);
                    _db.Messages.RemoveRange(messages);
                }
                _db.Conversations.RemoveRange(conversations);

                var interests = _db.Interests.Where(x => x.ProductId == product.ProductId);
                _db.Interests.RemoveRange(interests);

                _db.Products.Remove(product);
            }

            _db.SubCategories.Remove(subCategory);

            _db.SaveChanges();
            TempData["message"] = "SubCategoria a fost stearsa!";
            return RedirectToAction("Show", "Categories", new { id = subCategory.CategoryId });
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();
            // Extragem toate categoriile din baza de date
            var categories = from cat in _db.Categories select cat;
            // iteram prin categorii
            foreach (var category in categories)
            {
                // Adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            // returnam lista de categorii
            return selectList;
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
