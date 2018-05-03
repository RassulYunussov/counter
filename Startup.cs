using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using counter.Authorization;
using counter.Data;
using counter.Hubs;
using counter.Middlewares;
using counter.Models;
using counter.Observers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace counter
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
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlite(Configuration.GetConnectionString("default"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

             // Register the Identity services.
            services.AddIdentity<ApplicationUser, IdentityRole>(options=>{
                        options.Password.RequireDigit = false;
                        options.Password.RequiredLength = 1;
                        options.Password.RequireLowercase = false;
                        options.Password.RequireUppercase = false;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequiredUniqueChars = 1;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

             // Register the validation handler, that is used to decrypt the tokens.
            services.AddAuthentication(options =>{
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddOAuthValidation();
            // Register the OpenIddict services.
            services.AddOpenIddict(options=>{
                     options.AddEntityFrameworkCoreStores<ApplicationDbContext>();
                     options.AddMvcBinders();
                     options.EnableTokenEndpoint("/connect/token");
                     options.AllowPasswordFlow().AllowRefreshTokenFlow();
                     options.DisableHttpsRequirement();
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("EditPolicy", policy =>
                    policy.Requirements.Add(new SameOwnerRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, BusinessObjectAuthorizationHandler>();

            services.AddMvc();
            services.AddSignalR();
            services.AddSingleton<StatsObservables>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(options=>{
                options.AllowAnyHeader();
                options.AllowAnyMethod();
                options.AllowAnyOrigin();
                options.AllowCredentials();
            });
            app.UseSignalRQueryStringAuth();
            app.UseAuthentication();
            app.UseSignalR(options=>{
                options.MapHub<StatsHub>("/Stats");
            });
            app.UseMvc();
            Seed(app);
        }
        private async Task Seed(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                // Get an instance of the DbContext from the DI container
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    // perform database delete
                    if(await context.Database.EnsureCreatedAsync())
                    {
                        UserManager<ApplicationUser> um = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                        RoleManager<IdentityRole> rm = serviceScope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
                        await rm.CreateAsync(new IdentityRole("Administrator"));
                        await rm.CreateAsync(new IdentityRole("Owner"));
                        await rm.CreateAsync(new IdentityRole("Operator"));
                        //Create trainer account
                        var user = new ApplicationUser { UserName = "owner", Email ="owner@counter.com" };
                        var ir = await um.CreateAsync(user, "owner");
                        if (ir.Succeeded)
                        {
                            await um.AddToRoleAsync(user,"Owner");
                            await um.AddClaimAsync(user,new Claim(OpenIdConnectConstants.Claims.Subject,user.Id));
                        }
                        var oper = new ApplicationUser { UserName = "operator", Email ="operator@counter.com" };
                        oper.Owner = user;
                        ir = await um.CreateAsync(oper, "operator");
                        if (ir.Succeeded)
                        {
                            await um.AddToRoleAsync(oper,"Operator");
                            await um.AddClaimAsync(oper,new Claim(OpenIdConnectConstants.Claims.Subject,oper.Id));
                        }
                        var bp = new BusinessPoint{Name="Batut",Owner = user,Location="Park", Price = 500, Duration = 15};
                        context.Add(bp);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
