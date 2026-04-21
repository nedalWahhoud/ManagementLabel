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
using ManagementLabel.Components.DistributionLinesF;
using ManagementLabel.Components.CustomersF;
using ManagementLabel.Components.Share;
using ManagementLabel.Components.DebtF;
using ManagementLabel.Components.TransactionsCustomersF;
using ManagementLabel.Components.SupplierF;

var builder = WebApplication.CreateBuilder(args);
// Netzwerk anhören
/*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5105); 
});*/
/*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7122, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});*/


builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5105);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(10); // Wartet 10 Minuten, bevor die Verbindung getrennt wird
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);    // Alle 15 Sekunden ein Ping durchführen, um die Verbindung aufrechtzuerhalten
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    })
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
        // Speichert den Benutzerstatus für 10 Minuten nach Verbindungsverlust
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30);
    });
// ProjectInfo 
builder.Services.Configure<ProjectInfo>(builder.Configuration.GetSection("ProjectInfo"));
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

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
// Suppliers
builder.Services.AddScoped<SuppliersService>();
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
//  DistributionLines Service 
builder.Services.AddScoped<DistributionLinesService>();
//  Customers Service 
builder.Services.AddScoped<CustomersService>();
//  WhatsApp Service 
builder.Services.AddScoped<WhatsAppService>();
//  DebtCustomers Service 
builder.Services.AddScoped<DebtService>();
builder.Services.AddScoped<WhatsAppService>();
//  TransactionsCustomersService Service 
builder.Services.AddScoped<TransactionsCustomersService>();
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

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
