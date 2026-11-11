namespace Cardex;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Provisório: a navegação (FlyoutPage/NavigationPage) entra na etapa 2.
		return new Window(new ContentPage { Content = new Label { Text = "Cardex", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center } });
	}
}
