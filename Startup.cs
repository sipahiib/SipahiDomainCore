using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SipahiDomainCore.Validators;
using System;
using System.Globalization;

namespace SipahiDomainCore
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
            services.AddControllersWithViews();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(1);
            });
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddControllersWithViews().AddFluentValidation();
            services.AddTransient<IValidator, ContactFormValidator>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //türkçe karakterler çıkması için (descriptions.json)
            var cultureInfo = new CultureInfo("tr-TR");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "About",
                    template: "{Controller=Home}/{action=About}");

                routes.MapRoute(
                    name: "Contact",
                    template: "{Controller=Home}/{action=Contact}");


                routes.MapRoute(
                    name: "Login",
                    template: "{Controller=Security}/{action=Login}");

                routes.MapRoute(
                    name: "PasswordRemember",
                    template: "{Controller=Security}/{action=PasswordRemember}");

                routes.MapRoute(
                    name: "Default",
                    template: "{Controller=Home}/{action=Index}/{id?}");
            });



            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});
        }
    }
}
