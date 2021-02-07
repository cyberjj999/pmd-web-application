using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace PMDWebApplication.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        //To add this attribute to the database without deleting tables
        //Just go Package Manager Console and type
        //enable migration
        //after that type add-migration [attribute], eg add-migration gender
        //then type update-database

        [Display(Name = "Full Name"), Required]
        [StringLength(100)]
        public string FullName { get; set; }

        public string Uimg { get; set; }

        //Useful vid to explain about migration https://www.youtube.com/watch?v=i7SDd5JcjN4
        //Always add code inside the model first, then add-migration (whatever name)
        //Then update-database. The column will be created automatically

        //Adding ? to make it nullable
        public DateTime? AccessFailDate { get; set; }

        public DateTime PasswordLastChangedDate { get; set; }
        public int Age { get; set; }

        public bool isVerified { get; set; }

        public string PromoCode { get; set; }

        public DateTime? PasswordGeneratedDate { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }


      /*  public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Student> Students { get; set; }*/
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        //This were used for a tutorial
        //public DbSet<UserClass> UserClasses { get; set; }

        public DbSet<RoleViewModel> RoleViewModels { get; set; }

        public DbSet<SecurityConfigurationsViewModel> SecurityConfigurationsViewModels { get; set; }

        //public DbSet<ClaimInsuranceViewModel> ClaimInsuranceViewModels { get; set; }


    }
}