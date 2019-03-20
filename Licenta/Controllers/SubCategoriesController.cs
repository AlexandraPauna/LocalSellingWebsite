using Licenta.Models;
using Licenta.Models.Categories;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Controllers
{
    public class SubCategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: SubCategories
        public ActionResult Index()
        {
            return View();
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();
            // Extragem toate categoriile din baza de date
            var categories = from cat in db.Categories select cat;
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
                    db.SubCategories.Add(subCategory);
                    db.SaveChanges();
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
        public ActionResult Show(int id)
        {
            SubCategory subCategory = db.SubCategories.Find(id);
            ViewBag.SubCategoryId = subCategory.SubCategoryId;
            ViewBag.SubCategoryName = subCategory.SubCategoryName;
            var userId = User.Identity.GetUserId();

            ViewBag.Allow = db.Roles.Any(x => x.Users.Any(y => y.UserId == userId) && x.Name == "Administrator");

            var products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Where(a => a.SubCategoryId == id);
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            ViewBag.Products = products;

            return View();
        }
        
        [HttpPost]
        public ActionResult Show(int id, string sortType)
        {
            SubCategory subCategory = db.SubCategories.Find(id);
            ViewBag.SubCategoryId = subCategory.SubCategoryId;
            ViewBag.SubCategoryName = subCategory.SubCategoryName;
            var userId = User.Identity.GetUserId();

            ViewBag.Allow = db.Roles.Any(x => x.Users.Any(y => y.UserId == userId) && x.Name == "Administrator");

            var products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Where(a => a.SubCategoryId == id);

            if (sortType == "Title")
                products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Where(a => a.SubCategoryId == id).OrderBy(x => x.Title);
            else
            if (sortType == "Date")
                products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Where(a => a.SubCategoryId == id).OrderByDescending(x => x.Date);
            else
            if (sortType == "PriceAsc")
                products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Where(a => a.SubCategoryId == id).OrderBy(x => x.Price);
            else
            if (sortType == "PriceDesc")
                products = db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Where(a => a.SubCategoryId == id).OrderByDescending(x => x.Price);


            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            ViewBag.Products = products;
           
            return View();
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int id)
        {
            SubCategory subCategory = db.SubCategories.Find(id);
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
                    SubCategory subCategory = db.SubCategories.Find(id);
                    if (TryUpdateModel(subCategory))
                    {
                        subCategory.SubCategoryName = requestSubCategory.SubCategoryName;
                        db.SaveChanges();
                        TempData["message"] = "Subcategoria a fost modificata!";
                    }
                    return RedirectToAction("Index");
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
            SubCategory subCategory = db.SubCategories.Find(id);
            db.SubCategories.Remove(subCategory);
            db.SaveChanges();
            TempData["message"] = "SubCategoria a fost stearsa!";
            return RedirectToAction("Show", "Categories", new { id = subCategory.CategoryId });
        }
    }
}
