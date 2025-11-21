using GerenciadorAD_Web.Configurations;
using GerenciadorAD_Web.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE SEGURANÇA E AMBIENTE (K8S) ---
builder.Services.Configure<AdConfig>(builder.Configuration.GetSection("AdConfig"));

// --- 2. INJEÇÃO DE DEPENDÊNCIA (DI) ---
builder.Services.AddScoped<IAdService, AdService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();