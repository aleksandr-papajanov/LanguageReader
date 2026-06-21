using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LanguageReader.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.HostEnvironment.IsDevelopment()
    ? builder.Configuration["Api:DevelopmentBaseUrl"] ?? "http://localhost:5000"
    : builder.Configuration["Api:BaseUrl"] ?? "https://languagereader.onrender.com";
var apiBaseAddress = new Uri(apiBaseUrl, UriKind.Absolute);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });
builder.Services.AddScoped<UserSession>();
builder.Services.AddLanguageReaderApiClients();

await builder.Build().RunAsync();
