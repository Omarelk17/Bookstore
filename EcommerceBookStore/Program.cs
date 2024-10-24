using EcommerceBookStore.Data;
using EcommerceBookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add DbContext and Identity services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Use ApplicationUser instead of IdentityUser throughout the Identity setup
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Add role management
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Use ApplicationDbContext for Identity

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed roles and users at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    await SeedRolesAndUsers(roleManager, userManager);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Ensure Authentication middleware is used
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

await app.RunAsync();

// Role and user seeding method
async Task SeedRolesAndUsers(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
    string[] roleNames = { "Manager", "Sales Executive" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Create default Manager and Sales Executive accounts
    var managerEmail = "manager@example.com";
    var salesEmail = "sales@example.com";
    var password = "SecurePassword123!";

    if (await userManager.FindByEmailAsync(managerEmail) == null)
    {
        var manager = new ApplicationUser { UserName = managerEmail, Email = managerEmail };
        await userManager.CreateAsync(manager, password);
        await userManager.AddToRoleAsync(manager, "Manager");
    }

    if (await userManager.FindByEmailAsync(salesEmail) == null)
    {
        var salesExec = new ApplicationUser { UserName = salesEmail, Email = salesEmail };
        await userManager.CreateAsync(salesExec, password);
        await userManager.AddToRoleAsync(salesExec, "Sales Executive");
    }
}
