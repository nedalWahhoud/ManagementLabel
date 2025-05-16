using ManagementLabel.Components;
using ManagementLabel.LogIn;
using Blazored.LocalStorage;
using ManagementLabel.Data;
using ManagementLabel.ProductsF;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Authorization
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost/APIs/")
});
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddSingleton<CustomAuthStateProvider.MainRenderState>();

// storage
builder.Services.AddBlazoredLocalStorage();

// auth AuthService
builder.Services.AddScoped<AuthService>();


// Database connection
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// products
builder.Services.AddScoped<ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
