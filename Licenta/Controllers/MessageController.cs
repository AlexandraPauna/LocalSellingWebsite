using Licenta.Models;
using Licenta.Models.Communication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Licenta.Controllers
{
    public class MessageController : Controller
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Message
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult New()
        {
            _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                Message message = new Message();

                return View(message);
            }

        }

        [HttpPost]
        public ActionResult New(Message message)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ownerUser = message.ReceiverId;
                    var currentUser = User.Identity.GetUserId();
                    message.SenderId = currentUser;

                    //check if conversation already exists
                    var conversationId = from conv in db.Conversations
                                       where (conv.SenderId.Equals(currentUser) && conv.ProductId.Equals(ownerUser) )
                                       select conv.ConversationId;
                    if(conversationId == null)
                    {
                        //add Conversation
                        Conversation conversation = new Conversation();
                        //conversation.ProductId = 

                        return RedirectToAction("Index", "Conversation");
                    }
                    else
                    {
                        message.ConversationId = Convert.ToInt32(conversationId);
                        db.Messages.Add(message);
                        db.SaveChanges();
                        TempData["message"] = "Mesaj trimis!";

                        return RedirectToAction("Index", "Conversation");
                    }

                }
                else
                {
                    return View(message);
                }

            }
            catch (Exception e)
            {
                return View();
            }
        }
    }
}