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

            var products = _db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Include("User").Where(x => x.Active == true).OrderByDescending(a => a.Date);
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
           
            var nrPerColumn = locationsList.Count() / 4;
            var surplus = locationsList.Count() - (nrPerColumn * 4);

            var s = 0;
            var counter = 0;
            if (locationsList.Count() > 0)
            {
                if (surplus > 0)
                {
                    s = 1;
                    surplus = surplus - 1;
                }
                ViewBag.LocationsList1 = locationsList.Take(nrPerColumn + s).ToList();
                counter = nrPerColumn + s;
                if (locationsList.Count() > counter)
                {
                    s = 0;
                    if (surplus > 0)
                    {
                        s = 1;
                        surplus = surplus - 1;
                    }
                    ViewBag.LocationsList2 = locationsList.Skip(counter).Take(nrPerColumn + s).ToList();
                    counter = counter + nrPerColumn + s;
                    if (locationsList.Count() > counter)
                    {
                        s = 0;
                        if (surplus > 0)
                        {
                            s = 1;
                            surplus = surplus - 1;
                        }
                        ViewBag.LocationsList3 = locationsList.Skip(counter).Take(nrPerColumn + s).ToList();
                        counter = counter + nrPerColumn + s;
                        if (locationsList.Count() > counter)
                        {
                            s = 0;
                            if (surplus > 0)
                            {
                                s = 1;
                                surplus = surplus - 1;
                            }
                            ViewBag.LocationsList4 = locationsList.Skip(counter).Take(nrPerColumn + s).ToList();
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

            //Tuple<NumeSubCategorie, NrLike-uri, Procent>
            bool interestsPresent = false;
            List<Tuple<string, int, int>> subcategoriesRm = new List<Tuple<string, int, int>>();

             var currentUser = User.Identity.GetUserId();
            if (currentUser != null)
            {
                var interests = from intr in _db.Interests
                                where intr.UserId == currentUser
                                select intr;
                var totalNrOfInterests = interests.Count();

                if (totalNrOfInterests != 0)
                {
                    interestsPresent = true;

                    var subcOfInterest = (from intr in _db.Interests
                                         where intr.UserId == currentUser
                                         select intr.Product.SubCategory).Distinct();
                    foreach(var subc in subcOfInterest)
                    {
                        var interestsForSubc = (from intr in _db.Interests
                                               where intr.Product.SubCategoryId == subc.SubCategoryId
                                               select intr).Count();
                        subcategoriesRm.Add(new Tuple<string, int, int>(subc.SubCategoryId.ToString(), interestsForSubc, (interestsForSubc * 100 / totalNrOfInterests)));
                    }

                    var subcategories = (from sub in _db.SubCategories
                                         select sub).ToList();
                    var remainingSubcat = subcategories.Where(p => !subcOfInterest.ToList().Any(p2 => p2.SubCategoryId == p.SubCategoryId));
                    foreach (var sub in remainingSubcat)
                    {
                        subcategoriesRm.Add(new Tuple<string, int, int>(sub.SubCategoryId.ToString(), 0, 0));
                    }

                    // Sortam subcategoriile dupa nr de vizualizari
                    subcategoriesRm.Sort((x, y) => -(x.Item2.CompareTo(y.Item2)));

                }
            }

            ViewBag.NoStatistic = false;
            var allproducts = from prd in _db.Products
                              select prd;
            List<Product> productsArray = new List<Product>();
            if (!interestsPresent)
            {
                ViewBag.NoStatistic = true;
                ViewBag.RecommendedProducts = null;

            }
            else
            {
                var auxProd = allproducts;
                List<Product> auxList = new List<Product>();

                foreach (Product p in auxProd)
                {
                    auxList.Add(p);
                }

                // adaugam elem la recomandari cat timp exista elem neadaugate
                while (auxList.Count() != 0)
                {
                    //elem vor fi adaugate in lista de recomandari in seturi
                    //fiecare iteratie a acestui for va reprezenta un set adaugat
                    //calculam nr de anunturi ce trb adaugate
                    //mereu verificam sa mai existe elem care trb adaugate altfel break
                    foreach (Tuple<string, int, int> t in subcategoriesRm)
                    {
                        int nrOfPrd = t.Item3 / 10;
                        if (nrOfPrd < 1)
                        {
                            nrOfPrd = 1;
                        }
                        for (int i = 0; i < nrOfPrd; i++)
                        {
                            for (var j = 0; j < auxList.Count(); j++)
                            {
                                if (auxList.ElementAt(j).SubCategoryId.ToString().Equals(t.Item1))
                                {
                                    productsArray.Add(auxList.ElementAt(j));
                                    auxList.RemoveAt(j); // elem a fost adaugat, deci il scoatem din lista
                                    break;
                                }
                            }
                            if (auxList.Count() == 0)
                            {
                                break;
                            }
                        }
                        if (auxList.Count() == 0)
                        {
                            break;
                        }
                    }
                }

                ViewBag.RecommendedProducts = productsArray.Take(40);
            }
                //utilizand nr de vizualizari
                /*bool statisticPresent = false;
                // Tuple<NumeCategorie, NrLike-uri, Procent>
                List<Tuple<string, int, int>> subcategoriesSt = new List<Tuple<string, int, int>>();

                var currentUser = User.Identity.GetUserId();
                if (currentUser != null)
                {
                    var statistic = from stat in _db.Statistics
                                    where stat.UserId == currentUser
                                    select stat;
                    var nrSubcategoriesViewed = statistic.Count();
                    var totalNrOfViews = 0;
                    if (nrSubcategoriesViewed != 0)
                    {
                        statisticPresent = true;
                        foreach (var st in statistic)
                        {
                            totalNrOfViews = totalNrOfViews + st.ViewCounter;
                        }
                        foreach (var st in statistic)
                        {
                            subcategoriesSt.Add(new Tuple<string, int, int>(st.SubCategoryId.ToString(), st.ViewCounter, (st.ViewCounter * 100 / totalNrOfViews)));
                        }
                        var subcategories = (from sub in _db.SubCategories
                                             select sub).ToList();
                        var remainingSubcat = subcategories.Where(p => !statistic.ToList().Any(p2 => p2.SubCategoryId == p.SubCategoryId));
                        foreach (var sub in remainingSubcat)
                        {
                            subcategoriesSt.Add(new Tuple<string, int, int>(sub.SubCategoryId.ToString(), 0, 0));
                        }

                        // Sortam subcategoriile dupa nr de vizualizari
                        subcategoriesSt.Sort((x, y) => -(x.Item2.CompareTo(y.Item2)));
                    }


                }

                ViewBag.NoStatistic = false;
                var allproducts = from prd in _db.Products
                                  select prd;
                List<Product> productsArray = new List<Product>();
                if (!statisticPresent)
                {
                    ViewBag.NoStatistic = true;
                    ViewBag.RecommendedProducts = null;

                }
                else
                {
                    var auxProd = allproducts;
                    List<Product> auxList = new List<Product>();

                    foreach (Product p in auxProd)
                    {
                        auxList.Add(p);
                    }

                    while (auxList.Count() != 0)
                    {
                        foreach (Tuple<string, int, int> t in subcategoriesSt)
                        {
                            int nrOfPrd = t.Item3 / 10;
                            if (nrOfPrd < 1)
                            {
                                nrOfPrd = 1;
                            }
                            for (int i = 0; i < nrOfPrd; i++)
                            {
                                for (var j = 0; j < auxList.Count(); j++)
                                {
                                    if (auxList.ElementAt(j).SubCategoryId.ToString().Equals(t.Item1))
                                    {
                                        productsArray.Add(auxList.ElementAt(j));
                                        auxList.RemoveAt(j);
                                        break;
                                    }
                                }
                                if (auxList.Count() == 0)
                                {
                                    break;
                                }
                            }
                            if (auxList.Count() == 0)
                            {
                                break;
                            }
                        }
                    }

                    ViewBag.RecommendedProducts = productsArray;

                }*/


                return View();
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCities()
        {

            //generate empty list
            var selectList = new List<SelectListItem>();

            var cities = (from cit in _db.Cities select cit).OrderBy(x => x.CityName);
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