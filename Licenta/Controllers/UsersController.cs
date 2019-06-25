using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Licenta.Common.Entities;
using Licenta.DataAccess;
using System.Data.SqlClient;
using Licenta.Common.Models;
using PagedList;
using System.Threading.Tasks;

namespace Licenta.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _db = ApplicationDbContext.Create();
        private readonly EmailService _emailService = new EmailService();
        // GET: Users
        [Authorize(Roles = "Administrator")]
        public ActionResult Index(int? page)
        {
            /*var users = from user in _db.Users
                        orderby user.UserName
                        select user;
            ViewBag.UsersList = users;
            return View();*/
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var users = from user in _db.Users
                        orderby user.UserName
                        select user;

            int pageIndex = page ?? 1;
            int dataCount = 5;

            var model = new UsersViewModel { Users = users.ToList().ToPagedList(pageIndex, dataCount) };

            return View(model);
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
        public async Task<ActionResult> Delete(string id)
        {
            ApplicationUser user = _db.Users.Find(id);

            //fct ce trimite email utilizatorului anuntandu-l de contul sters
            var products = from prd in _db.Products
                           where prd.UserId == id
                           select prd;
            foreach(var product in products)
            {
                var productImages = from prdImg in _db.ProductImages
                                    where prdImg.ProductId == product.ProductId
                                    select prdImg;
                _db.ProductImages.RemoveRange(productImages);

                var productInterests = from prdIns in _db.Interests
                                       where prdIns.ProductId == product.ProductId
                                       select prdIns;
                _db.Interests.RemoveRange(productInterests);

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
                                   where msg.ReceiverId == id
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


            var context = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var userRole = userManager.GetRoles(user.Id).FirstOrDefault();
            //userManager.RemoveFromRoles(user.Id, userRole);
            _db.Database.ExecuteSqlCommand(@"delete from aspnetuserroles from aspnetuserroles ur 
                                             inner join aspnetroles r on r.id=ur.roleid inner join aspnetusers 
                                             u on u.id=ur.userid where r.name=@role and u.username=@user",
                                           new SqlParameter("@role", userRole),
                                           new SqlParameter("@user", user.UserName));

            _db.Users.Remove(user);
            _db.SaveChanges();
            TempData["message"] = "Userul a fost sters!";

            //trimitere email
            string content = "Buna " + user.UserName + ", \r\n" + "Contul tau a fost sters de un administrator! Din pacate toate datele personale din cadrul site-ului au fost sterse si nu mai pot fi recuperate."
                            + " Acest lucru s-a intamplat deoarece nu ai respectat normele comunitatii!";
            await _emailService.SendEmailAsync(user.Email, "site_anunturi@yahoo.com", "Site anunturi", "Cont sters", content);

            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();
            var roles = from role in _db.Roles where role.Name != "Editor" select role;
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
