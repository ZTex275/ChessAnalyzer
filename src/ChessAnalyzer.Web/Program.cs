using ChessAnalyzer.Core.Analysis;
using ChessAnalyzer.Core.ChessCom;
using ChessAnalyzer.Core.Engine;
using ChessAnalyzer.Web;
using ChessAnalyzer.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ChessComClient>();
builder.Services.AddScoped<IStockfishEngine, WebAnalysisEngine>();
builder.Services.AddScoped<GameAnalyzer>();

await builder.Build().RunAsync();
