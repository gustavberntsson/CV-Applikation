using CV_Applikation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databasePath = Path.Combine(Directory.GetCurrentDirectory(), "Databas");
AppDomain.CurrentDomain.SetData("DataDirectory", databasePath);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<UserContext>(options =>
            options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("UserContext")));
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<UserContext>().AddDefaultTokenProviders();

var app = builder.Build();
var supportedCultures = new[] { "sv-SE", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("sv-SE")  
    .AddSupportedCultures(supportedCultures) 
    .AddSupportedUICultures(supportedCultures);



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
