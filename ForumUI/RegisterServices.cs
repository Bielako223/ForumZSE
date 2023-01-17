using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace ForumUI;

public static class RegisterServices
{
   public static void ConfigureServices(this WebApplicationBuilder builder)
   {
      // Add services to the container.
      builder.Services.AddRazorPages();
      builder.Services.AddServerSideBlazor().AddMicrosoftIdentityConsentHandler();
      builder.Services.AddMemoryCache();
      builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

      builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
         .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection(key: "AzureAdB2C"));

      builder.Services.AddAuthorization(options =>
      {
         options.AddPolicy(name: "Admin", policy =>
         {
            policy.RequireClaim(claimType: "jobTitle", allowedValues: "Admin");
         });
      });

      builder.Services.AddSingleton<IDbConnection, DbConnection>();
      builder.Services.AddSingleton<ICategoryData, MongoCategoryData>();
      builder.Services.AddSingleton<IPostData, MongoPostData>();
      builder.Services.AddSingleton<IuserData, MongouserData>();
      builder.Services.AddSingleton<ICommentData, MongoCommentData>();
   }
}