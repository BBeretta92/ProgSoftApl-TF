using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using TF_PSA.Models;

[assembly: OwinStartupAttribute(typeof(PSA_TF_Final.Startup))]
namespace PSA_TF_Final
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            adicionaRoles();
        }


        private void adicionaRoles()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));



            if (!roleManager.RoleExists("Admin"))
            {


                var role = new IdentityRole();
                role.Name = "Admin";
                roleManager.Create(role);



                var user = new ApplicationUser();
                user.UserName = "admin";
                user.Email = "admin@admin.com";

                string userPWD = "Admin*123";

                var chkUser = UserManager.Create(user, userPWD);


                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");

                }
            }

            if (!roleManager.RoleExists("Gerente"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Gerente";
                roleManager.Create(role);

            }

            // creating Creating Manager role     
            if (!roleManager.RoleExists("Operador de Caixa"))
            {
                var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                role.Name = "Operador de Caixa";
                roleManager.Create(role);

            }

        }
    }
}
