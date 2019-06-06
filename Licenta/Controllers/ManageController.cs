using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Licenta.Common.Entities;
using Licenta.Common.Models;
using Licenta.DataAccess;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Licenta.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [AllowAnonymous]
        public async Task<JsonResult> UserAlreadyExistsAsync(string userName)
        {
            var result = await UserManager.FindByNameAsync(userName);
            string currentUserName = User.Identity.Name;
            if (currentUserName.CompareTo(userName) == 0)
            {
                result = null;
            }
            return Json(result == null, JsonRequestBehavior.AllowGet);
        }

        //foarte vechi
        // GET: /Manage/Index
        /*public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }*/

        public ActionResult Delete()
        {
            return View();
        }
       

        [HttpDelete]
        public ActionResult DeleteAccount()
        {
            var id = User.Identity.GetUserId();

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            ApplicationUser user = _db.Users.Find(id);

            //fct ce trimite email utilizatorului anuntandu-l de contul sters
            var products = from prd in _db.Products
                           where prd.UserId == id
                           select prd;
            foreach (var product in products)
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
            foreach (var interest in interests)
            {
                _db.Interests.Remove(interest);
            }

            var ratings = from rtn in _db.Ratings
                          where rtn.UserId == id
                          select rtn;
            foreach (var rating in ratings)
            {
                _db.Ratings.Remove(rating);
            }
            var ratingsReceived = from rtn in _db.Ratings
                                  where rtn.RatedUserId == id
                                  select rtn;
            foreach (var ratingReceived in ratingsReceived)
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
            foreach (var conversationSent in conversationsSent)
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
            TempData["message"] = "Cont sters!";

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            var userId = User.Identity.GetUserId();

            var unreadMessages = (from mess in _db.Messages
                                  where mess.ReceiverId == userId && mess.Read == false
                                  select mess).Count();
            ViewBag.UnreadMessages = unreadMessages;

            var adViews = _db.Products.GroupBy(a => a.UserId).Select(x => new
            {
                UserId = x.Key,
                Value = x.Sum((c => c.Views)),

            }).Where(x => x.UserId == userId).FirstOrDefault();
            if (adViews != null)
                ViewBag.AdViews = (int)adViews.Value;
            else
                ViewBag.AdViews = 0;

            var nrAds = _db.Products.Where(x => x.UserId == userId).Count();
            ViewBag.NrAds = nrAds;

            var nrRatings = _db.Ratings.Where(x => x.RatedUserId == userId).Count();
            ViewBag.NrRatings = nrRatings;

            var nrInterests = _db.Interests.Where(x => x.UserId == userId).Count();
            ViewBag.NrInterests = nrInterests;

            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        public ActionResult Index()
        {
            _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "AccountControler");
            }
            else
            {
                ApplicationUser user = _userManager.FindByIdAsync(userId).Result;
                user.Cities = GetAllCities();

                var recentProducts = from prod in _db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User")
                                     where prod.UserId.Equals(userId)
                                     orderby prod.Date
                                     select prod;
                ViewBag.RecentProducts = recentProducts.Take(3);

                var unreadMessages = (from mess in _db.Messages
                                     where mess.ReceiverId == userId && mess.Read == false
                                     select mess).Count();
                ViewBag.UnreadMessages = unreadMessages;

                var adViews = _db.Products.GroupBy(a => a.UserId).Select(x => new
                    {
                        UserId = x.Key,
                        Value = x.Sum((c => c.Views)),

                    }).Where(x => x.UserId == userId).FirstOrDefault();
                if (adViews != null)
                    ViewBag.AdViews = (int)adViews.Value;
                else
                    ViewBag.AdViews = 0;

                var nrAds = _db.Products.Where(x => x.UserId == userId).Count();
                ViewBag.NrAds = nrAds;

                var nrRatings = _db.Ratings.Where(x => x.RatedUserId == userId).Count();
                ViewBag.NrRatings = nrRatings;

                var nrInterests = _db.Interests.Where(x => x.UserId == userId).Count();
                ViewBag.NrInterests = nrInterests;

                return View(user);
            }
        }

        //merge intre Index si ChangeProfile
       /* [HttpGet]
        public ActionResult Index()
        {
            _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "AccountControler");
            }
            else
            {
                ApplicationUser user = _userManager.FindByIdAsync(userId).Result;
                user.Cities = GetAllCities();

                var recentProducts = from prod in _db.Products.Include("City").Include("SubCategory").Include("ProductState").Include("DeliveryCompany").Include("ProductImages").Include("User")
                                     where prod.UserId.Equals(userId)
                                     orderby prod.Date
                                     select prod;
                ViewBag.RecentProducts = recentProducts.Take(3);

                return View(user);
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index([Bind(Exclude = "UserPhoto")]ApplicationUser model)
        {
            _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "AccountControler");
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = UserManager.FindById(User.Identity.GetUserId());

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.UserName = model.UserName;
                user.PhoneNumber = model.PhoneNumber;
                user.CityId = model.CityId;

                byte[] imageData = null;
                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase poImgFile = Request.Files["UserPhoto"];

                    using (var binary = new BinaryReader(poImgFile.InputStream))
                    {
                        imageData = binary.ReadBytes(poImgFile.ContentLength);
                    }

                    //UserPhoto is not updated if no file is chosen
                    if (imageData.Length > 0)
                    {
                        user.UserPhoto = imageData;
                    }
                }

                UserManager.Update(user);

                //sign in user automatically after change of UserName
                await SignInManager.SignInAsync(user, true, true);

                //return RedirectToAction("Profile", "Account");
                return RedirectToAction("Index", "Manage");
            }
        }*/

        //
        //GET: /Manage/ChangeProfile
        public ActionResult ChangeProfile()
        {
            _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "AccountControler");
            }
            else
            {
                ApplicationUser user = _userManager.FindByIdAsync(userId).Result;

                var model = new ChangeProfileViewModel();
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.UserName = user.UserName;
                model.CityId = user.CityId;
                
                model.PhoneNumber = user.PhoneNumber;
                //model.UserPhoto = user.UserPhoto;
                model.Cities = GetAllCities();

                var unreadMessages = (from mess in _db.Messages
                                      where mess.ReceiverId == userId && mess.Read == false
                                      select mess).Count();
                ViewBag.UnreadMessages = unreadMessages;

                var adViews = _db.Products.GroupBy(a => a.UserId).Select(x => new
                {
                    UserId = x.Key,
                    Value = x.Sum((c => c.Views)),

                }).Where(x => x.UserId == userId).FirstOrDefault();
                if (adViews != null)
                    ViewBag.AdViews = (int)adViews.Value;
                else
                    ViewBag.AdViews = 0;

                var nrAds = _db.Products.Where(x => x.UserId == userId).Count();
                ViewBag.NrAds = nrAds;

                var nrRatings = _db.Ratings.Where(x => x.RatedUserId == userId).Count();
                ViewBag.NrRatings = nrRatings;

                var nrInterests = _db.Interests.Where(x => x.UserId == userId).Count();
                ViewBag.NrInterests = nrInterests;

                return View(model);
            }
        }

        //
        // POST: /Manage/ChangeProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeProfile([Bind(Exclude = "UserPhoto")]ChangeProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = UserManager.FindById(User.Identity.GetUserId());

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;
            user.PhoneNumber = model.PhoneNumber;
            user.CityId = model.CityId;

            byte[] imageData = null;
            if (Request.Files.Count > 0)
            {
                HttpPostedFileBase poImgFile = Request.Files["UserPhoto"];

                using (var binary = new BinaryReader(poImgFile.InputStream))
                {
                    imageData = binary.ReadBytes(poImgFile.ContentLength);
                }

                //UserPhoto is not updated if no file is chosen
                if (imageData.Length > 0)
                {
                    user.UserPhoto = imageData;
                }
            }

            UserManager.Update(user);

            //sign in user automatically after change of UserName
            await SignInManager.SignInAsync(user, true, true);

            //return RedirectToAction("Profile", "Account");
            return RedirectToAction("Index", "Manage");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCities()
        {
            // se genereaza o lista goala
            var selectList = new List<SelectListItem>();
            //se extrag orasele din baza de date
            var cities = from cit in _db.Cities select cit;
            //se itereaza prin orase
            foreach (var city in cities)
            {
                //se adauga in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = city.CityId.ToString(),
                    Text = city.CityName.ToString()
                });
            }
            //se returneaza lista de orase
            return selectList;
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}