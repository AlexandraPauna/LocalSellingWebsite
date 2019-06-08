using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Licenta.Common.Entities;
using Licenta.Common.Models;
using Licenta.DataAccess;
using System.Threading.Tasks;

namespace Licenta.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private ApplicationUserManager _userManager;
        private readonly EmailService _emailService = new EmailService();

        public ActionResult Index(string id, string sortType)
        {
            var user = (from usr in _db.Users
                        where usr.Id == id
                        select usr).Single();
            ViewBag.User = user;

            var products = from prod in _db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User")
                           where prod.UserId.Equals(id)
                           select prod;

            if (sortType == null)
            {
                sortType = "Active";
            }
            if(sortType == "Active")
            {
                products = products.Where(p => p.Active == true);
            }
            if(sortType == "Deactivated")
            {
                products = products.Where(p => p.Active == false);
            }
            products = products.OrderByDescending(p => p.Date);

            var model = new ProductViewModel { Products = products.ToList() };

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View(model);
        }

        public ActionResult Personal(string sortType)
        {
            var id = User.Identity.GetUserId();

            if (id == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = (from usr in _db.Users
                        where usr.Id == id
                        select usr).Single();
            ViewBag.User = user;

            var products = from prod in _db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User")
                           where prod.UserId.Equals(id)
                           select prod;

            if (sortType == null)
            {
                sortType = "Active";
            }
            if (sortType == "Active")
            {
                products = products.Where(p => p.Active == true);
            }
            if (sortType == "Deactivated")
            {
                products = products.Where(p => p.Active == false);
            }
            products = products.OrderByDescending(p => p.Date);

            var model = new ProductViewModel { Products = products.ToList() };

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            var unreadMessages = (from mess in _db.Messages
                                  where mess.ReceiverId == id && mess.Read == false
                                  select mess).Count();
            ViewBag.UnreadMessages = unreadMessages;

            var nrAds = _db.Products.Where(x => x.UserId == id).Count();
            ViewBag.NrAds = nrAds;

            var nrRatings = _db.Ratings.Where(x => x.RatedUserId == id).Count();
            ViewBag.NrRatings = nrRatings;

            var nrInterests = _db.Interests.Where(x => x.UserId == id).Count();
            ViewBag.NrInterests = nrInterests;

            return View(model);
        }


        public ActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            Product product = _db.Products.Find(id);
            ViewBag.Product = product;
            ViewBag.City = product.City;

            var catId = (from categs in _db.SubCategories
                         where categs.SubCategoryId.Equals(product.SubCategoryId)
                         select categs.CategoryId).Single();
            ViewBag.Category = Convert.ToInt32(catId);
           
            var catName = (from categs in _db.Categories
                           where categs.CategoryId.Equals(catId)
                           select categs.CategoryName).Single();
            ViewBag.CategoryName = Convert.ToString(catName);

            ViewBag.SubCategory = product.SubCategory;
            ViewBag.DeliveryCompany = product.DeliveryCompany;

            var productImages = from prodImages in _db.ProductImages
                                where prodImages.ProductId.Equals(product.ProductId)
                                select prodImages.Id;

            
            var imgList = new List<String>();
            foreach (var img in productImages)
            {
                imgList.Add("/Product/ProductPhoto/?photoId=" + img);
            }
            ViewBag.ProductImages = imgList;

            //user profile image
            var userImage = "/Account/UserPhotos/?id="+product.UserId;
            ViewBag.UserImage = userImage;

            var currentUser = User.Identity.GetUserId();
            if (currentUser != null && currentUser != product.UserId)
            {
                product.Views = product.Views + 1;
                _db.SaveChanges();
            }
            if (currentUser == product.UserId || User.IsInRole("Administrator") || User.IsInRole("Editor"))
                ViewBag.Allow = true;
            else
                ViewBag.Allow = false;

            ViewBag.currentUser = currentUser;
            var interest = from interests in _db.Interests
                       where interests.ProductId == id && interests.UserId == currentUser
                       select interests;

            if (interest.Count() > 0)
            {
                ViewBag.interest = interest.First();
            }
            else ViewBag.interest = null;

            return View(product);
        }

        // GET: /Product/New
        public ActionResult New()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Product product = new Product();
            product.Cities = GetAllCities();
            ViewBag.Categories = GetAllCategories();
            product.ProductStateTypes = GetAllProductStateTypes();
            product.DeliveryCompanies = GetAllDeliveryCompanies();
            product.UserId = User.Identity.GetUserId();
            product.Views = 0;

            return View(product);
        }

        // POST: /Product/New
        [HttpPost]
       // public ActionResult New([Bind(Exclude = "ProductPhotos")]Product product)
        public async Task<ActionResult> New([Bind(Exclude = "ProductPhotos")]Product product)
        {
            product.Cities = GetAllCities();
            ViewBag.Categories = GetAllCategories();
            product.ProductStateTypes = GetAllProductStateTypes();
            product.DeliveryCompanies = GetAllDeliveryCompanies();
            product.UserId = User.Identity.GetUserId();
            var currentUserId = User.Identity.GetUserId();
            var user = (from users in _db.Users
                        where users.Id == currentUserId
                        select users).Single();
            product.User = (ApplicationUser)user;

            //convert the user uploaded Photo as Byte Array before save to DB
            //byte[] imageData = null;
            if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
            {
                List<HttpPostedFileBase> images = new List<HttpPostedFileBase>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    // this file's Type is HttpPostedFileBase which is in memory
                    images.Add(file);
                }

                var prdImageList = new List<ProductImage>();
                foreach (var image in images)
                {
                    using (var br = new BinaryReader(image.InputStream))
                    {
                        var data = br.ReadBytes(image.ContentLength);
                        var img = new ProductImage { ProductId = product.ProductId };
                        img.ImageData = data;
                        prdImageList.Add(img);
                    }
                }
                product.ProductImages = prdImageList;


            }

            //product.SubCategories = GetAllSubCategories();
            try
            {
                //if (ModelState.IsValid)
               // {
                    _db.Products.Add(product);
                    _db.SaveChanges();
                    TempData["message"] = "Anuntul a fost adaugat cu succes!";

                    //send email
                    string content = "Buna " + user.UserName + ", \r\n" + "Felicitari! Anuntul tau a fost adaugat cu succes! Anunt:" + product.Title + ".";
                    await _emailService.SendEmailAsync(user.Email, "site_anunturi@yahoo.com", "Site anunturi", "Anunt nou", content);

                return RedirectToAction("Show", "SubCategories", new { @id = product.SubCategoryId });
                // }
                // else
                // {
                //     return View(product);
                // }
            }
            catch (Exception e)
            {

                return View(product);
            }
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
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            //generate empty list
            var selectList = new List<SelectListItem>();

            //get all categories from Data Base
            var categories = from cat in _db.Categories select cat;
            foreach (var category in categories)
            {
                //add elements in dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllProductStateTypes()
        {
            //generate empty list
            var selectList = new List<SelectListItem>();

            var stateTypes = from st in _db.ProductState select st;
            foreach (var stateType in stateTypes)
            {
                //add elements in dropdown
                selectList.Add(new SelectListItem
                {
                    Value = stateType.ProductStateId.ToString(),
                    Text = stateType.ProductStateName.ToString()
                });
            }

            return selectList;
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllDeliveryCompanies()
        {
            //generate empty list
            var selectList = new List<SelectListItem>();

            var delivTypes = from dlv in _db.DeliveryCompanies select dlv;
            foreach (var delivType in delivTypes)
            {
                //add elements in dropdown
                selectList.Add(new SelectListItem
                {
                    Value = delivType.DeliveryCompanyId.ToString(),
                    Text = delivType.DeliveryCompanyName.ToString()
                });
            }

            return selectList;
        }

        public FileContentResult MainProductPhoto(int prodId)
        {
            var productsImages = from prodImages in _db.ProductImages
                                 where prodImages.ProductId.Equals(prodId)
                                 select prodImages;
            var prodImage = productsImages.FirstOrDefault();

            if (prodImage == null)
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

            return new FileContentResult(prodImage.ImageData, "image/jpeg");

        }

        public FileContentResult ProductPhoto(int photoId)
        {
            var productsImages = from prodImages in _db.ProductImages
                                 where prodImages.Id.Equals(photoId)
                                 select prodImages;
            var prodImage = productsImages.FirstOrDefault();

            if (prodImage == null)
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

            return new FileContentResult(prodImage.ImageData, "image/jpeg");

        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult LoadSubCategories(int catId)
        {
            var subCatList = GetAllSubCategories(Convert.ToInt32(catId));

            return Json(subCatList, JsonRequestBehavior.AllowGet);

        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllSubCategories(int selectedCatId)
        {
            //generate empty list
            var selectList = new List<SelectListItem>();

            //get all categories from Data Base

            var subcategories = from sbcat in _db.SubCategories
                                where sbcat.CategoryId == selectedCatId
                                select sbcat;
            foreach (var subcategory in subcategories)
            {
                //add elements in dropdown
                selectList.Add(new SelectListItem
                {
                    Value = subcategory.SubCategoryId.ToString(),
                    Text = subcategory.SubCategoryName.ToString()
                });
            }

            return selectList;

        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategoriesForEdit(int id)
        {
            //generate empty list
            var selectList = new List<SelectListItem>();

            var prod = _db.Products.Find(id);
            var defaultCatId = from categs in _db.SubCategories
                        where categs.SubCategoryId.Equals(prod.SubCategoryId)
                        select categs.CategoryId;

            //get all categories from Data Base
            var categories = from cat in _db.Categories select cat;
            foreach (var category in categories)
            {
                //add elements in dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString(),
                    Selected = (category.CategoryId.ToString() == defaultCatId.ToString() ? true : false)
                });
            }

            
            

            return selectList;
        }

        public ActionResult Edit(int id)
        {
            Product product = _db.Products.Find(id);
            ViewBag.Product = product;
            product.Cities = GetAllCities();
            ViewBag.Categories = GetAllCategories();
            //ViewBag.Categories = GetAllCategoriesForEdit(id);
            var catId = (from categs in _db.SubCategories
                              where categs.SubCategoryId.Equals(product.SubCategoryId)
                              select categs.CategoryId).Single();
            ViewBag.CategoryId = Convert.ToInt32(catId);
            //product.Categories = GetAllCategories();
            product.ProductStateTypes = GetAllProductStateTypes();
            product.DeliveryCompanies = GetAllDeliveryCompanies();

            if (product.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator") || User.IsInRole("Editor"))
            {
                return View(product);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui anunt care nu va apartine!";
                return Redirect(Request.UrlReferrer.ToString());
            }
        }

        [HttpPut]
        //public ActionResult Edit([Bind(Exclude = "ProductPhotos")]int id, Product requestProduct)
        public async Task<ActionResult> Edit([Bind(Exclude = "ProductPhotos")]int id, Product requestProduct)
        {
            try
            {

                Product product = _db.Products.Find(id);

                product.Title = requestProduct.Title;
                product.Price = requestProduct.Price;
                product.Description = requestProduct.Description;
                product.CityId = requestProduct.CityId;
                //product.CategoryId = requestProduct.CategoryId;
                product.SubCategoryId = requestProduct.SubCategoryId;
                product.ProductStateId = requestProduct.ProductStateId;
                product.Site = requestProduct.Site;
                product.PersonalDelivery = requestProduct.PersonalDelivery;
                product.DeliveryCompanyId = requestProduct.DeliveryCompanyId;
                product.ReturnPolicy = requestProduct.ReturnPolicy;
                product.Warranty = requestProduct.Warranty;
                _db.SaveChanges();
                TempData["message"] = "Anuntul a fost modificat cu succes!";

                //send email

                var usersInterested = from interests in _db.Interests
                                      where interests.ProductId == id
                                      select interests.User;
                foreach(var user in usersInterested)
                {
                    string content = "Buna " + user.UserName + ", \n" + "Un anunt de care esti interesat a fost modificat. Anunt: " + product.Title + ".";
                    await _emailService.SendEmailAsync(user.Email, "site_anunturi@yahoo.com", "Site anunturi", "Notificare modificare anunt", content);

                }

                return RedirectToAction("Show", "SubCategories", new { @id = product.SubCategoryId });
            }
            catch (Exception e)
            {
                return View();
            }
        }

        public struct Data
        {
            public Data(int id, string source)
            {
                IntegerData = id;
                StringData = source;
            }

            public int IntegerData { get; private set; }
            public string StringData { get; private set; }
        }


        public ActionResult ManageGallery(int id)
        {
            Product product = _db.Products.Find(id);
            ViewBag.Product = product;

            var productImages = from prodImages in _db.ProductImages
                                where prodImages.ProductId.Equals(product.ProductId)
                                select prodImages.Id;

            var imgList = new List<Data>();

            foreach (var img in productImages)
            {
                imgList.Add(new Data(img, "/Product/ProductPhoto/?photoId=" + img));
            }
            ViewBag.ProductImages = imgList;

            //var imgList = new List<String>();
            //foreach (var img in productImages)
            //{
            //    imgList.Add("/Product/ProductPhoto/?photoId=" + img);
            //}
            //ViewBag.ProductImages = imgList;

            //ViewBag.ProductImagesId = productImages.ToList();

            if (product.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator")|| User.IsInRole("Editor"))
            {
                return View(product);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui anunt care nu va apartine!";
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        public ActionResult AddPhotos([Bind(Exclude = "ProductPhotos")]int id)
        {

            Product product = _db.Products.Find(id);
            if (Request.Files.Count > 0 &&  Request.Files[0].ContentLength > 0)
            {

                List<HttpPostedFileBase> images = new List<HttpPostedFileBase>();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    // this file's Type is HttpPostedFileBase which is in memory
                    images.Add(file);
                }

                var prdImageList = new List<ProductImage>();
                foreach (var image in images)
                {
                    using (var br = new BinaryReader(image.InputStream))
                    {
                        var data = br.ReadBytes(image.ContentLength);
                        var img = new ProductImage { ProductId = product.ProductId };
                        img.ImageData = data;
                        prdImageList.Add(img);
                    }
                }
                product.ProductImages = prdImageList;

            }
            
            _db.SaveChanges();
            TempData["message"] = "Adaugare efectuata!";

            return RedirectToAction("ManageGallery", "Product" , new { id = id });

        }

        [HttpDelete]
        public ActionResult DeletePhoto(int id)
        {
            ProductImage prdPhoto = _db.ProductImages.Find(id);

            _db.ProductImages.Remove(prdPhoto);
            _db.SaveChanges();

            TempData["message"] = "Poza a fost stearsa!";
            return RedirectToAction("ManageGallery", "Product", new { id = prdPhoto.ProductId });

        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            Product product = _db.Products.Find(id);
            var currentUser = User.Identity.GetUserId();
            var user = _db.Users.Where(x => x.Id == currentUser).SingleOrDefault();
            if (product.UserId == currentUser || User.IsInRole("Administrator") || User.IsInRole("Editor"))
            {
                var productImages = _db.ProductImages.Where(x => x.ProductId == product.ProductId);
                _db.ProductImages.RemoveRange(productImages);
                var conversations = _db.Conversations.Where(x => x.ProductId == id);
                foreach(var conversation in conversations)
                {
                    var messages = _db.Messages.Where(x => x.ConversationId == conversation.ConversationId);
                    _db.Messages.RemoveRange(messages);
                }
                _db.Conversations.RemoveRange(conversations);
                var interests = _db.Interests.Where(x => x.ProductId == id);
                _db.Interests.RemoveRange(interests);

                _db.Products.Remove(product);
                _db.SaveChanges();
                TempData["message"] = "Anuntul a fost sters!";

                //trimitere email
                string content = "Buna " + user.UserName + ", \r\n" + "Anuntul tau a fost sters cu succes! Anunt:" + product.Title + ".";
                await _emailService.SendEmailAsync(user.Email, "site_anunturi@yahoo.com", "Site anunturi", "Anunt sters", content);

            }

            return RedirectToAction("Index", "Product", new { id = currentUser});
        }

        public ActionResult Activate(int id)
        {
            Product product = _db.Products.Find(id);
            var currentUser = User.Identity.GetUserId();
            if (product.UserId == currentUser || User.IsInRole("Administrator") || User.IsInRole("Editor"))
            {
                product.Active = true;
                product.DateLastChecked = DateTime.Now;

                _db.SaveChanges();
                TempData["message"] = "Anuntul a fost Activat!";
            }

            //return RedirectToAction("Show", "Product", new { id = product.ProductId });
            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Deactivate(int id)
        {
            Product product = _db.Products.Find(id);
            var currentUser = User.Identity.GetUserId();
            if (product.UserId == currentUser || User.IsInRole("Administrator") || User.IsInRole("Editor"))
            {
                product.Active = false;

                _db.SaveChanges();
                TempData["message"] = "Anuntul a fost dezactivat!";
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

        public ActionResult Save(int id)
        {
            _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "AccountControler");
            }
            else
            {
                var interest = from interests in _db.Interests
                               where interests.ProductId == id && interests.UserId == userId
                               select interests;
                if (interest.Count() == 1)
                {
                    _db.Interests.Remove(interest.First());
                }
                else
                {
                    Interest newInterest = new Interest();
                    newInterest.ProductId = id;
                    var product = (from prod in _db.Products
                                  where prod.ProductId == id
                                  select prod).SingleOrDefault();
                    newInterest.Product = (Product) product;
                    newInterest.UserId = userId;
                    newInterest.Date = DateTime.Now;

                    _db.Interests.Add(newInterest);
                }
                _db.SaveChanges();
            }

            return Redirect(Request.UrlReferrer.ToString());
        }

    }

}