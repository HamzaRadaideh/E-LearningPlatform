using DinarkTaskOne.Data;
using DinarkTaskOne.Services;
using DinarkTaskOne.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddMvcOptions(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())) // Auto CSRF validation
    .AddRazorRuntimeCompilation(); // Enable Razor runtime compilation for views

// Add distributed memory cache and session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.SchemaName = "dbo";
    options.TableName = "SessionCache";
    options.DefaultSlidingExpiration = TimeSpan.FromMinutes(30); // Set sliding expiration for cached items
});

// Configure session management
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add health checks for database and distributed cache
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);


// Register the SignService and EnrollmentService as scoped dependencies
builder.Services.AddScoped<ISignService, SignService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Configure custom authentication scheme using cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CustomScheme";
    options.DefaultChallengeScheme = "CustomScheme";
    options.DefaultSignInScheme = "CustomScheme";
})
.AddCookie("CustomScheme", options =>
{
    options.LoginPath = "/Sign/Login";
    options.LogoutPath = "/Sign/Logout";
    options.AccessDeniedPath = "/Sign/AccessDenied";
});

// Add authorization policies for Admin and Instructor
builder.Services.AddAuthorizationBuilder()
                                                          // Add authorization policies for Admin and Instructor
                                                          .AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"))
                                                          // Add authorization policies for Admin and Instructor
                                                          .AddPolicy("InstructorOnly", policy =>
        policy.RequireRole("Instructor"));

// Add Razor Pages support
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Middleware to check session and authentication consistency
app.Use(async (context, next) =>
{
    var userIdString = context.Session.GetString("UserId");

    if (context.User.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(userIdString))
    {
        await context.SignOutAsync("CustomScheme");
        context.Response.Redirect("/Sign/Login");
        return;
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// Define default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
