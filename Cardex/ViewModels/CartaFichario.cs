using Cardex.Models;

namespace Cardex.ViewModels;

// Um "bolso" do fichário: a carta + quantos exemplares eu tenho dela.
public class CartaFichario
{
    public CartaFichario(Carta carta, int quantidade)
    {
        Carta = carta;
        Quantidade = quantidade;
    }

    public Carta Carta { get; }

    public int Quantidade { get; }

    public bool Tenho => Quantidade > 0;

    // Selo de quantidade, só quando passar de 1.
    public bool MostrarSelo => Quantidade > 1;

    public string Selo => $"x{Quantidade}";

    public string Numero => Carta.EhPromo ? $"Promo {Carta.NumeroFormatado}" : Carta.NumeroFormatado;

    public string Nome => Carta.Nome;

    public string Raridade => Carta.Raridade.Nome();

    public string? ImagemUrl => Carta.ImagemPequenaUrl; // miniatura: versão leve
}

// Um grupo do fichário ("Coleção principal (001–084)" etc.).
// A CollectionView agrupada espera uma lista de listas: cada grupo É uma lista, com um título.
public class GrupoCartas : List<CartaFichario>
{
    public GrupoCartas(string titulo, IEnumerable<CartaFichario> cartas) : base(cartas)
    {
        Titulo = titulo;
    }

    public string Titulo { get; }
}

// Parâmetro do formulário de carta: em qual coleção e, se for edição, qual carta.
public record CartaFormParametro(int ColecaoId, int? CartaId = null);
