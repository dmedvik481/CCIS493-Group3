using HaircutBookingSystem.Models;
using HaircutBookingSystem.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register services (must happen before Build)
builder.Services.AddControllersWithViews();

// Bind SMTP settings from configuration
var smtpSection = builder.Configuration.GetSection("Smtp");
var smtpOptions = smtpSection.Get<SmtpOptions>() ?? new SmtpOptions();
builder.Services.AddSingleton(smtpOptions);
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure middleware (after Build)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // if using Identity
app.UseAuthorization();

// Default route goes to Homepage
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();