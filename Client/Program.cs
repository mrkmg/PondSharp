using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace PondSharp.Client
{
    public sealed class Program
    {
        // ReSharper disable once ASYNC0001
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            ConfigureServices(builder);
            var host = builder.Build();
            ConfigureProviders(host.Services);
            await host.RunAsync().ConfigureAwait(false);
        }

        private static void ConfigureServices(WebAssemblyHostBuilder builder)
        {
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
            });
        }

        private static void ConfigureProviders(IServiceProvider services)
        {
            try
            {
                var jsRuntime = services.GetService<IJSRuntime>();
                var prop = typeof(JSRuntime).GetProperty("JsonSerializerOptions", BindingFlags.NonPublic | BindingFlags.Instance);
                var value = (JsonSerializerOptions)Convert.ChangeType(prop.GetValue(jsRuntime, null), typeof(JsonSerializerOptions));
                value.DictionaryKeyPolicy = null;
                value.PropertyNamingPolicy = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SOME ERROR: {ex}");
            }
        }
    }
}
