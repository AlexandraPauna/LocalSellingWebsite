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
    public class ConversationController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Conversation
        public ActionResult Index(string sortType)
        {
            var currentUser = User.Identity.GetUserId();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if(sortType == "Received")
                {
                    var conversations = from conv in _db.Conversations.Include("Product").Include("User")
                                        where conv.Product.UserId == currentUser
                                        select conv;
                    ViewBag.conversations = conversations;

                    var latestMessages = new List<Message>();
                    foreach (var conv in conversations)
                    {
                        var message = (from mess in _db.Messages.Include("User")
                                       where mess.ConversationId == conv.ConversationId
                                       orderby mess.Date descending
                                       select mess).First();
                        latestMessages.Add(message);
                    }
                    //ordonare dupa mesaje a conversatiilor?
                }
                else
                if(sortType == "Send")
                {
                    var conversations = from conv in _db.Conversations.Include("Product").Include("User")
                                        where conv.SenderId == currentUser
                                        select conv;
                    ViewBag.conversations = conversations;

                    var latestMessages = new List<Message>();
                    foreach (var conv in conversations)
                    {
                        var message = (from mess in _db.Messages.Include("User")
                                       where mess.ConversationId == conv.ConversationId
                                       orderby mess.Date descending
                                       select mess).First();
                        latestMessages.Add(message);
                    }
                    //ordonare dupa mesaje a conversatiilor?
                }

                return View();
            }

        }

        public ActionResult Show(int id)
        {
            Conversation conversation = _db.Conversations.Find(id);

            var messages = from msg in _db.Messages.Include("User")
                           where msg.ConversationId == id
                           orderby msg.Date
                           select msg;

            var model = new MessageViewModel
            {
                ConversationId = conversation.ConversationId,
                ProductId = conversation.ProductId,
                SenderId = conversation.SenderId,
                Sender = conversation.Sender,
                Messages = messages.ToList()
            };


            var currentUser = User.Identity.GetUserId();
            ViewBag.CurrentUser = currentUser;

            if ((conversation.Product.UserId == currentUser) || (conversation.SenderId == currentUser))
            {
                return View(model); 
            }
            /*ViewBag.Received = true;
            if (conversation.Product.UserId == User.Identity.GetUserId())
            {
                return View(model); 
            }
            else
            if (conversation.SenderId == User.Identity.GetUserId())
            {
                ViewBag.Received = false; 
                return View(model);
            }*/
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
