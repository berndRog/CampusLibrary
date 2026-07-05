using System.Security.Claims;
using Asp.Versioning;
using CampusLibraryApi._1_Web.Security;
using CampusLibraryApi._2_BuildingBlocks._1_Ports;
using CampusLibraryApi.Configure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CampusLibraryApi;

public static class DiRoot {

   // Add JWT bearer authentication and simple CampusLibrary policies.
   public static IServiceCollection AddCampusLibraryAuthentication(
      this IServiceCollection services,
      IConfiguration configuration
   ) {
      var authority = configuration["IdentityAccessServer:Authority"]
         ?? configuration["IdentityAccessServer:IssuerUri"]
         ?? "https://localhost:7010";

      var audience = configuration["IdentityAccessServer:Audience"]
         ?? configuration["IdentityAccessServer:Resource"]
         ?? "campuslibrary-api";

      var requireHttpsMetadata = configuration.GetValue(
         "IdentityAccessServer:RequireHttpsMetadata",
         false
      );

      services.AddScoped<IIdentityGateway, IdentityGatewayHttpContext>();

      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(options => {
            options.Authority = authority;
            options.Audience = audience;
            options.RequireHttpsMetadata = requireHttpsMetadata;

            options.TokenValidationParameters = new TokenValidationParameters {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               NameClaimType = ClaimTypes.Name,
               RoleClaimType = "role"
            };
         });

      services.AddAuthorization(options => {
         options.AddPolicy(
            CampusLibraryPolicies.Reader,
            policy => policy.RequireAssertion(context =>
               context.User.Identity?.IsAuthenticated == true &&
               HasRole(context.User, "Reader", "student")
            )
         );

         options.AddPolicy(
            CampusLibraryPolicies.Employee,
            policy => policy.RequireAssertion(context =>
               context.User.Identity?.IsAuthenticated == true &&
               HasRole(context.User, "Employee")
            )
         );
      });

      return services;
   }
   
   // Add API versioning to services
   public static IServiceCollection AddApiReaderAndVersioning(
      this IServiceCollection services
   ) {
      var apiVersionReader = ApiVersionReader.Combine(
         new UrlSegmentApiVersionReader()
         // new HeaderApiVersionReader("x-api-version")
         // new MediaTypeApiVersionReader("x-api-version"),
         // new QueryStringApiVersionReader("api-version")
      );

      services.AddApiVersioning(options => {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            //          opt.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.ApiVersionReader = apiVersionReader;
         })
         .AddMvc()
         .AddApiExplorer(options => {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
         });

      return services;
   }

   // Add Swagger/OpenAPI to services
   public static IServiceCollection AddSwagger(
      this IServiceCollection services
   ) {
      services.AddEndpointsApiExplorer();

      // create SwaggerDoc(...) dynamically for all discovered API versions
      services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfigureOptions>();

      services.AddSwaggerGen(options => {
         // include XML docs from all copied XML files in output folder
         var basePath = AppContext.BaseDirectory;

         foreach (var xmlFile in Directory.EnumerateFiles(basePath, "*.xml")) {
            options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
         }

         // Use short schema names so DTO references stay readable in Swagger.
         options.CustomSchemaIds(type => type.Name.Replace("+", "."));

         // optional: remove version parameter from generated operation parameters
         options.OperationFilter<SwaggerRemoveVersionParameterFilter>();

         // normalize response content types and document custom ProblemDetails extensions
         options.OperationFilter<SwaggerNormalizeResponseContentTypesFilter>();
         options.SchemaFilter<SwaggerProblemDetailsSchemaFilter>();

         // optional: replace version placeholder in route templates
         options.DocumentFilter<SwaggerReplaceVersionWithExactValueInPathFilter>();
      });

      return services;
   }

   private static bool HasRole(
      ClaimsPrincipal user,
      params string[] acceptedRoles
   ) {
      foreach (var role in acceptedRoles) {
         if (user.IsInRole(role))
            return true;

         if (user.Claims.Any(claim =>
                (claim.Type == ClaimTypes.Role || claim.Type == "role" || claim.Type == "roles") &&
                string.Equals(claim.Value, role, StringComparison.OrdinalIgnoreCase)))
            return true;
      }

      return false;
   }
}
