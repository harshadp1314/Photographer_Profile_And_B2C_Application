using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Owin;
using Microsoft.Owin;


[assembly: OwinStartup(typeof(UploadMusic.App_Start.Startup))]

namespace UploadMusic.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "http://pixthonprofile.com", //some string, normally web url,  
                        ValidAudience = "http://pixthonprofile.com",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("RoshHarsh@1314 Jaan Hai Tu Meri"))
                    }
                });
        }
    }
}
