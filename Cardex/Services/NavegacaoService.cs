using Cardex.Views;

namespace Cardex.Services;

// ViewModels que precisam de um dado ao abrir (ex.: o Id da coleção a editar)
// implementam esta interface. O NavegacaoService chama o método antes de mostrar a tela.
public interface IRecebeParametro
{
    Task ReceberParametroAsync(object? parametro);
}

// Centraliza a navegação para os ViewModels não conhecerem FlyoutPage/NavigationPage.
//
//   FlyoutPage (MainPage)
//   ├─ Flyout: menu
//   └─ Detail: NavigationPage  ← IrParaAsync empilha aqui (PushAsync)
//   Modais (PushModalAsync)    ← formulários de cadastro/edição
public class NavegacaoService
{
    private readonly IServiceProvider _servicos;

    public NavegacaoService(IServiceProvider servicos)
    {
        _servicos = servicos;
    }

    private static MainPage Principal => (MainPage)Application.Current!.Windows[0].Page!;

    private static INavigation Navegacao => Principal.Detail.Navigation;

    // Item do menu lateral: troca o Detail por uma NavigationPage nova com a página escolhida.
    public async Task IrParaMenuAsync<TPagina>() where TPagina : Page
    {
        var pagina = await CriarAsync<TPagina>(null);
        Principal.Detail = new NavigationPage(pagina);
        Principal.IsPresented = false; // fecha o menu
    }

    // Empilha uma página dentro do Detail (tem botão voltar).
    public async Task IrParaAsync<TPagina>(object? parametro = null) where TPagina : Page
    {
        var pagina = await CriarAsync<TPagina>(parametro);
        await Navegacao.PushAsync(pagina);
    }

    // Abre um formulário como modal (cobre a tela; fecha com FecharModalAsync).
    public async Task AbrirModalAsync<TPagina>(object? parametro = null) where TPagina : Page
    {
        var pagina = await CriarAsync<TPagina>(parametro);
        await Navegacao.PushModalAsync(pagina);
    }

    public Task FecharModalAsync()
    {
        return Navegacao.PopModalAsync();
    }

    // Pede a página ao container de DI (que já injeta o ViewModel) e entrega o parâmetro.
    private async Task<TPagina> CriarAsync<TPagina>(object? parametro) where TPagina : Page
    {
        var pagina = _servicos.GetRequiredService<TPagina>();
        if (pagina.BindingContext is IRecebeParametro vm)
            await vm.ReceberParametroAsync(parametro);
        return pagina;
    }
}
