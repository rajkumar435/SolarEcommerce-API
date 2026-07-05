//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using Product.Infrastructure.Data;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();

//// ================= DB =================
//builder.Services.AddDbContext<ProductDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

//// ================= JWT KEY =================
//var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]);

//// ================= AUTHENTICATION =================
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,

//        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
//        ValidAudience = builder.Configuration["JwtSettings:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(key),
//        ClockSkew = TimeSpan.Zero
//    };
//});

//// ================= AUTHORIZATION (IMPORTANT) =================
//builder.Services.AddAuthorization();

//// ================= SWAGGER + JWT SUPPORT =================
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(options =>
//{
//    options.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Product API",
//        Version = "v1"
//    });

//    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header,
//        Description = "Enter: Bearer {your token}"
//    });

//    options.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            new string[] {}
//        }
//    });
//});

//// ================= BUILD APP =================
//var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();










using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product.Application.Interfaces;
using Product.Infrastructure.Data;
using Product.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
// Controllers
builder.Services.AddControllers();

// Database
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

// JWT
var key = Encoding.UTF8.GetBytes(
    builder.Configuration["JwtSettings:Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["JwtSettings:Issuer"],

            ValidAudience =
                builder.Configuration["JwtSettings:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(key),

            ClockSkew = TimeSpan.Zero
        };
});

builder.Services.AddAuthorization();

// DI
builder.Services.AddScoped<IProductService, ProductService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Product API",
            Version = "v1"
        });

    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter: Bearer {your JWT token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AngularPolicy");
app.UseStaticFiles();
// Authentication
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

app.Run();