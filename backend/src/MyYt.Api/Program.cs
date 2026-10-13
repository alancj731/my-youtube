using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MyYt.Api.Endpoints;
using MyYt.Api.Infrastructure;
using MyYt.Application;
using MyYt.Application.Abstractions;
using MyYt.Infrastructure;
using MyYt.Infrastructure.Auth;
using MyYt.Infrastructure.Persistence;
using Scalar.AspNetCore;

// Local settings such as MYSQL_* and FIREBASE_PROJECT_ID live in backend/.env (not committed).
DotEnv.Load(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

var firebase = FirebaseAuthOptions.FromConfiguration(builder.Configuration);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Firebase ID tokens are standard OIDC JWTs; signing keys are discovered from the authority and cached.
        options.Authority = firebase.Issuer;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = firebase.Issuer,
            ValidAudience = firebase.ProjectId,
            NameClaimType = "name",
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"])
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<UserProvisioningMiddleware>();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapVideoEndpoints();
app.MapTagEndpoints();
// Reports database reachability too, so a blocked network path is visible right after a deploy.
app.MapGet("/api/health", async (AppDbContext db, CancellationToken ct) =>
        await db.Database.CanConnectAsync(ct)
            ? Results.Ok(new { status = "ok", database = "ok" })
            : Results.Json(new { status = "degraded", database = "unreachable" }, statusCode: StatusCodes.Status503ServiceUnavailable))
    .AllowAnonymous();

app.Run();
