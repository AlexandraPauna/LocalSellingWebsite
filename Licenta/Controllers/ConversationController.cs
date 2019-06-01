using Licenta.Common.Entities;
using Licenta.Common.Models;
using Licenta.DataAccess;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
            var userId = User.Identity.GetUserId();
            var unreadMessages = (from mess in _db.Messages
                                  where mess.ReceiverId == userId && mess.Read == false
                                  select mess).Count();
            ViewBag.UnreadMessages = unreadMessages;

            var nrAds = _db.Products.Where(x => x.UserId == userId).Count();
            ViewBag.NrAds = nrAds;

            var nrRatings = _db.Ratings.Where(x => x.RatedUserId == userId).Count();
            ViewBag.NrRatings = nrRatings;

            var nrInterests = _db.Interests.Where(x => x.UserId == userId).Count();
            ViewBag.NrInterests = nrInterests;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            if (sortType == null)
            {
                sortType = "Unread";
            }
            var currentUser = User.Identity.GetUserId();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                if(sortType == "Received")
                {
                    var conversations = (from c in _db.Conversations.Include("Product").Include("Sender")
                                        join m in _db.Messages.Include("Sender").Include("User")
                                        on c.ConversationId equals m.ConversationId
                                        where c.Product.UserId == currentUser
                                        select new { c, m } into x
                                        group x by new { x.c } into g
                                        select new
                                        { Conversation = g.Key.c,
                                          Message = g.Select(x => x.m).Where(y => y.ConversationId == g.Key.c.ConversationId).OrderByDescending(m => m.Date),
                                          MessageDate = g.Select(x => x.m).Max(x => x.Date)}).OrderByDescending(y => y.MessageDate);

                    var conversationsMes = new List<ConversationMessage>();
                    foreach(var conversation in conversations)
                    {
                        var convMes = new ConversationMessage { Conversation = conversation.Conversation ,
                                                               LatestMessage = conversation.Message.Where(m => m.ConversationId== conversation.Conversation.ConversationId).First()
                                                              };

                        convMes.LatestMessage.Content = MessageController.Decrypt(convMes.LatestMessage.Content);
                        conversationsMes.Add(convMes);
                    }
                    var model = new ConversationViewModel { Conversations = conversationsMes };

                    return View(model);
                }
                else
                if(sortType == "Sent")
                {
                    var conversations = (from c in _db.Conversations.Include("Product").Include("Sender")
                                         join m in _db.Messages.Include("Sender").Include("User")
                                         on c.ConversationId equals m.ConversationId
                                         where c.SenderId == currentUser
                                         select new { c, m } into x
                                         group x by new { x.c } into g
                                         select new
                                         {
                                             Conversation = g.Key.c,
                                             Message = g.Select(x => x.m).Where(y => y.ConversationId == g.Key.c.ConversationId).OrderByDescending(m => m.Date),
                                             MessageDate = g.Select(x => x.m).Max(x => x.Date)
                                         }).OrderByDescending(y => y.MessageDate);

                    var conversationsMes = new List<ConversationMessage>();
                    foreach (var conversation in conversations)
                    {
                        var convMes = new ConversationMessage
                        {
                            Conversation = conversation.Conversation,
                            LatestMessage = conversation.Message.Where(m => m.ConversationId == conversation.Conversation.ConversationId).First()
                        };
                        convMes.LatestMessage.Content = MessageController.Decrypt(convMes.LatestMessage.Content);
                        conversationsMes.Add(convMes);
                    }
                    var model = new ConversationViewModel { Conversations = conversationsMes };

                    return View(model);
                }
                else
                if(sortType == "Unread")
                {
                    var conversations = (from c in _db.Conversations.Include("Product").Include("Sender")
                                         join m in _db.Messages.Include("Sender").Include("User")
                                         on c.ConversationId equals m.ConversationId
                                         where m.ReceiverId == currentUser && m.Read == false
                                         select new { c, m } into x
                                         group x by new { x.c } into g
                                         select new
                                         {
                                             Conversation = g.Key.c,
                                             Message = g.Select(x => x.m).Where(y => y.ConversationId == g.Key.c.ConversationId).OrderByDescending(m => m.Date),
                                             MessageDate = g.Select(x => x.m).Max(x => x.Date)
                                         }).OrderByDescending(y => y.MessageDate);

                    var conversationsMes = new List<ConversationMessage>();
                    foreach (var conversation in conversations)
                    {
                        var convMes = new ConversationMessage
                        {
                            Conversation = conversation.Conversation,
                            LatestMessage = conversation.Message.Where(m => m.ConversationId == conversation.Conversation.ConversationId).First()
                        };
                        convMes.LatestMessage.Content = MessageController.Decrypt(convMes.LatestMessage.Content);
                        conversationsMes.Add(convMes);
                    }
                    var model = new ConversationViewModel { Conversations = conversationsMes };

                    return View(model);
                }

                return View();
            }

        }

        public ActionResult Show(int id)
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var currentUser = User.Identity.GetUserId();
            ViewBag.CurrentUser = currentUser;

            Conversation conversation = _db.Conversations.Find(id);

            var messages = from msg in _db.Messages.Include("Sender").Include("Receiver")
                           where msg.ConversationId == id
                           orderby msg.Date
                           select msg;

            if ((conversation.Product.UserId == currentUser) || (conversation.SenderId == currentUser))
            {
                var readMessages = messages.Where(m => (m.ReceiverId == currentUser) && (m.Read == false)).ToList();

                if (readMessages.Count() > 0)
                {
                    foreach (var messageRead in readMessages)
                    {
                        messageRead.Read = true;
                    }
                    /*foreach (var message in messages)
                        _db.Entry(message).Property(x => x.Content).IsModified = false;*/

                    _db.SaveChanges();
                }
            }

            foreach (var message in messages)
            {
                message.Content = MessageController.Decrypt(message.Content);
                //_db.Entry(message).Property(x => x.Content).IsModified = false;
            };

            var model = new MessageViewModel
            {
                ConversationId = conversation.ConversationId,
                ProductId = conversation.ProductId,
                Product = conversation.Product,
                SenderId = conversation.SenderId,
                Sender = conversation.Sender,
                Messages = messages.ToList()
            };


            if ((conversation.Product.UserId == currentUser) || (conversation.SenderId == currentUser))
            {
                return View(model); 
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        /*[HttpDelete]
        public ActionResult Delete(int id)
        {
            Conversation conversation = _db.Conversations.Find(id);

            _db.Conversations.Remove(conversation);
            _db.SaveChanges();
            TempData["message"] = "Conversatia a fost stearsa!";

            return RedirectToAction("Index");
        }*/
    }
}
