using Cardex.Views;

namespace Cardex;

public partial class App : Application
{
	private readonly StatusPage _paginaInicial;

	// A página chega pela injeção de dependência (com o ViewModel e os DAOs já resolvidos).
	public App(StatusPage paginaInicial)
	{
		InitializeComponent();
		_paginaInicial = paginaInicial;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Provisório: a navegação (FlyoutPage/NavigationPage) entra na etapa 2.
		return new Window(_paginaInicial);
	}
}
