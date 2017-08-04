using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;

namespace PeriodAid.Models
{
    // 可以通过向 ApplicationUser 类添加更多属性来为用户添加配置文件数据。若要了解详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=317594。
    public class ApplicationUser : IdentityUser
    {
        [StringLength(256)]
        public string ImgUrl { get; set; }

        [StringLength(64)]
        public string City { get; set; }

        [StringLength(64)]
        public string Province { get; set; }

        public bool Sex { get; set; }

        [StringLength(64)]
        public string OpenId { get; set; }

        [StringLength(32)]
        public string NickName { get; set; }

        [StringLength(256)]
        public string AccessToken { get; set; }

        [StringLength(64)]
        public string OffSalesSystem { get; set; }

        public int DefaultSystemId { get; set; }

        public int DefaultSellerId { get; set; }
        
        public int Credits { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // 请注意，authenticationType 必须与 CookieAuthenticationOptions.AuthenticationType 中定义的相应项匹配
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在此处添加自定义用户声明
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("MembershipConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}