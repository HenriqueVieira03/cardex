using Cardex.Data;
using Cardex.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cardex.ViewModels;

// Tela provisória da etapa 1: roda o seed e mostra o que há no banco.
// Será substituída pelo Início na etapa 2.
public partial class StatusViewModel : ObservableObject
{
    private readonly SeedService _seed;
    private readonly ColecaoDao _colecaoDao;
    private readonly CartaDao _cartaDao;

    public StatusViewModel(SeedService seed, ColecaoDao colecaoDao, CartaDao cartaDao)
    {
        _seed = seed;
        _colecaoDao = colecaoDao;
        _cartaDao = cartaDao;
    }

    // [ObservableProperty] gera o código que avisa a tela (INotifyPropertyChanged) quando o valor muda.
    [ObservableProperty]
    public partial bool Carregando { get; set; }

    [ObservableProperty]
    public partial string Mensagem { get; set; } = "";

    [ObservableProperty]
    public partial string Resumo { get; set; } = "";

    // [RelayCommand] gera a propriedade CarregarCommand, usada no binding da View.
    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        try
        {
            bool importou = await _seed.ImportarSeNecessarioAsync();
            Mensagem = importou ? "Catálogo importado agora." : "Catálogo já estava no banco.";

            var colecoes = await _colecaoDao.ObterTodosAsync();
            var linhas = new List<string>();
            foreach (var colecao in colecoes)
            {
                var cartas = await _cartaDao.ObterPorColecaoAsync(colecao.Id);
                linhas.Add($"{colecao.Codigo} · {colecao.Nome}{(colecao.Favorita ? " ★" : "")}\n" +
                           $"{cartas.Count(c => !c.EhPromo && !c.EhSecreta)} principais · " +
                           $"{cartas.Count(c => c.EhSecreta)} secretas · " +
                           $"{cartas.Count(c => c.EhPromo)} promos");
            }
            Resumo = linhas.Count == 0 ? "Nenhuma coleção." : string.Join("\n\n", linhas);
        }
        catch (Exception ex)
        {
            Mensagem = "Erro: " + ex.Message;
        }
        finally
        {
            Carregando = false;
        }
    }
}
