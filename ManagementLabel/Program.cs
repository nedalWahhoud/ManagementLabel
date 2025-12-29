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
using Microsoft.Extensions.Options;
using ManagementLabel.Components.ImagesF;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(5); // Wartet 5 Minuten, bevor die Verbindung getrennt wird
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);    // Alle 15 Sekunden ein Ping durchführen, um die Verbindung aufrechtzuerhalten
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);   
    })
    .AddCircuitOptions(options =>
    {
        // Speichert den Benutzerstatus für 10 Minuten nach Verbindungsverlust
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30);
    });

// ProjectInfo 
builder.Services.Configure<ProjectInfo>(builder.Configuration.GetSection("ProjectInfo"));

// app config
builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));

// http client with base address from app config
builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IOptions<AppConfig>>().Value;
    return new HttpClient { BaseAddress = config.ApiUri };
});

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
// ProductImages
builder.Services.AddScoped<ProductImagesService>();
//  Carousel Image 
builder.Services.AddScoped<CarouselImageService>();
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
