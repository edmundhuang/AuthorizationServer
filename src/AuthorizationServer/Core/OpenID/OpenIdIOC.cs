using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthorizationServer.Core.OpenID
{
    public static class OpenIdIOC
    {
        public static IServiceCollection AddDefaultOpenId<T>(this IServiceCollection services)
            where T : DbContext
        {
            services.Configure<IdentityOptions>(options =>
            {
                // Configure Identity to use the same JWT claims as OpenIddict instead
                // of the legacy WS-Federation claims it uses by default (ClaimTypes),
                // which saves you from doing the mapping in your authorization controller.
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.ClaimsIdentity.EmailClaimType = Claims.Email;

                // Note: to require account confirmation before login,
                // register an email sender service (IEmailSender) and
                // set options.SignIn.RequireConfirmedAccount to true.
                //
                // For more information, visit https://aka.ms/aspaccountconf.
                options.SignIn.RequireConfirmedAccount = false;
            });


            services.AddOpenIddict()
             .AddCore(options =>
             {
                 options.UseEntityFrameworkCore()
                        .UseDbContext<T>();
             })
             .AddServer(options =>
             {
                 // Enable the authorization, logout, token and userinfo endpoints.
                 options.SetAuthorizationEndpointUris("connect/authorize")
                    //.SetDeviceEndpointUris("connect/device")
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetLogoutEndpointUris("connect/logout")
                    .SetTokenEndpointUris("connect/token")
                    .SetUserinfoEndpointUris("connect/userinfo")
                    .SetVerificationEndpointUris("connect/verify");

                 options.AllowAuthorizationCodeFlow()
                        .AllowHybridFlow()
                        .AllowClientCredentialsFlow()
                        .AllowRefreshTokenFlow();

                 options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "dataEventRecords");

                 options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                 options.UseAspNetCore()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableTokenEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough()
                        .EnableStatusCodePagesIntegration();
             })
             .AddValidation(options =>
             {
                 options.UseLocalServer();
                 options.UseAspNetCore();
             });

            return services;
        }
    }
}
