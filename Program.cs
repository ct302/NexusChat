using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using OllamaBlazorWasm;
using OllamaBlazorWasm.Services;
using Microsoft.Extensions.Logging;
using System;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Register HTTP client
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Register services
builder.Services.AddScoped<OllamaService>();
builder.Services.AddScoped<OllamaModelService>();

// Replace with LocalStorage version
builder.Services.AddScoped<LocalStorageConversationService>();
// Keep the old service for backward compatibility
builder.Services.AddSingleton<ConversationService>();

await builder.Build().RunAsync();