using Application = Microsoft.Maui.Controls.Application;

namespace DellikCorp.Apps.RgbControlCenter;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
