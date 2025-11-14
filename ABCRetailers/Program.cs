// Program.cs
using ABCRetailers.Services;
using Microsoft.AspNetCore.Http.Features;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ABCRetailers.Data;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// 1. Services registration
// ----------------------

// MVC
builder.Services.AddControllersWithViews();

// Typed HttpClient for your Azure Functions
builder.Services.AddHttpClient("Functions", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Functions:BaseUrl"] ?? throw new InvalidOperationException("Functions:BaseUrl missing");
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/api/");
    client.Timeout = TimeSpan.FromSeconds(100);
});

// Use the typed client (replaces IAzureStorageService)
builder.Services.AddScoped<IFunctionsApi, FunctionsApiClient>();

// Optional: allow larger multipart uploads (images, proofs, etc.)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

// Optional: logging
builder.Services.AddLogging();

// ----------------------
// 2. DbContext
// ----------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ABCRetailersDb")));

// ----------------------
// 3. Authentication & Authorization
// ----------------------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// ----------------------
// 4. Session Support (MOVED HERE - BEFORE builder.Build())
// ----------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ----------------------
// 5. Build app
// ----------------------
var app = builder.Build();

// ----------------------
// 6. Culture settings
// ----------------------
var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// ----------------------
// 7. Middleware pipeline
// ----------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();  // Session middleware - now this will work
app.UseAuthentication();  // <-- MUST be before UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();