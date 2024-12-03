using DellikCorp.Libs.ColorPicker;

namespace DellikCorp.Apps.RgbControlCenter;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
            .UseColorPicker()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("FontAwesome6FreeBrands.otf", "FontAwesomeBrands");
				fonts.AddFont("FontAwesome6FreeRegular.otf", "FontAwesomeRegular");
				fonts.AddFont("FontAwesome6FreeSolid.otf", "FontAwesomeSolid");
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<ListDetailDetailViewModel>();
		builder.Services.AddSingleton<ListDetailDetailPage>();
#if DEBUG
        builder.Services.AddSingleton<IArduinoConnector, FakeArduinoConnector>();

#else
		builder.Services.AddSingleton<IArduinoConnector, ArduinoConnector>();
#endif

        builder.Services.AddSingleton<ListDetailViewModel>();

		builder.Services.AddSingleton<ListDetailPage>();

		return builder.Build();
	}
}
