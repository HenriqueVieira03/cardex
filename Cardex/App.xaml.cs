using Cardex.Views;

namespace Cardex;

public partial class App : Application
{
	private readonly IServiceProvider _servicos;

	public App(IServiceProvider servicos)
	{
		InitializeComponent(); // carrega os recursos do App.xaml (cores, estilos, converters)
		_servicos = servicos;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// A MainPage é pedida aqui, e não no construtor: as páginas usam {StaticResource ...}
		// e esses recursos só existem depois do InitializeComponent() acima.
		return new Window(_servicos.GetRequiredService<MainPage>());
	}
}
