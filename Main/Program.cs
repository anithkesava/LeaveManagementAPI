using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;
using RealTime_APIDev.Entity;
using RealTime_APIDev.JWT;
using RealTime_APIDev.Layer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
using Microsoft.OpenApi.Models;
namespace RealTime_APIDev.Main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your token in the text box below.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });

            /* from here the jwt is started */
            var Key = builder.Configuration["jwt:key"];
            if (string.IsNullOrWhiteSpace(Key))
            {
                throw new Exception("key is null in invalid in appSettings.json file");
            }
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["jwt:issuer"],
                    ValidAudience = builder.Configuration["jwt:audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
                };
            });
            builder.Services.AddScoped<GenerateToken>();
            /*to here*/

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnections"));
            });            
            builder.Services.AddScoped<IEmployeeService, EmployeeService>();            
            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
