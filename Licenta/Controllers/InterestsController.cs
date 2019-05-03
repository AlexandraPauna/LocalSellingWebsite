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
                
                return View(model);
            }

               
        }
    }
}