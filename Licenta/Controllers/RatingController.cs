using Licenta.Common.Entities;
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
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Rating
        public ActionResult Index(string id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var user = (from usr in _db.Users
                       where usr.Id == id
                       select usr).Single();
            var ratings = (from rtg in _db.Ratings.Include("User").Include("RatedUser")
                           where rtg.RatedUserId == id
                           select rtg).OrderByDescending(r => r.Date);

            RatingViewModel model = new RatingViewModel { RatedUser = user, Ratings = ratings.ToList() };

            return View(model);
        }

        [HttpPost]
        public ActionResult New(Rating rating)
        {
            var currentUserId = User.Identity.GetUserId();

            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
                if(currentUserId == rating.RatedUserId)
                {
                    TempData["message"] = "Nu va puteti acorda calificativ!";
                    return Redirect(Request.UrlReferrer.ToString());
                }
                else
                {
                    var userFind = from rtn in _db.Ratings
                                   where rtn.UserId == currentUserId && rtn.RatedUserId == rating.RatedUserId
                                   select rtn;
                    if (userFind.Count() >= 1)
                    {
                        TempData["message"] = "Ati adaugat deja un calificativ!";
                        return Redirect(Request.UrlReferrer.ToString());
                    }
                    else
                    {

                        rating.UserId = currentUserId;
                        var currentUser = (from usr in _db.Users
                                           where usr.Id == currentUserId
                                           select usr).Single();
                        rating.User = currentUser;

                        var ratedUserId = rating.RatedUserId;
                        var ratedUser = (from usr in _db.Users
                                         where usr.Id == ratedUserId.ToString()
                                         select usr).Single();
                        rating.RatedUser = ratedUser;

                        double average = (double)(rating.Communication + rating.Accuracy + rating.Time) / 3;
                        rating.Average = Math.Round(average, 1);

                        if (ratedUser.RatingScore == null)
                        {
                            ratedUser.CommunicationScore = rating.Communication;
                            ratedUser.AccuracyScore = rating.Accuracy;
                            ratedUser.TimeScore = rating.Time;
                            ratedUser.RatingScore = rating.Average;
                        }
                        else
                        {
                            var numberOfRatings = (from rtng in _db.Ratings
                                                   where rtng.RatedUserId == rating.RatedUserId
                                                   select rtng).Count();
                            int newNumberOfRatings = (int)numberOfRatings + 1;

                            ratedUser.CommunicationScore = Math.Round(((double)ratedUser.CommunicationScore + (double)rating.Communication) / newNumberOfRatings, 1);
                            ratedUser.AccuracyScore = Math.Round(((double)ratedUser.AccuracyScore + (double)rating.Accuracy) / newNumberOfRatings, 1);
                            ratedUser.TimeScore = Math.Round(((double)ratedUser.TimeScore + (double)rating.Time) / newNumberOfRatings, 1);
                            ratedUser.RatingScore = Math.Round(((double)ratedUser.RatingScore + rating.Average) / newNumberOfRatings, 1);
                        }

                        try
                        {

                            _db.Ratings.Add(rating);
                            _db.SaveChanges();
                            TempData["message"] = "Calificativ adaugat!";

                            return RedirectToAction("Index", "Rating", new { id = rating.RatedUserId});
                            //return Redirect(Request.UrlReferrer.ToString());

                        }
                        catch (Exception e)
                        {

                            return Redirect(Request.UrlReferrer.ToString());
                        }
                    }
            
                }
        }
    }
}