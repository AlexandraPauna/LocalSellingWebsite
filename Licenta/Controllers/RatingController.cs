using Licenta.Common.Entities;
using Licenta.Common.Models;
using Licenta.DataAccess;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Controllers
{
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private readonly EmailService _emailService = new EmailService();

        // GET: Rating
        /*public ActionResult Index(string id)
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
        }*/

        public ActionResult Index(string sortType)
        {
            var id = User.Identity.GetUserId();
            
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            if (id == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var user = (from usr in _db.Users
                            where usr.Id == id
                            select usr).Single();
                var ratingsReceived = (from rtg in _db.Ratings.Include("User").Include("RatedUser")
                                       where rtg.RatedUserId == id
                                       select rtg).OrderByDescending(r => r.Date);
                var ratingsGiven = (from rtg in _db.Ratings.Include("User").Include("RatedUser")
                                    where rtg.UserId == id
                                    select rtg).OrderByDescending(r => r.Date);

                RatingViewModel model = new RatingViewModel{ };
                if (sortType == null || sortType == "Received")
                {
                    model.Ratings = ratingsReceived.ToList();
                }
                if (sortType == "Given")
                {
                    model.Ratings = ratingsGiven.ToList();
                }
                else
                {
                    //pagina eroare
                }
                
                return View(model);
            }
           
        }

        [HttpPost]
        //public ActionResult New(Rating rating)
        public async Task<ActionResult> New(Rating rating)
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

                            //send email
                            string content = "Buna " + rating.RatedUser.UserName + ", \n" + "Ai primit un calificativ nou de la " + rating.User.UserName + ".";
                            await _emailService.SendEmailAsync(rating.RatedUser.Email, "site_anunturi@yahoo.com", "Site anunturi", "Calificativ nou", content);

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

        public ActionResult Edit(int id)
        {
            Rating rating = _db.Ratings.Find(id);
            ViewBag.Rating = rating;

            if (rating.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator") || User.IsInRole("Editor"))
            {
                return View(rating);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui calificativ care nu va apartine!";
                return Redirect(Request.UrlReferrer.ToString());
            }
        }

        [HttpPut]
        //public ActionResult Edit(int id, Rating requestRating)
        public async Task<ActionResult> Edit(int id, Rating requestRating)
        {
            try
            {
                Rating rating = _db.Ratings.Find(id);

                var ratedUserId = rating.RatedUserId;
                var ratedUser = (from usr in _db.Users
                                 where usr.Id == ratedUserId.ToString()
                                 select usr).Single();
                var numberOfRatings = (from rtng in _db.Ratings
                                       where rtng.RatedUserId == rating.RatedUserId
                                       select rtng).Count();
                ratedUser.CommunicationScore = ratedUser.CommunicationScore * numberOfRatings - rating.Communication;
                ratedUser.AccuracyScore = ratedUser.AccuracyScore * numberOfRatings - rating.Accuracy;
                ratedUser.TimeScore = ratedUser.TimeScore * numberOfRatings - rating.Time;
                ratedUser.RatingScore = ratedUser.RatingScore * numberOfRatings - rating.Average;

                rating.Communication = requestRating.Communication;
                rating.Accuracy = requestRating.Accuracy;
                rating.Time = requestRating.Time;
                rating.Text = requestRating.Text;

                double average = (double)(requestRating.Communication + requestRating.Accuracy + requestRating.Time) / 3;
                rating.Average = Math.Round(average, 1);

                ratedUser.CommunicationScore = Math.Round(((double)ratedUser.CommunicationScore + (double)rating.Communication) / numberOfRatings, 1);
                ratedUser.AccuracyScore = Math.Round(((double)ratedUser.AccuracyScore + (double)rating.Accuracy) / numberOfRatings, 1);
                ratedUser.TimeScore = Math.Round(((double)ratedUser.TimeScore + (double)rating.Time) / numberOfRatings, 1);
                ratedUser.RatingScore = Math.Round(((double)ratedUser.RatingScore + rating.Average) / numberOfRatings, 1);


                _db.SaveChanges();
                TempData["message"] = "Calificativul a fost modificat cu succes!";

                //send email
                string content = "Buna " + rating.RatedUser.UserName + ", \n" + "Calificativul primit de la  " + rating.User.UserName + " a fost modificat!";
                await _emailService.SendEmailAsync(rating.RatedUser.Email, "site_anunturi@yahoo.com", "Site anunturi", "Calificativ modificat", content);


                //return RedirectToAction("Index", new { id = rating.RatedUserId});
                return RedirectToAction("UserProfile", "Account", new { id = rating.RatedUserId });
            }
            catch (Exception e)
            {
                return View();
            }
        }
    }
}