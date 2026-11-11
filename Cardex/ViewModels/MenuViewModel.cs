using Cardex.Services;
using Cardex.Views;
using CommunityToolkit.Mvvm.Input;

namespace Cardex.ViewModels;

// Menu lateral (Flyout): cada comando troca a página de detalhe.
public partial class MenuViewModel
{
    private readonly NavegacaoService _navegacao;

    public MenuViewModel(NavegacaoService navegacao)
    {
        _navegacao = navegacao;
    }

    [RelayCommand]
    private Task IrParaInicioAsync() => _navegacao.IrParaMenuAsync<InicioPage>();

    [RelayCommand]
    private Task IrParaColecoesAsync() => _navegacao.IrParaMenuAsync<ColecoesPage>();

    [RelayCommand]
    private Task IrParaMinhasCartasAsync() => _navegacao.IrParaMenuAsync<MinhasCartasPage>();

    [RelayCommand]
    private Task IrParaSobreAsync() => _navegacao.IrParaMenuAsync<SobrePage>();
}
