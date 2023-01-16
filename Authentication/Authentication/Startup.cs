using Authentication.Model.Exception;
using Authentication.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Authentication
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
            services.AddControllers();

            services.AddCors(c =>
            {
                c.AddPolicy("AllowAll", options => options.SetIsOriginAllowed(isOriginAllowed: _ => true)
                                                            .AllowAnyMethod()
                                                            .AllowAnyHeader()
                                                            .AllowCredentials());

                if (!string.IsNullOrEmpty(Configuration["PolicyUrl:idpPolicy"]))
                {
                    c.AddPolicy("idpPolicy", builder =>
                    {
                        builder.WithOrigins((Configuration["PolicyUrl:idpPolicy"]).Split(","))
                        .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                    });
                }

            });

            //services.AddDataProtection().
            //    PersistKeysToFileSystem(new DirectoryInfo(Configuration["Cookie:KeyRingFolder"])).
            //    ProtectKeysWithCertificate(new X509Certificate2(Configuration["Cookie:KeyProtectedCertificate"], Configuration["Cookie:KeyProtectedCertificatePwd"])).
            //    SetApplicationName(Configuration["Cookie:CommonApplicationName"]);

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  options.Cookie.HttpOnly = true;
                  options.Cookie.SecurePolicy = true
                    ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
                  options.Cookie.SameSite = SameSiteMode.Lax;
                  options.Cookie.Name = Configuration["Cookie:CommonCookieName"];
                  options.Cookie.Path = "/";

                  // This is to work cookie for sub domains. this is required when things are getting properly distributed.
                  if (!string.IsNullOrEmpty(Configuration["Cookie:Domain"]))
                      options.Cookie.Domain = Configuration["Cookie:Domain"];
              });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
                options.HttpOnly = HttpOnlyPolicy.None;
                options.Secure = true
                  ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Washme.IdentityProvider", Version = "v1" });
            });

            services.AddHttpContextAccessor();
            services.AddScoped(typeof(IUserManagementService), typeof(UserManagementService));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AESA.IdentityProvider.EFE v1"));
            }

            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;

                if (exception is AuthenticationFailure)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    var result = JsonConvert.SerializeObject(exception);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(result);
                }

            }));

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
