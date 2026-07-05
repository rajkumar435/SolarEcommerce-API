//using Auth.Application.Interfaces;
//using Auth.Infrastructure.Data;
//using Auth.Infrastructure.Services;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();

////# ---------------- DB ----------------
//builder.Services.AddDbContext<AppDbContext>(opt =>
//    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

////# ---------------- JWT KEY ----------------
//var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]);

////# ---------------- AUTH ----------------
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

//builder.Services.AddAuthorization();

////# ---------------- DI REGISTRATION (IMPORTANT) ----------------
//builder.Services.AddScoped<IAuthService, AuthService>();
//builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

////# ---------------- SWAGGER ----------------
//builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });

//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer",
//        BearerFormat = "JWT",
//        In = ParameterLocation.Header
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

//var app = builder.Build();

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();













using Auth.Application.Interfaces;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

builder.Services.AddControllers();

// DB
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// JWT KEY
var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]);

// AUTH
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// DI
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
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
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AngularPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();