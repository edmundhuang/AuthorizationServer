using AuthorizationServer.Core.OpenID;
using AuthorizationServer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Serilog;

namespace AuthorizationServer
{
    public static class IOC
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"));

                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddTransient<DbContext, ApplicationDbContext>();

            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddDefaultUI();

            services.AddDefaultOpenId<ApplicationDbContext>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "OpenID Server", Version = "v1" });

                options.AddSecurityDefinition("Authentication", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OpenIdConnect,
                    Description = "Description",
                    In = ParameterLocation.Header,
                    Name = HeaderNames.Authorization,
                    Flows = new OpenApiOAuthFlows
                    {
                        ClientCredentials = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("/connect/token", UriKind.Relative),
                            TokenUrl = new Uri("/connect/token", UriKind.Relative)
                        }
                    },
                    OpenIdConnectUrl = new Uri("/.well-known/openid-configuration", UriKind.Relative)
                });

                options.AddSecurityRequirement(
                                    new OpenApiSecurityRequirement
                                    {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                        { Type = ReferenceType.SecurityScheme, Id = "oauth" },
                                },
                                Array.Empty<string>()
                            }
                                    }
                                );
            });

          

            services.AddRazorPages();

            services.AddHostedService<Worker>();

            return services;
        }

        public static WebApplication ConfigApplication(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenID Server");
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            

            return app;
        }
    }
}
