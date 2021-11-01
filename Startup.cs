using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Task4.Data;
using Task4.Entities;
using Task4.Hubs;
using Task4.Services;

namespace Task4
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("AuthSystem");
            services.AddDbContext<ApplicationDbContext>(config =>
                {
                    config.UseSqlServer(connectionString);
                })
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthentication()
                .AddFacebook(config =>
                {
                    config.AppId = Configuration["Authentication:Facebook:AppId"];
                    config.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                })
                .AddGoogle(config =>
                {
                    config.ClientId = Configuration["Authentication:Google:ClientId"];
                    config.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                })
                .AddOAuth("VK", "VKontakte", config =>
                {
                    config.ClientId = Configuration["Authentication:VKontakte:ClientId"];
                    config.ClientSecret = Configuration["Authentication:VKontakte:ClientSecret"];
                    config.ClaimsIssuer = "VKontakte";
                    config.CallbackPath = new PathString("/signin-vkontakte-token");
                    config.AuthorizationEndpoint = "https://oauth.vk.com/authorize";
                    config.TokenEndpoint = "https://oauth.vk.com/access_token";
                    config.SaveTokens = true;
                    config.Scope.Add("user.name");
                    config.Scope.Add("user.first_name");
                    config.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "user_id");
                    config.ClaimActions.MapJsonKey(ClaimTypes.Name, "user_first_name");
                    config.Events = new OAuthEvents()
                    {
                        OnCreatingTicket = context =>
                        {
                            context.RunClaimActions(context.TokenResponse.Response.RootElement);
                            return Task.CompletedTask;
                        }
                    };
                });

            services.ConfigureApplicationCookie(config =>
            {
                config.LoginPath = "/Menu/Index";
                config.AccessDeniedPath = "/Menu/AccessDein";
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Player", builder =>
                {
                    builder.RequireClaim(ClaimTypes.Role, "Player");
                });
            });

            services.AddSignalR();

            services.AddControllersWithViews();

            services.AddSingleton<IFileAccessService, FileAccessService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseHttpsRedirection();
            }
            else
            {
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Menu}/{action=Index}/{id?}");
                endpoints.MapHub<ChatHub>("/chat");
            });
        }
    }
}
