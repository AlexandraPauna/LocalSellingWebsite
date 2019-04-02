using Licenta.Common.Entities;
using Licenta.DataAccess;
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
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

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
                var conversations = from conv in _db.Conversations.Include("Product").Include("User")
                                    where conv.Product.UserId == currentUser
                                    select conv;
                ViewBag.conversations = conversations;

                var latestMessages = new List<Message>(); 
                foreach(var conv in conversations)
                {
                    var message = (from mess in _db.Messages.Include("User")
                                   where mess.ConversationId == conv.ConversationId
                                   orderby mess.Date descending
                                   select mess).First();
                    latestMessages.Add(message);
                }
                //ordonare dupa mesaje a conversatiilor?

                return View();
            }

        }

        public ActionResult Show(int id)
        {
            Conversation conversation = _db.Conversations.Find(id);

            if (conversation.Product.UserId == User.Identity.GetUserId())
            {
                return View(conversation);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Conversation conversation = _db.Conversations.Find(id);

            _db.Conversations.Remove(conversation);
            _db.SaveChanges();
            TempData["message"] = "Conversatia a fost stearsa!";

            return RedirectToAction("Index");
        }
    }
}
