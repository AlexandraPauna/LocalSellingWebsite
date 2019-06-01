using Licenta.Common.Models;
using Licenta.DataAccess;
using Microsoft.AspNet.Identity;
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
        public ActionResult Index()
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
                                 select ints).OrderByDescending(x => x.Date);

                var model = new InterestViewModel { Interests = interests.ToList() };

                var unreadMessages = (from mess in _db.Messages
                                      where mess.ReceiverId == currentUser && mess.Read == false
                                      select mess).Count();
                ViewBag.UnreadMessages = unreadMessages;

                var adViews = _db.Products.GroupBy(a => a.UserId).Select(x => new
                {
                    UserId = x.Key,
                    Value = x.Sum((c => c.Views)),

                }).Where(x => x.UserId == currentUser).FirstOrDefault();
                if (adViews != null)
                    ViewBag.AdViews = (int)adViews.Value;
                else
                    ViewBag.AdViews = 0;

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