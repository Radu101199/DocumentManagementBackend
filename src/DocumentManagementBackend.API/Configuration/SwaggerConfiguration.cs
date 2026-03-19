using Microsoft.OpenApi.Models;
using System.Reflection;

namespace DocumentManagementBackend.API.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Document Management API",
                Version = "v1",
                Description = """
                    API for managing documents with approval workflows.
                    
                    ## Authentication
                    Use the `/api/auth/login` endpoint to obtain a JWT token, 
                    then click **Authorize** and enter: `Bearer <your-token>`
                    
                    ## Roles
                    - **User** — can create and manage own documents
                    - **Admin** — can approve/reject documents
                    """,
                Contact = new OpenApiContact
                {
                    Name = "Document Management Team",
                    Email = "api@documentmanagement.com"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            options.OrderActionsBy(api => $"{api.ActionDescriptor.RouteValues["controller"]}_{api.HttpMethod}");
            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Management API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Document Management API";
            options.DefaultModelsExpandDepth(2);
            options.DefaultModelExpandDepth(2);
            options.DisplayRequestDuration();
            options.EnableFilter();
        });

        return app;
    }
}
