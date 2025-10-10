using ERPSystem.Data;
using ERPSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configurar EF con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 Registrar servicios personalizados
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<AuditService>();

// 🔹 Controladores y vistas (MVC)
builder.Services.AddControllersWithViews();

// 🔹 Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";               // ✅ aseguramos que apunte al método correcto
        options.LogoutPath = "/Login/Logout";             // ✅ si luego lo implementamos
        options.AccessDeniedPath = "/Home/AccessDenied";  // opcional
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;                 // 🔹 renueva el tiempo si el usuario sigue activo
    });

// 🔹 Agregar soporte para sesión
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 🔹 Configurar pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Activar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// 🔹 Ruta por defecto: login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();



//using ERPSystem.Data;
//using ERPSystem.Services;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// Configurar EF con SQL Server
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
////Agregar el servicio para exportar
//builder.Services.AddScoped<ERPSystem.Services.ExportService>();

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//// Configurar autenticación con cookies
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        options.LoginPath = "/Login"; // Página de login
//        options.AccessDeniedPath = "/Login/AccessDenied"; // Página acceso denegado
//        options.ExpireTimeSpan = TimeSpan.FromHours(8);
//    });

//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<AuditService>();


//builder.Services.AddSession();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}



//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseSession();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();
