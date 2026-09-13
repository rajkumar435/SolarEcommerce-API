//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using System.Text;
//using Yarp.ReverseProxy;

//var builder = WebApplication.CreateBuilder(args);

//// CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AngularPolicy", policy =>
//    {
//        policy
//            .AllowAnyOrigin()
//            .AllowAnyHeader()
//            .AllowAnyMethod();
//    });
//});

//// Controllers
//builder.Services.AddControllers();

//// YARP Reverse Proxy
//builder.Services
//    .AddReverseProxy()
//    .LoadFromConfig(
//        builder.Configuration.GetSection("ReverseProxy"));

//// Swagger
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Swagger
//app.UseSwagger();
//app.UseSwaggerUI();

//// CORS
//app.UseCors("AngularPolicy");

//// HTTPS
////app.UseHttpsRedirection();

//// Authorization
//app.UseAuthorization();

//// Simple gateway health check
//app.MapGet("/", () => "API Gateway is running");

//// Controllers
//app.MapControllers();

//// YARP
//app.MapReverseProxy();

//app.Run();




using ApiGateway.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Http.Resilience;
using Polly;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// =====================================================
// CONTROLLERS
// =====================================================

builder.Services.AddControllers();


// =====================================================
// CIRCUIT BREAKER CONFIGURATION
// =====================================================

builder.Services.Configure<CircuitBreakerOptions>(
    builder.Configuration.GetSection("CircuitBreaker"));


// =====================================================
// HTTP CLIENT RESILIENCE
// =====================================================

builder.Services
    .AddHttpClient("ProductServiceClient")
    .AddResilienceHandler("product-resilience", (pipeline, context) =>
    {
        var configuration =
            context.ServiceProvider
                .GetRequiredService<IConfiguration>();

        var failureThreshold =
            configuration.GetValue<int>(
                "CircuitBreaker:FailureThreshold");

        var breakDuration =
            configuration.GetValue<int>(
                "CircuitBreaker:BreakDurationSeconds");


        pipeline.AddRetry(
            new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 2,

                Delay = TimeSpan.FromSeconds(1),

                BackoffType =
                    DelayBackoffType.Exponential,

                UseJitter = true
            });


        pipeline.AddCircuitBreaker(
            new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 1.0,

                MinimumThroughput =
                    failureThreshold,

                SamplingDuration =
                    TimeSpan.FromSeconds(10),

                BreakDuration =
                    TimeSpan.FromSeconds(
                        breakDuration)
            });
    });


// =====================================================
// YARP
// =====================================================

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(
        builder.Configuration.GetSection(
            "ReverseProxy"));


// =====================================================
// SWAGGER
// =====================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


var app = builder.Build();


// =====================================================
// SWAGGER
// =====================================================

app.UseSwagger();

app.UseSwaggerUI();


// =====================================================
// CORS
// =====================================================

app.UseCors("AngularPolicy");


// =====================================================
// AUTHORIZATION
// =====================================================

app.UseAuthorization();


// =====================================================
// HEALTH CHECK
// =====================================================

app.MapGet(
    "/",
    () => "API Gateway is running");


// =====================================================
// CONTROLLERS
// =====================================================

app.MapControllers();


// =====================================================
// YARP
// =====================================================

app.MapReverseProxy();


app.Run();