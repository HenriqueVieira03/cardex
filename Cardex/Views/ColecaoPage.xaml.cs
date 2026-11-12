using Cardex.Data;
using Cardex.Services;

namespace Cardex.Views;

// Recebe o Id da coleção (vindo do NavegacaoService) e repassa para cada aba.
public partial class ColecaoPage : TabbedPage, IRecebeParametro
{
    private readonly ColecaoDao _colecaoDao;

    public ColecaoPage(ColecaoDao colecaoDao, FicharioPage fichario, RepetidasPage repetidas, EstatisticasPage estatisticas)
    {
        InitializeComponent();
        _colecaoDao = colecaoDao;
        Children.Add(fichario);
        Children.Add(repetidas);
        Children.Add(estatisticas);
    }

    public async Task ReceberParametroAsync(object? parametro)
    {
        if (parametro is not int colecaoId)
            return;

        var colecao = await _colecaoDao.ObterPorIdAsync(colecaoId);
        Title = colecao?.Nome ?? "Coleção";

        foreach (var aba in Children)
        {
            if (aba.BindingContext is IRecebeParametro vm)
                await vm.ReceberParametroAsync(colecaoId);
        }
    }
}
