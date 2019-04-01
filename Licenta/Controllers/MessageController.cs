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
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Message
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(int productId, Message message)
        {
            if (String.IsNullOrEmpty(message.Content))
            {
                return Redirect("Show/" + productId.ToString());
            }
            else
            {
                var currentUser = User.Identity.GetUserId();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    message.Date = DateTime.Now;
                    message.Read = false;
                    message.SenderId = currentUser;
                    var product = (from prod in _db.Products
                                   where prod.ProductId == productId
                                   select prod).Single();
                    message.ReceiverId = product.UserId;

                    //check if conversation already exists
                    var conversationId = from conv in _db.Conversations
                                         where (conv.SenderId.Equals(currentUser) && conv.ProductId.Equals(productId))
                                         select conv.ConversationId;


                    if (conversationId == null)
                    {
                        //add Conversation
                        Conversation conversation = new Conversation();
                        conversation.ProductId = productId;
                        conversation.SenderId = currentUser;
                        _db.Conversations.Add(conversation);

                        var newConversationId = from conv in _db.Conversations
                                                where (conv.SenderId.Equals(currentUser) && conv.ProductId.Equals(productId))
                                                select conv.ConversationId;


                        message.ConversationId = Convert.ToInt32(newConversationId);

                        _db.Messages.Add(message);
                        _db.SaveChanges();
                        TempData["message"] = "Mesaj trimis!";

                        return RedirectToAction("Index", "Conversation");
                    }
                    else
                    {
                        message.ConversationId = Convert.ToInt32(conversationId);

                        _db.Messages.Add(message);
                        _db.SaveChanges();
                        TempData["message"] = "Mesaj trimis!";

                        return RedirectToAction("Index", "Conversation");
                    }
                }


            }

        }
    }
}
