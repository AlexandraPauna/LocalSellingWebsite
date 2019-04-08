﻿using Licenta.Common.Entities;
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
        public ActionResult New(int? id, Message message)
        {
            if (id != null)
            {
                if (String.IsNullOrEmpty(message.Content))
                {
                    return Redirect("Show/" + id.ToString());
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
                        var sender = (from usr in _db.Users
                                      where usr.Id == currentUser
                                      select usr).Single();
                        message.Sender = sender;
                        var product = (from prod in _db.Products
                                       where prod.ProductId == id
                                       select prod).Single();
                        message.ReceiverId = product.UserId;
                        message.Receiver = product.User;

                        //check if conversation already exists
                        var conversationId = from conv in _db.Conversations
                                             where (conv.SenderId ==currentUser && conv.ProductId == id)
                                             select conv.ConversationId;


                        if (conversationId.Count() == 0)
                        {
                            //add Conversation
                            Conversation conversation = new Conversation();
                            conversation.ProductId = Convert.ToInt32(id);
                            conversation.Product = product;
                            conversation.SenderId = currentUser;

                            conversation.Sender = sender;
                            _db.Conversations.Add(conversation);
                            _db.SaveChanges();

                            var newConversationId = (from conv in _db.Conversations
                                                     where ((conv.SenderId == currentUser) && (conv.ProductId == id))
                                                     select conv.ConversationId).First();


                            message.ConversationId = Convert.ToInt32(newConversationId);

                            _db.Messages.Add(message);
                            _db.SaveChanges();
                            TempData["message"] = "Mesaj trimis!";

                            return RedirectToAction("Index", "Conversation");
                        }
                        else
                        {
                            message.ConversationId = Convert.ToInt32(conversationId.Single());

                            _db.Messages.Add(message);
                            _db.SaveChanges();
                            TempData["message"] = "Mesaj trimis!";

                            return RedirectToAction("Index", "Conversation");
                        }
                    }
                }
            }
            else
            {
                if (String.IsNullOrEmpty(message.Content))
                {
                    return Redirect("Show/" + message.ConversationId.ToString());
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

                        //look for receiver user id
                        var conversation = (from conv in _db.Conversations.Include("Sender").Include("Product")
                                            where conv.ConversationId == message.ConversationId
                                            select conv).Single();
                        //var conversation = message.Conversation;
                        if (message.SenderId == currentUser) //the receiver is either the buyer
                            message.ReceiverId = conversation.SenderId;
                        else //either the seller
                            message.ReceiverId = conversation.Product.UserId;

                        _db.Messages.Add(message);
                        _db.SaveChanges();
                        TempData["message"] = "Mesaj trimis!";

                        return Redirect(Request.UrlReferrer.ToString());
                    }
                }

            }
            /*
            [HttpPost]
            public ActionResult New(Message message)
            {
                if (String.IsNullOrEmpty(message.Content))
                {
                    return Redirect("Show/" + message.ConversationId.ToString());
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

                        //look for receiver user id
                        var conversation = message.Conversation;
                        if (conversation.SenderId == currentUser) //the receiver is either the buyer
                            message.ReceiverId = conversation.SenderId;
                        else //either the seller
                            message.ReceiverId = message.Conversation.Product.UserId;

                        _db.Messages.Add(message);
                        _db.SaveChanges();
                        TempData["message"] = "Mesaj trimis!";

                        return Redirect(Request.UrlReferrer.ToString());
                    }
                }

            }*/

        }
    }
}