using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Licenta.Common.Entities;
using Licenta.DataAccess;

namespace Licenta.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db = ApplicationDbContext.Create();
        // GET: Users
        [Authorize(Roles = "Administrator")]
        public ActionResult Index()
        {
            var users = from user in _db.Users
                        orderby user.UserName
                        select user;
            ViewBag.UsersList = users;
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(string id)
        {
            ApplicationUser user = _db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            ViewBag.NotVisible = _db.Roles.Any(x => x.Users.Any(y => y.UserId == id) && x.Name == "Administrator");

            return View(user);
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = _db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;

            
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;
                    var roles = from role in _db.Roles select role;
                    foreach (var role in roles)
                    {
                        UserManager.RemoveFromRole(id, role.Name);
                    }
                    var selectedRole = _db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    UserManager.AddToRole(id, selectedRole.Name);
                    _db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View(user);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(string id)
        {
            ApplicationUser user = _db.Users.Find(id);

            //fct ce trimite email utilizatorului anuntandu-l de contul sters
            var products = from prd in _db.Products
                           where prd.UserId == id
                           select prd;
            foreach(var product in products)
            {
                _db.Products.Remove(product);
            }

            var interests = from intrs in _db.Interests
                            where intrs.UserId == id
                            select intrs;
            foreach(var interest in interests)
            {
                _db.Interests.Remove(interest);
            }

            var ratings = from rtn in _db.Ratings
                          where rtn.UserId == id
                          select rtn;
            foreach(var rating in ratings)
            {
                _db.Ratings.Remove(rating);
            }
            var ratingsReceived = from rtn in _db.Ratings
                                  where rtn.RatedUserId == id
                                  select rtn;
            foreach(var ratingReceived in ratingsReceived)
            {
                _db.Ratings.Remove(ratingReceived);
            }

            var messagesSent = from msg in _db.Messages
                               where msg.SenderId == id
                               select msg;
            foreach (var messageSent in messagesSent)
            {
                _db.Messages.Remove(messageSent);
            }
            var messagesReceived = from msg in _db.Messages
                                   where msg.SenderId == id
                                   select msg;
            foreach (var messageReceived in messagesReceived)
            {
                _db.Messages.Remove(messageReceived);
            }

            var conversationsSent = from conv in _db.Conversations
                                    where conv.SenderId == id
                                    select conv;
            foreach(var conversationSent in conversationsSent)
            {
                _db.Conversations.Remove(conversationSent);
            }
            var conversationsReceived = from conv in _db.Conversations
                                        where conv.Product.UserId == id
                                        select conv;
            foreach (var conversationReceived in conversationsReceived)
            {
                _db.Conversations.Remove(conversationReceived);
            }

            _db.Users.Remove(user);
            _db.SaveChanges();
            TempData["message"] = "Userul a fost sters!";

            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();
            var roles = from role in _db.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

    }
}
