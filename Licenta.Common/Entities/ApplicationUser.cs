using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required(ErrorMessage = "Orasul este obligatoriu!")]
        public int CityId { get; set; }
        public virtual City City { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }

        public byte[] UserPhoto { get; set; }

        public double? RatingScore { get; set; }
        public double? TimeScore { get; set; }
        public double? CommunicationScore { get; set; }
        public double? AccuracyScore { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public IEnumerable<SelectListItem> AllRoles { get; set; }
    }
}
