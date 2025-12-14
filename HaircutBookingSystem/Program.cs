using System;
using System.Linq;
using System.Threading.Tasks;
using HaircutBookingSystem.Models;
using HaircutBookingSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

static async Task EnsureRootAdminAsync(IServiceProvider services, IConfiguration config)
{
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    var enabled = config.GetValue<bool>("RootAdmin:Enabled");
    var email = config["RootAdmin:Email"];
    var password = config["RootAdmin:Password"];
    const string adminRole = "Admin";

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return;

    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    var user = await userManager.FindByNameAsync(email)
               ?? await userManager.FindByEmailAsync(email);

    if (enabled)
    {
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                var msg = string.Join("; ", create.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new Exception($"Root admin create failed: {msg}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, adminRole))
            await userManager.AddToRoleAsync(user, adminRole);

        await userManager.SetLockoutEnabledAsync(user, false);
        await userManager.SetLockoutEndDateAsync(user, null);
    }
    else
    {
        if (user != null)
        {
            await userManager.SetLockoutEnabledAsync(user, true);
            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }
    }
}

/// <summary>
/// If this is a legacy database that already has your domain tables (Stylists/Services)
/// but is missing ASP.NET Identity tables, create Identity tables in-place.
/// This avoids dropping data and avoids "AspNetRoles doesn't exist" crashes.
/// </summary>
static async Task EnsureIdentityTablesExistAsync(ApplicationDbContext db)
{
    // If AspNetRoles exists, Identity is already set up
    var hasRolesTable = await db.Database
    .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles'")
    .SingleAsync();

    if (hasRolesTable > 0)
        return;

    // Create only the Identity tables needed by default Identity.
    // This is safe to run once and does NOT touch your existing domain tables.
    await db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'[dbo].[AspNetRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoles](
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END;

IF OBJECT_ID(N'[dbo].[AspNetUsers]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUsers](
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
    CREATE INDEX [EmailIndex] ON [dbo].[AspNetUsers] ([NormalizedEmail]);
END;

IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims](
        [Id] int IDENTITY(1,1) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]([RoleId]);
END;

IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims](
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]([UserId]);
END;

IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins](
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]([UserId]);
END;

IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles](
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]([RoleId]);
END;

IF OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens](
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers]([Id]) ON DELETE CASCADE
    );
END;
");
}

var builder = WebApplication.CreateBuilder(args);

// ---------- Database ----------
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=HaircutBookingSystem;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------- Identity ----------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// ---------- MVC / Razor Pages / Session ----------
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ---------- SMTP (still wired, even if unused) ----------
var smtpOptions = builder.Configuration.GetSection("Smtp").Get<SmtpOptions>() ?? new SmtpOptions();
builder.Services.AddSingleton(smtpOptions);
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

// ---------- Middleware ----------
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
app.UseSession();

// ---------- Endpoints ----------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ---------- Startup DB tasks ----------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    // 1) Bring legacy DB up to "Identity-ready" state (without dropping your existing tables)
    await EnsureIdentityTablesExistAsync(db);

    // 2) Apply EF migrations AFTER identity tables exist
    // NOTE: if you still have older migrations that try to create Stylists again,
    // you must remove/adjust those migrations before this will succeed.
    await db.Database.MigrateAsync();

    // 3) Ensure root admin account/role
    await EnsureRootAdminAsync(services, app.Configuration);
}

app.Run();
