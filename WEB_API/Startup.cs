using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WEB_API.Data;
using WEB_API.Models;
using WEB_API.Repository.Interface;
using WEB_API.Repository.ServiceClass;

namespace WEB_API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true; //Add this line
            services.AddDbContext<ApplicationDbContext>(option =>
            {
                option.UseMySql(Configuration.GetConnectionString("DefaultMySQLConnection"));
            });

            services.AddScoped<IAccount, AccountDBService>();
            services.AddScoped<IDeposit, DepositDBService>();
            services.AddScoped<ILoan, LoanDBService>();
            services.AddScoped<ICbill, CbillDBService>();
            services.AddScoped<IUserRepository, UserDBService>();
            services.AddTransient<IMenuMaster, MenuMasterDBService>();

            var key = Configuration.GetValue<string>("ApiSettings:Secret");

            services.AddIdentity<ApplicationUser, IdentityRole>()
      .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddAuthentication
       (options =>
       {
           options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
           options.DefaultChallengeScheme = "oidc";
       })
                 .AddCookie(options =>
                 {
                     options.Cookie.HttpOnly = true;
                     options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                     options.LoginPath = "/Auth/Login";
                     options.AccessDeniedPath = "/Auth/AccessDenied";
                     options.SlidingExpiration = true;
                 }).AddOpenIdConnect("oidc", options =>
                 {
                     options.Authority = Configuration["ServiceUrls:IdentityAPI"];
                     options.GetClaimsFromUserInfoEndpoint = true;
                     options.ClientId = "magic";
                     options.ClientSecret = "secret";
                     options.ResponseType = "code";
                     options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                     options.TokenValidationParameters.NameClaimType = "unique_name";
                     options.TokenValidationParameters.RoleClaimType = "role";
                     options.Scope.Add("magic");
                     options.SaveTokens = true;
                     options.RequireHttpsMetadata = false;

                     options.ClaimActions.MapJsonKey("role", "role");

                     options.Events = new OpenIdConnectEvents
                     {
                         OnRemoteFailure = context =>
                         {
                             context.Response.Redirect("/");
                             context.HandleResponse();
                             return Task.FromResult(0);
                         }
                     };
                 });


            services.AddAutoMapper(typeof(Mappings.Mapping));

            //services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
            // .AddAzureADBearer(options => Configuration.Bind("AzureAd", options));
            services.AddControllers(option =>
            {
                option.CacheProfiles.Add("Default30",
                   new CacheProfile()
                   {
                       Duration = 30
                   });
                //option.ReturnHttpNotAcceptable=true;
            }).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description =
                                "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
                                "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                "Example: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
        }
    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {

                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Import Excel API");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
