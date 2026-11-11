using Cardex.Data;
using Cardex.Services;
using Cardex.ViewModels;
using Cardex.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Cardex;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit() // Toast e Snackbar
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// ---- Injeção de dependência ----
		// Singleton: uma instância para o app inteiro (banco, DAOs e services).
		string caminhoBanco = Path.Combine(FileSystem.AppDataDirectory, "cardex.db3");
		builder.Services.AddSingleton(new CardexDatabase(caminhoBanco));
		builder.Services.AddSingleton<ColecaoDao>();
		builder.Services.AddSingleton<CartaDao>();
		builder.Services.AddSingleton<ExemplarDao>();
		builder.Services.AddSingleton<SeedService>();

		// Transient: uma instância nova cada vez que a tela é aberta.
		builder.Services.AddTransient<StatusViewModel>();
		builder.Services.AddTransient<StatusPage>();

		return builder.Build();
	}
}
