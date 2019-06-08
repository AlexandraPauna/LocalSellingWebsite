using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System.Linq;
using Licenta.Common.Entities;
using Licenta.DataAccess;
using System;

[assembly: OwinStartupAttribute(typeof(Licenta.Startup))]
namespace Licenta
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            // Se apeleaza o metoda in care se adauga datele necesare initializarii aplicatiei
            CreateInitData();
        }

        private void CreateInitData()
        {
            var context = new ApplicationDbContext();
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // Se adauga un oras
            if (!context.Cities.Any())
            {
                context.Cities.Add(new City { CityId = 1, CityName = "Bucuresti" });
            }

            // Se adauga starile produsului
            if (!context.ProductState.Any())
            {
                context.ProductState.Add(new ProductState { ProductStateId = 1, ProductStateName = "Nou" });
                context.ProductState.Add(new ProductState { ProductStateId = 2, ProductStateName = "Utilizat" });
            }

            if (!roleManager.RoleExists("Administrator"))
            {
                // Se adauga rolul de administrator
                var role = new IdentityRole { Name = "Administrator" };
                roleManager.Create(role);

                // Se adauga utilizatorul administrator
                var user = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    City = context.Cities.ToList().First(),
                    Date = (DateTime)DateTime.Now
                };

                var adminCreated = userManager.Create(user, "Administrator1!");
                if (adminCreated.Succeeded)
                {
                    userManager.AddToRole(user.Id, "Administrator");
                }
            }

            // Se adauga rolul de editor
            if (!roleManager.RoleExists("Editor"))
            {
                var role = new IdentityRole { Name = "Editor" };
                roleManager.Create(role);
            }

            // Se adauga rolul de user
            if (!roleManager.RoleExists("User"))
            {
                var role = new IdentityRole { Name = "User" };
                roleManager.Create(role);
            }
        }
    }
}
