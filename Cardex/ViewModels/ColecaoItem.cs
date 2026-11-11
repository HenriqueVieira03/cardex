using Cardex.Models;

namespace Cardex.ViewModels;

// Uma linha da lista de coleções: a coleção + o texto de progresso já calculado.
public class ColecaoItem
{
    public ColecaoItem(Colecao colecao, string progresso)
    {
        Colecao = colecao;
        Progresso = progresso;
    }

    public Colecao Colecao { get; }

    public string Progresso { get; } // ex.: "57/120"

    public string Nome => Colecao.Nome;

    public string Codigo => Colecao.Codigo;

    public bool Favorita => Colecao.Favorita;
}
