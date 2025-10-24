using BlazorFrontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure HttpClient for API Gateway
var apiGatewayUrl = builder.Configuration["ApiGateway:BaseUrl"] ?? "http://localhost:5000";
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiGatewayUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IApiClient, ApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
