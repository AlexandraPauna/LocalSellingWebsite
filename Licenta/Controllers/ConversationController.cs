using Licenta.Models;
using Licenta.Models.Communication;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Controllers
{
    public class ConversationController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Conversation
        public ActionResult Index()
        {
            var currentUser = User.Identity.GetUserId();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var conversations = from conv in db.Conversations.Include("Product").Include("User")
                                    where conv.Product.UserId == currentUser
                                    select conv;
                ViewBag.conversations = conversations;

                //var latestMessages = from message in db.Messages
                 //                  where
                                   

                return View();
            }
                
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Conversation conversation = db.Conversations.Find(id);

            db.Conversations.Remove(conversation);
            db.SaveChanges();
            TempData["message"] = "Conversatia a fost stearsa!";

            return RedirectToAction("Index");
        }
    }
}