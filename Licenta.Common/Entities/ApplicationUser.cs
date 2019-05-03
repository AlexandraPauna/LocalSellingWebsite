using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Licenta.Common.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int CityId { get; set; }
        public virtual City City { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }
        public byte[] UserPhoto { get; set; }
        public float? RatingScore { get; set; }
        public float? TimeScore { get; set; }
        public float? CommunicationScore { get; set; }
        public float? AccuracyScore { get; set; }

        public IEnumerable<SelectListItem> AllRoles { get; set; }
    }
}
