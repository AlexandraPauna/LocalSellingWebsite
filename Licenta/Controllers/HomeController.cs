using System.IO;
using System.Linq;
using System.Web.Mvc;
using Licenta.DataAccess;

namespace Licenta.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var categories = from category in _db.Categories
                             orderby category.CategoryName
                             select category;

            ViewBag.Categories = categories;

            var products = _db.Products.Include("SubCategory").Include("City").Include("DeliveryCompany").Include("ProductState").Include("User").OrderByDescending(a => a.Date).Take(2);
            ViewBag.LatestProducts = products;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }
            return View();
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