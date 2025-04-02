using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafeVault;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("SafeVaultDb"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("UserRole", policy => policy.RequireRole("User"))
    .AddPolicy("AdminRole", policy => policy.RequireRole("Admin"));

builder.Services.AddControllers();

var app = builder.Build();

// Create roles and users on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = ["Admin", "User"];
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var exampleUsers = new[]
    {
        new { Username = "admin", Password = "Admin123!", Role = "Admin" },
        new { Username = "user", Password = "User123!", Role = "User" }
    };

    foreach (var exampleUser in exampleUsers)
    {
        if (await userManager.FindByNameAsync(exampleUser.Username) == null)
        {
            var user = new IdentityUser { UserName = exampleUser.Username, Email = exampleUser.Username };
            var result = await userManager.CreateAsync(user, exampleUser.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, exampleUser.Role);
            }
        }
    }
}

app.UseAuthentication();
app.UseAuthorization();


app.MapPost("/login", async (HttpContext context, SignInManager<IdentityUser> signInManager) =>
{
    var form = await context.Request.ReadFormAsync();

    string username = form["username"].ToString();
    string password = form["password"].ToString();

    if (!ValidationHelpers.IsValidInput(username) || !ValidationHelpers.IsValidInput(password))
    {
        return Results.BadRequest("Invalid input.");
    }

    var result = await signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);

    if (result.Succeeded)
    {
        return Results.Ok("Login successful");
    }

    return Results.Unauthorized();
});

app.MapGet("/public", () =>
{
    return Results.Ok("This is a public endpoint");
});

app.MapGet("/authenticated", () =>
{
    return Results.Ok("This is an authenticated endpoint");
}).RequireAuthorization();


app.MapGet("/user", () =>
{
    return Results.Ok("This is a user endpoint");
}).RequireAuthorization("UserRole");


app.MapGet("/admin", () =>
{
    return Results.Ok("This is an admin endpoint");
}).RequireAuthorization("AdminRole");

app.Run();

public partial class Program { }