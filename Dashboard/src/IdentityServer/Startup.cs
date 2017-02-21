using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer.Configurations;
using IdentityServer.DAL;
using Microsoft.EntityFrameworkCore;
using IdentityServer.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityServer4.Services;
using IdentityServer.Services;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using IdentityServer.Models.Enums;

namespace IdentityServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            //if (env.IsEnvironment("Development"))
            //{
            //    // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
            //    builder.AddApplicationInsightsSettings(developerMode: true);
            //}

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            
            // Add framework services.
            services.AddDbContext<IdentityServerDbContext>(options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("IdentityServer.DAL")));
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<IdentityServerDbContext>()
                .AddDefaultTokenProviders();

            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            // Add application services.
            services.AddTransient<IProfileService, IdentityWithAdditionalClaimsProfileService>();

            // configure identity server with in-memory stores, keys, clients and scopes
            services.AddIdentityServer()
               //.AddInMemoryApiResources(Resources.GetApiResources())
               //.AddInMemoryClients(Clients.Get())
               .AddTemporarySigningCredential()
               .AddAspNetIdentity<AppUser>()
               .AddProfileService<IdentityWithAdditionalClaimsProfileService>()
               .AddConfigurationStore(
                   builder => builder.UseSqlServer(connectionString,
                   options => options.MigrationsAssembly("IdentityServer.DAL")))
               .AddOperationalStore(
                   builder => builder.UseSqlServer(connectionString,
                   options => options.MigrationsAssembly("IdentityServer.DAL")));
               
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();

            // this will do the initial DB population
            InitializeDatabase(app);

            app.UseIdentity();
            app.UseIdentityServer();
            app.UseMvc();
        }

        // The first database initialization
        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                scope.ServiceProvider.GetRequiredService<IdentityServerDbContext>().Database.Migrate();
                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                // intitial clients       
                if (!context.Clients.Any())
                {
                    foreach (var client in Clients.Get())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                // intitial resources
                if (!context.ApiResources.Any())
                {
                    foreach (var apiResource in Resources.GetApiResources())
                    {
                        context.ApiResources.Add(apiResource.ToEntity());
                    }
                    context.SaveChanges();
                }
                
                // intitial roles       
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                if (!roleManager.Roles.Any())
                {
                    roleManager.CreateAsync(new IdentityRole(EnumRoles.SuperAdmin.ToString()));
                }

                // intitial test users 
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
                if (!userManager.Users.Any())
                {
                    foreach (var inMemoryUser in TestUsers.Get())
                    {
                        var identityUser = new AppUser(inMemoryUser.Username);                        
                        userManager.CreateAsync(identityUser, inMemoryUser.Password).Wait();
                        userManager.AddToRoleAsync(identityUser, EnumRoles.SuperAdmin.ToString()).Wait(); // Set user role "Superadmin"
                    }
                }
            }
        }
    }
}
