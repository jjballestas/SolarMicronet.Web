using SolarMicronet.Web.Components;
using SolarMicronet.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registrar servicios de blockchain
builder.Services.AddSingleton<IMicrogridReadService, MicrogridReadService>();
builder.Services.AddSingleton<IEventIndexerService, EventIndexerService>();

// Registrar cliente HTTP para SmartMeter API
builder.Services.AddHttpClient<ISmartMeterApiClient, SmartMeterApiClient>();

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Logger.LogInformation(@"
╔═══════════════════════════════════════════════════════════════╗
║          SOLARMICRONET - BLOCKCHAIN ENERGY MANAGER            ║
╚═══════════════════════════════════════════════════════════════╝

🌐 Application:     http://localhost:5000
🔗 Blockchain:      BLOCK-LAB (ChainId: 1337)
📜 Contracts:
   - EnergonToken:       {0}
   - MicrogridManager:   {1}
   - SmartMeter:         {2}

🔌 SmartMeter API:  {3}

Features:
  ✅ MetaMask Integration
  ✅ Energy Generation & Consumption
  ✅ Energon Token Transfers
  ✅ Community Activities
  ✅ Event History & Auditing
  ✅ Admin Panel

Ready to manage your microgrid! 🚀
", BlockchainConfig.ENERGON_TOKEN_ADDRESS, BlockchainConfig.MICROGRID_MANAGER_ADDRESS, 
   BlockchainConfig.SMART_METER_ADDRESS, BlockchainConfig.SMART_METER_API_BASE);

app.Run();
