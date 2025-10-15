using ManagementLabel.Components;
using ManagementLabel.Components.CartF;
using ManagementLabel.Components.OrderF;
using ManagementLabel.Components.ReceiptF;
using ManagementLabel.LogIn;
using ManagementLabel.Model;
using ManagementLabel.ProductsF;
using ManagementLabel.Components.DiscountF;
using ManagementLabel.Components.ProductGroupF;
using Microsoft.AspNetCore.Components.Authorization;
using ManagementLabel.Components.CategoriesF;
using ManagementLabel.Components.InvoiceF;
using ManagementLabel.Components.AddressesF;
using Blazored.LocalStorage;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// ProjectInfo 
builder.Services.Configure<ProjectInfo>(builder.Configuration.GetSection("ProjectInfo"));

// API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7250") });
// auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
// auth AuthService
builder.Services.AddScoped<AuthService>();
// products
builder.Services.AddScoped<ProductService>();
// order
builder.Services.AddScoped<OrderService>();
// cart
builder.Services.AddScoped<CartService>();
// addresses
builder.Services.AddScoped<AddressService>();
// Receipt
builder.Services.AddScoped<ReceiptService>();
// Group Products
builder.Services.AddScoped<ProductGroupService>();
// discount
builder.Services.AddScoped<DiscountService>();
// categories
builder.Services.AddScoped<CategoryService>();
// Invoice
builder.Services.AddScoped<InvoiceService>();
//
builder.Services.AddBlazoredLocalStorage();
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
