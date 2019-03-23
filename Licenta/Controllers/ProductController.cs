﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Licenta.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Controllers
{
    public class ProductController : Controller
    {
        //private ProductDBContext db = new ProductDBContext();
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: /Product/Index
        public ActionResult Index()
        {
            _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //products of current logged in user
            var products = from prod in db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User")
                where prod.UserId.Equals(userId)
                select prod;

            var model = new ProductViewModel { Products = products.ToList() };

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            return View(model);
        }


        public ActionResult Show(int id)
        {
            Product product = db.Products.Find(id);
            ViewBag.Product = product;
            ViewBag.City = product.City;

            var catId = (from categs in db.SubCategories
                         where categs.SubCategoryId.Equals(product.SubCategoryId)
                         select categs.CategoryId).Single();
            ViewBag.Category = Convert.ToInt32(catId);
            /*var catId = from categs in db.SubCategories
                        where categs.SubCategoryId.Equals(product.SubCategoryId)
                        select categs.CategoryId;
            ViewBag.Category = catId.First();*/
           
            var catName = (from categs in db.Categories
                           where categs.CategoryId.Equals(catId)
                           select categs.CategoryName).Single();
            ViewBag.CategoryName = Convert.ToString(catName);

            //ViewBag.Category = product.Category;
            ViewBag.SubCategory = product.SubCategory;
            ViewBag.DeliveryCompany = product.DeliveryCompany;

            var productImages = from prodImages in db.ProductImages
                                where prodImages.ProductId.Equals(product.ProductId)
                                select prodImages.Id;

            /*if (productImages == null)
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

            ViewBag.ProductImages = productImages;*/
            //ViewBag.ProductImages = new FileContentResult(productImages, "image/jpeg");
            var imgList = new List<String>();
            foreach (var img in productImages)
            {
                //var imgData = new FileContentResult(img.ImageData, "image/jpeg");
                imgList.Add("/Product/ProductPhoto/?photoId=" + img);
            }
            ViewBag.ProductImages = imgList;

            return View(product);
        }

        // GET: /Product/New
        public ActionResult New()
        {
            _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {

                Product product = new Product();
                product.Cities = GetAllCities();
                //product.Categories = GetAllCategories();
                ViewBag.Categories = GetAllCategories();
                product.ProductStateTypes = GetAllProductStateTypes();
                product.DeliveryCompanies = GetAllDeliveryCompanies();
                product.UserId = User.Identity.GetUserId();
                //product.Date = DateTime.Now;

                return View(product);
            }
        }

        // POST: /Product/New
        [HttpPost]
        public ActionResult New([Bind(Exclude = "ProductPhotos")]Product product)
        {
            product.Cities = GetAllCities();
            //product.Categories = GetAllCategories();
            ViewBag.Categories = GetAllCategories();
            product.ProductStateTypes = GetAllProductStateTypes();
            product.DeliveryCompanies = GetAllDeliveryCompanies();
            product.UserId = User.Identity.GetUserId();

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
                    db.Products.Add(product);
                    db.SaveChanges();
                    TempData["message"] = "Anuntul a fost adaugat cu succes!";

                    return RedirectToAction("Index");
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

            var cities = from cit in db.Cities select cit;
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
            var categories = from cat in db.Categories select cat;
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

            var stateTypes = from st in db.ProductState select st;
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

            var delivTypes = from dlv in db.DeliveryCompanies select dlv;
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
            var productsImages = from prodImages in db.ProductImages
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
            var productsImages = from prodImages in db.ProductImages
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

            var subcategories = from sbcat in db.SubCategories
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

            var prod = db.Products.Find(id);
            var defaultCatId = from categs in db.SubCategories
                        where categs.SubCategoryId.Equals(prod.SubCategoryId)
                        select categs.CategoryId;

            //get all categories from Data Base
            var categories = from cat in db.Categories select cat;
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
            Product product = db.Products.Find(id);
            ViewBag.Product = product;
            product.Cities = GetAllCities();
            ViewBag.Categories = GetAllCategories();
            //ViewBag.Categories = GetAllCategoriesForEdit(id);
            var catId = (from categs in db.SubCategories
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
                return RedirectToAction("Index");
            }
        }

        [HttpPut]
        public ActionResult Edit([Bind(Exclude = "ProductPhotos")]int id, Product requestProduct)
        {
            try
            {

                Product product = db.Products.Find(id);

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
                db.SaveChanges();
                TempData["message"] = "Anuntul a fost modificat cu succes!";

                return RedirectToAction("Index");
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
            Product product = db.Products.Find(id);
            ViewBag.Product = product;

            var productImages = from prodImages in db.ProductImages
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

            Product product = db.Products.Find(id);
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
            
            db.SaveChanges();
            TempData["message"] = "Adaugare efectuata!";

            return RedirectToAction("ManageGallery", "Product" , new { id = id });

        }

        [HttpDelete]
        public ActionResult DeletePhoto(int id)
        {
            ProductImage prdPhoto = db.ProductImages.Find(id);

            db.ProductImages.Remove(prdPhoto);
            db.SaveChanges();

            TempData["message"] = "Poza a fost stearsa!";
            return RedirectToAction("ManageGallery", "Product", new { id = prdPhoto.ProductId });

        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            if (product.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator") || User.IsInRole("Editor"))
            {
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["message"] = "Anuntul a fost sters!";
            }
            return RedirectToAction("Index");
        }
    }
}