using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Licenta.Common.Entities;
using Licenta.DataAccess;
using Microsoft.AspNet.Identity;

namespace Licenta.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            //deactivate old products (that have been active for 30 days at least)
            CheckProductsActivation(DateTime.Now);

            var categories = from category in _db.Categories
                             orderby category.CategoryName
                             select category;

            ViewBag.Categories = categories.Take(12);

            var products = _db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Include("User").OrderByDescending(a => a.Date);
            ViewBag.LatestProducts = products.Take(15);

            var locations = (from c in _db.Cities
                             join p in _db.Products
                             on c.CityId equals p.CityId
                             select new { c, p } into x
                             group x by new { x.c } into g
                             select new
                             {
                                 City = g.Key.c,
                                 NrProducts = g.Select(x => x.p).Count()
                             }).OrderByDescending(y => y.NrProducts);
            var locationsList = new List<City>();
            foreach(var location in locations)
            {
                var city = new City
                {
                    CityId = location.City.CityId,
                    CityName = location.City.CityName
                };
                locationsList.Add(city);
            }
            ViewBag.LocationsList1 = null;
            ViewBag.LocationsList2 = null;
            ViewBag.LocationsList3 = null;
            ViewBag.LocationsList4 = null;
            if (locationsList.Count() > 0)
            {
                ViewBag.LocationsList1 = locationsList.Take(8);
                if (locationsList.Count > 8)
                {
                    ViewBag.LocationsList2 = locationsList.Skip(8).Take(8).ToList();
                    if (locationsList.Count > 16)
                    {
                        ViewBag.LocationsList2 = locationsList.Skip(16).Take(8).ToList();
                        if (locationsList.Count > 24)
                        {
                            ViewBag.LocationsList3 = locationsList.Skip(8).Take(8).ToList();
                        }

                    }
                }
            }

            ViewBag.NrCategories = categories.Count();
            var subCategories = from subcategory in _db.SubCategories
                                select subcategory;
            ViewBag.NrSubcategories = subCategories.Count();
            var cities = from city in _db.Cities
                         select city;
            ViewBag.NrCities = cities.Count();
            var usersWithUserRole = (from user in _db.Users
                                  from userRole in user.Roles
                                  join role in _db.Roles on userRole.RoleId equals role.Id
                                  select new
                                  {
                                      Role = role.Name
                                  }).Where(r => r.Role == "User").ToList();
            ViewBag.NrUsers = usersWithUserRole.Count();

            ViewBag.Cities = GetAllCities();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            return View();
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
        public EmptyResult CheckProductsActivation(DateTime currentDate)
        {
            var products = from prod in _db.Products
                           //where (currentDate - prod.DateLastChecked).Days >= 30
                           where DbFunctions.DiffDays(prod.DateLastChecked, currentDate) >= 30
                           select prod;
            foreach (var product in products)
                product.Active = false;

            _db.SaveChanges();

            return new EmptyResult();
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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}