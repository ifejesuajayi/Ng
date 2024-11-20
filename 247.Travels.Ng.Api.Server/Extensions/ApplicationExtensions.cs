using CloudinaryDotNet;
using Dna;
using Hangfire;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Xown.Travels.Core;
using Xown.Travels.Core.DomainServices.Operations;
using Xown.Travels.Core.Infrastructure.Process;
using Xown.Travels.Core.Verteil.Services;

namespace _247.Travels.Ng.Apis.Server
{
    /// <summary>
    /// Application extensions
    /// </summary>
    public static class ApplicationExtensions
    {
        /// <summary>
        /// Adds identity configuration to the <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/></param>
        /// <returns><see cref="IServiceCollection"/> for further chaining</returns>
        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    // base-address of your identityserver
                    options.Authority = Framework.Construction.Configuration["IdentityServer:Host"];
                    options.RequireHttpsMetadata = false;
                    // audience is optional, make sure you read the following paragraphs
                    // to understand your options
                    options.TokenValidationParameters.ValidateAudience = false;

                    // it's recommended to check the type header to avoid "JWT confusion" attacks
                    options.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
                });

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Configures the authorization policy for the APIs
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/></param>
        /// <returns><see cref="IServiceCollection"/> for further chaining</returns>
        public static IServiceCollection AddClientAuthorization(this IServiceCollection services)
        {
            // Add authorization
            services.AddAuthorization(options =>
            {
                // Configure customer scope
                options.AddPolicy(AuthorizationPolicy.Customer, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtClaimTypes.Scope,
                        $"{Framework.Construction.Configuration["IdentityServer:PartialScope"]}.{ClientScope.CustomerResources}");
                });

                // Configure admin scope
                options.AddPolicy(AuthorizationPolicy.Administrator, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtClaimTypes.Scope,
                        $"{Framework.Construction.Configuration["IdentityServer:PartialScope"]}.{ClientScope.AdministratorResources}");
                });
            });

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Configure app db context
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Configure the api behavior
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        // public static IServiceCollection ConfigureApiBehavior(this IServiceCollection services)
        // {
        //     services.Configure<ApiBehaviorOptions>(options =>
        //     {
        //         options.InvalidModelStateResponseFactory = actionContext =>
        //         {
        //             var errors = actionContext.ModelState.Where(x => x.Value.Errors.Count > 0)
        //             .SelectMany(x => x.Value.Errors)
        //             .Select(x => x.ErrorMessage).ToArray();

        //             var apiResponse = new ApiResponse
        //             {
        //                 ErrorMessage = "An error occurred",
        //                 ErrorResult = errors
        //             };

        //             return new BadRequestObjectResult(apiResponse);
        //         };
        //     });

        //     return services;
        // }

        /// <summary>
        /// Set up cors
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader()
                    .AllowAnyOrigin()
                    .AllowAnyMethod();
                });
            });

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Add our domain services to IOC container
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<AdminDomain>()
                .AddScoped<AirportsDomain>()
                .AddScoped<AirlinesDomain>()
                .AddScoped<PaymentHandler>()
                .AddScoped<ShoppingOperations>()
                .AddScoped<BookingOperations>()
                .AddScoped<PriceMarkupService>()
                .AddScoped<ActivityManagement>()
                .AddScoped<DistributionClient>()
                .AddScoped<DistributionService>()
                .AddScoped<ProviderDistributionService>()
                .AddScoped<CorporateCodesManagement>()
                .AddScoped<CoralPayService>()
                .AddScoped<NdcPriceMarkupProcess>()
                .AddScoped<NdcNgnPriceMarkupProcess>()
                .AddScoped<NgPriceMarkupProcess>()
                .AddScoped<UkPriceMarkupProcess>()
                .AddScoped<BrightSunPriceMarkupProcess>()
                .AddScoped<CurrencyConversion>()
                .AddScoped<NdcOperations>()
                .AddScoped<NgOperations>()
                .AddScoped<NgUsdOperations>()
                .AddScoped<TiqwaOperations>()
                .AddScoped<TiqwaService>()
                .AddScoped<TiqwaPriceMarkupProcess>()
                .AddScoped<TiqwaRoutes>()
                .AddScoped<NgUsdPriceMarkupProcess>()   
                .AddScoped<NdcNgOperations>()
                .AddScoped<VerteilClient>()
                .AddScoped<VerteilService>()
                .AddScoped<VerteilPriceMarkupProcess>()
                .AddScoped<NgTwoPriceMarkupProcess>()
                .AddScoped<IMarkupManager, NgPriceMarkupProcess>();

            // Return services for further chaining
            return services;
        }

        /// <summary>
        /// Configure hangfire 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static IServiceCollection ConfigureHangfire(this IServiceCollection services, IConfiguration config)
        {
            services.AddHangfire(options =>
            {
                options.UseSqlServerStorage(config.GetConnectionString("HangfireConnection"));
            }).AddHangfireServer();

            return services;
        }

        /// <summary>
        /// Add the background jobs to IOC conatiner
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
        {
            services.AddSingleton<AuthJobs>();

            return services;
        }

        /// <summary>
        /// Adds the cloudinary service to DI container
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/></param>
        /// <param name="config">The <see cref="IConfiguration"/></param>
        /// <returns></returns>
        public static IServiceCollection AddCloudinary(this IServiceCollection services, IConfiguration config)
        {
            // Add cloudinary as a scoped service
            services.AddScoped(x => new Cloudinary(new Account
            {
                ApiKey = config["Cloudinary:Key"],
                ApiSecret = config["Cloudinary:Secret"],
                Cloud = config["Cloudinary:Cloud"]
            }));

            services.AddScoped<CloudinaryService>();

            // Return services for further chaining
            return services;
        }
    }
}
