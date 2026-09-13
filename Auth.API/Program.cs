using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder =
    WebApplication.CreateBuilder(args);


// ================================================================
// CORS
// ================================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AngularPolicy",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// ================================================================
// CONTROLLERS
// ================================================================

builder.Services.AddControllers();


// ================================================================
// DATABASE
// ================================================================

builder.Services.AddDbContext<AppDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString("Default")));


// ================================================================
// JWT CONFIGURATION
// ================================================================

var jwtKey =
    builder.Configuration[
        "JwtSettings:Key"];

var jwtIssuer =
    builder.Configuration[
        "JwtSettings:Issuer"];

var jwtAudience =
    builder.Configuration[
        "JwtSettings:Audience"];


if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JwtSettings:Key is missing.");
}


if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JwtSettings:Issuer is missing.");
}


if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JwtSettings:Audience is missing.");
}


// ================================================================
// AUTHENTICATION
// ================================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ValidateIssuer = true,

                ValidIssuer = jwtIssuer,

                ValidateAudience = true,

                ValidAudience = jwtAudience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,

                RequireExpirationTime = true,

                RequireSignedTokens = true
            };
    });

// ================================================================
// AUTHORIZATION
// ================================================================

builder.Services.AddAuthorization();


// ================================================================
// DI
// ================================================================

builder.Services.AddScoped<
    IAuthService,
    AuthService>();


builder.Services.AddScoped<
    IJwtTokenGenerator,
    JwtTokenGenerator>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();
// ================================================================
// SWAGGER
// ================================================================

builder.Services
    .AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(
    c =>
    {
        c.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name =
                    "Authorization",

                Type =
                    SecuritySchemeType.Http,

                Scheme =
                    "bearer",

                BearerFormat =
                    "JWT",

                In =
                    ParameterLocation.Header,

                Description =
                    "Enter JWT token"
            });


        c.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                            new OpenApiReference
                            {
                                Type =
                                    ReferenceType
                                        .SecurityScheme,

                                Id =
                                    "Bearer"
                            }
                    },

                    Array.Empty<string>()
                }
            });
    });


// ================================================================
// BUILD
// ================================================================

var app =
    builder.Build();


// ================================================================
// SWAGGER
// ================================================================

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();
// ================================================================
// MIDDLEWARE
// ================================================================

app.UseCors(
    "AngularPolicy");


// VERY IMPORTANT
// Authentication MUST come before Authorization.

app.UseAuthentication();

app.UseAuthorization();


// ================================================================
// CONTROLLERS
// ================================================================

app.MapControllers();


app.Run();