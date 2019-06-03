using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Licenta.Common.Entities;
using Licenta.Common.Models;
using Licenta.DataAccess;
using System.Web;
using System.Collections.Generic;

namespace Licenta.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            ViewBag.Cities = GetAllCities();
            ViewBag.CategoriesList = GetAllCategories();

            var categories = from category in _db.Categories
                             orderby category.CategoryName
                             select category;
            ViewBag.Categories = categories;

            var userId = User.Identity.GetUserId();
            ViewBag.Allow = _db.Roles.Any(x => x.Users.Any(y => y.UserId == userId) && x.Name == "Administrator");

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            return View();
        }

        [Authorize(Roles = "Editor, Administrator")]
        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Editor, Administrator")]
        public ActionResult New([Bind(Exclude = "CategoryPhoto")]Category category)
        {
            try
            {
                byte[] imageData = null;
                if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var poImgFile = Request.Files["CategoryPhoto"];

                    using (var binary = new BinaryReader(poImgFile.InputStream))
                    {
                        imageData = binary.ReadBytes(poImgFile.ContentLength);
                    }
                }
                category.CategoryPhoto = imageData;

                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["message"] = "Categoria a fost adaugata!";

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View();
            }
        }

        //vizibil pt toata lumea
        public ActionResult Show(int id)
        {
            ViewBag.Cities = GetAllCities();
            ViewBag.CategoriesList = GetAllCategories();

            Category category = _db.Categories.Find(id);
            //ViewBag.CategoryId = category.CategoryId;
            //ViewBag.CategoryName = category.CategoryName;

            var userId = User.Identity.GetUserId();
            ViewBag.Allow = _db.Roles.Any(x => x.Users.Any(y => y.UserId == userId) && x.Name == "Administrator");

            var subCatgs = _db.SubCategories.Where(a => a.CategoryId == id).OrderBy(x => x.SubCategoryName);

            var nrPerColumn = subCatgs.Count() / 4;
            var surplus = subCatgs.Count() - (nrPerColumn * 4);
            //bool isInt = nrPerColumn == ((int)nrPerColumn);

            var s = 0;
            var counter = 0;
            
            IList<SubCategory> list1 = null; IList<SubCategory> list2 = null; IList<SubCategory> list3 = null; IList<SubCategory> list4 = null;
            if (subCatgs.Count() > 0)
            {
                if (surplus > 0)
                {
                    s = 1;
                    surplus = surplus - 1;
                }
                list1 = subCatgs.Take(nrPerColumn + s).ToList();
                counter = nrPerColumn + s;
                if (subCatgs.Count() > counter)
                {
                    s = 0;
                    if (surplus > 0)
                    {
                        s = 1;
                        surplus = surplus - 1;
                    }
                    list2 = subCatgs.Skip(counter).Take(nrPerColumn + s).ToList();
                    counter = counter + nrPerColumn + s;
                    if (subCatgs.Count() > counter)
                    {
                        s = 0;
                        if (surplus > 0)
                        {
                            s = 1;
                            surplus = surplus - 1;
                        }
                        list3 = subCatgs.Skip(counter).Take(nrPerColumn + s).ToList();
                        counter = counter + nrPerColumn + s;
                         if (subCatgs.Count() > counter)
                         {
                            s = 0;
                            if (surplus > 0)
                            {
                                s = 1;
                                surplus = surplus - 1;
                            }
                            list4 = subCatgs.Skip(counter).Take(nrPerColumn + s).ToList();
                         }

                    }
                }
                /*if (surplus == 1)
                {
                    list1 = subCatgs.Take(nrPerColumn + 1).ToList();
                    if (subCatgs.ToList().Count > nrPerColumn + 1)
                    {
                        list2 = subCatgs.Skip(nrPerColumn + 1).Take(nrPerColumn).ToList();
                        if (subCatgs.Count() > 2 * nrPerColumn + 1)
                        {
                            list3 = subCatgs.Skip(2 * nrPerColumn + 1).Take(nrPerColumn).ToList();
                            if (subCatgs.Count() > 3 * nrPerColumn + 1)
                            {
                                list3 = subCatgs.Skip(3 * nrPerColumn + 1).Take(nrPerColumn).ToList();
                            }

                        }
                    }*/
                }
               

            var model = new CategoryViewModel { CategoryId = category.CategoryId,
                                                CategoryName = category.CategoryName,
                                                CategoryPhoto = category.CategoryPhoto,
                                                SubCategories = subCatgs.ToList(),
                                                SubCategoriesL1 = list1,
                                                SubCategoriesL2 = list2,
                                                SubCategoriesL3 = list3,
                                                SubCategoriesL4 = list4};

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View(model);
        }

        //using ViewBag, not model
        /*public ActionResult Show(int id)
        {
            Category category = _db.Categories.Find(id);
            ViewBag.CategoryId = category.CategoryId;
            ViewBag.CategoryName = category.CategoryName;
            var userId = User.Identity.GetUserId();

            ViewBag.Allow = _db.Roles.Any(x => x.Users.Any(y => y.UserId == userId) && x.Name == "Administrator");

            var subCatgs = _db.SubCategories.Where(a => a.CategoryId == id);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            ViewBag.SubCategories = subCatgs;
           
            return View();
        }*/

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int id)
        {
            Category category = _db.Categories.Find(id);
            ViewBag.Category = category;
            return View(category);
        }

        [HttpPut]
        [Authorize(Roles = "Editor, Administrator")]
        public ActionResult Edit(int id, [Bind(Exclude = "CategoryPhoto")] Category requestCategory)
        {
            Category category = _db.Categories.Find(id);

            byte[] imageData = null;
            if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
            {
                HttpPostedFileBase poImgFile = Request.Files["CategoryPhoto"];

                using (var binary = new BinaryReader(poImgFile.InputStream))
                {
                    imageData = binary.ReadBytes(poImgFile.ContentLength);
                }

                //UserPhoto is not updated if no file is chosen
                if (imageData.Length > 0)
                {
                    category.CategoryPhoto = imageData;
                }

            }
            try
            {
                //if (ModelState.IsValid)
                //{
                   // if (TryUpdateModel(category))
                    //{
                        category.CategoryName = requestCategory.CategoryName;
                        _db.SaveChanges();
                        TempData["message"] = "Categoria a fost modificata!";
                    //}
                    return RedirectToAction("Index");
                //}

                //return View(requestCategory);

            }
            catch (Exception e)
            {
                return View(requestCategory);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Delete(int id)
        {
            Category category = _db.Categories.Find(id);
            _db.Categories.Remove(category);

            var subcategories = _db.SubCategories.Where(x => x.CategoryId == id);
            if(subcategories.Count() > 0) { 
                foreach(var subcategory in subcategories)
                {
                    var products = _db.Products.Where(x => x.SubCategoryId == subcategory.SubCategoryId);
                    foreach (var product in products)
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

                    _db.SubCategories.Remove(subcategory);
                }
            }

            _db.SaveChanges();
            TempData["message"] = "Categoria a fost stearsa!";
            return RedirectToAction("Index", "Home");
        }

        public FileContentResult DisplayCategoryPhoto(int categoryId)
        {
            var category = (from cat in _db.Categories
                          where cat.CategoryId.Equals(categoryId)
                          select cat).Single();
            var catImage = category.CategoryPhoto;

            if (catImage == null || catImage.Length <= 0)
            {
                string fileName = HttpContext.Server.MapPath(@"~/Images/noImg.png");

                byte[] imageData = null;
                FileInfo fileInfo = new FileInfo(fileName);
                long imageFileLength = fileInfo.Length;
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);
                imageData = br.ReadBytes((int)imageFileLength);

                return File(imageData, "image/png");
            }

            return new FileContentResult(catImage, "image/jpeg");
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

    }
}