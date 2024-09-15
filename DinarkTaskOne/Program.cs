using DinarkTaskOne.Services;
using DinarkTaskOne.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.SqlServer;
using DinarkTaskOne.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.SchemaName = "dbo";
    options.TableName = "SessionCache";
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

// Register the SignService as a scoped dependency
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



// Add authorization services
builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Ensure context.Session and context.User are not null
    if (context.Session != null)
    {
        var userIdString = context.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdString) && context.User != null && context.User.Identity?.IsAuthenticated == true)
        {
            await context.SignOutAsync("CustomScheme");
            context.Response.Redirect("/Sign/Login");
            return;
        }
    }

    await next();
});

// Define default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
