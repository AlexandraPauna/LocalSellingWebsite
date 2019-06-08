using Licenta.Common.Models;
using Licenta.DataAccess;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Controllers
{
    public class InterestsController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Interests
        public ActionResult Index(int? page)
        {
            var currentUser = User.Identity.GetUserId();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "AccountControler");
            }
            else
            {
                var interests = (from ints in _db.Interests.Include("Product")
                                 where ints.UserId == currentUser
                                 select ints).Where(x => x.Product.Active == true).OrderByDescending(x => x.Date);

                int pageIndex = page ?? 1;
                int dataCount = 5;
                var model = new InterestViewModel { Interests = interests.ToList().ToPagedList(pageIndex, dataCount) };

                var unreadMessages = (from mess in _db.Messages
                                      where mess.ReceiverId == currentUser && mess.Read == false
                                      select mess).Count();
                ViewBag.UnreadMessages = unreadMessages;

                var nrAds = _db.Products.Where(x => x.UserId == currentUser).Count();
                ViewBag.NrAds = nrAds;

                var nrRatings = _db.Ratings.Where(x => x.RatedUserId == currentUser).Count();
                ViewBag.NrRatings = nrRatings;

                var nrInterests = _db.Interests.Where(x => x.UserId == currentUser).Count();
                ViewBag.NrInterests = nrInterests;

                return View(model);
            }


        }

        [HttpPost]
        public ActionResult Delete(string[] productIDs)
        {

            foreach (string productID in productIDs)
            {
                int prodID;
                if (int.TryParse(productID, out prodID))
                {
                    //int prodId = int.Parse(productID);
                    var currentUserId = User.Identity.GetUserId();
                    var interest = from interests in _db.Interests
                                   where interests.ProductId == prodID && interests.UserId == currentUserId
                                   select interests;
                    if (interest.Count() == 1)
                    {
                        _db.Interests.Remove(interest.First());
                    }
                }
                

            }
            _db.SaveChanges();

            return Json(Url.Action("Index", "Interests"));
        }
    }
}