using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LanguageReader.Client;
using LanguageReader.Client.Features.Common.Services;
using LanguageReader.Client.Features.Users.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
var apiBaseAddress = string.IsNullOrWhiteSpace(apiBaseUrl)
    ? new Uri("http://localhost:5000")
    : new Uri(apiBaseUrl, UriKind.Absolute);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });
builder.Services.AddScoped<UserSession>();
builder.Services.AddLanguageReaderApiClients();

await builder.Build().RunAsync();
