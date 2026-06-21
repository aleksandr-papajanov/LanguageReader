using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LanguageReader.Client;
using LanguageReader.Client.Features.Common.Services;
using LanguageReader.Client.Features.Users.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var clientUri = new Uri(builder.HostEnvironment.BaseAddress);
var apiBaseAddress = new Uri($"{clientUri.Scheme}://{clientUri.Host}:5000");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });
builder.Services.AddScoped<UserSession>();
builder.Services.AddLanguageReaderApiClients();

await builder.Build().RunAsync();
