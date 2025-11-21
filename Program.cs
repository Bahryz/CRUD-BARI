using Microsoft.AspNetCore.Authentication.Cookies;
using GerenciadorAD_Web.Configurations;
using GerenciadorAD_Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração segura e Injeção de Dependência
builder.Services.Configure<AdConfig>(builder.Configuration.GetSection("AdConfig"));
builder.Services.AddScoped<IAdService, AdService>();

// --- CORREÇÃO DO LOGIN ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; 
        options.AccessDeniedPath = "/Login"; 
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });
// -------------------------

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();