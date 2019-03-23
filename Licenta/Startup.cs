using System.Data.Entity;
using Licenta.Models;
using Licenta.Models.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System.Linq;

[assembly: OwinStartupAttribute(typeof(Licenta.Startup))]
namespace Licenta
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            // Se apeleaza o metoda in care se adauga contul de administrator si rolurile aplicatiei
            CreateAdminUserAndApplicationRoles();
        }

        private void CreateAdminUserAndApplicationRoles()
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!context.Cities.Any())
            {
                context.Cities.Add(new City { CityId = 1, CityName = "Bucuresti" });
            }

            // Se adauga rolurile aplicatiei
            if (!roleManager.RoleExists("Administrator"))
            {
                // Se adauga rolul de administrator
                var role = new IdentityRole { Name = "Administrator" };
                roleManager.Create(role);

                // se adauga utilizatorul administrator
                var user = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    City = context.Cities.ToArray().First()
                };

                var adminCreated = userManager.Create(user, "Administrator1!");
                if (adminCreated.Succeeded)
                {
                    userManager.AddToRole(user.Id, "Administrator");
                }
            }

            if (!roleManager.RoleExists("Editor"))
            {
                var role = new IdentityRole { Name = "Editor" };
                roleManager.Create(role);
            }

            if (!roleManager.RoleExists("User"))
            {
                var role = new IdentityRole { Name = "User" };
                roleManager.Create(role);
            }
        }
    }
}
