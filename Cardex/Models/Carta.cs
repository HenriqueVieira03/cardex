using SQLite;

namespace Cardex.Models;

[Table("Cartas")]
public class Carta
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // "FK lógica": o SQLite do sqlite-net não cria FOREIGN KEY; a integridade é feita no DAO.
    [Indexed]
    public int ColecaoId { get; set; }

    public int Numero { get; set; }

    [NotNull]
    public string Nome { get; set; } = "";

    public Raridade Raridade { get; set; }

    public TipoCarta Tipo { get; set; }

    public string? ImagemUrl { get; set; }

    // Variantes que existem para esta carta (vêm da TCGdex: variants.normal/holo/reverse).
    public bool TemNormal { get; set; }

    public bool TemFoil { get; set; }

    public bool TemReverse { get; set; }

    // Calculado ao salvar: Numero > TotalPrincipal da coleção (e não é promo).
    public bool EhSecreta { get; set; }

    public bool EhPromo { get; set; }

    // true = cadastrada pelo usuário; false = veio do catálogo (não pode ser excluída).
    public bool Personalizada { get; set; }

    // Ex.: "045". Não é coluna do banco, só ajuda na exibição e na busca.
    [Ignore]
    public string NumeroFormatado => Numero.ToString("000");

    // Versão leve da imagem para miniaturas (fichário e listas).
    // A TCGdex tem duas qualidades: ".../high.webp" (~110 KB) e ".../low.webp" (~28 KB).
    // Para URLs de outros sites, devolve a própria URL.
    [Ignore]
    public string? ImagemPequenaUrl => ImagemUrl?.Replace("/high.webp", "/low.webp");

    // Regra única usada pelo seed e pelo DAO ao salvar.
    // Promos têm numeração própria (ex.: MEP 088), então nunca são "secretas".
    public void CalcularSecreta(int totalPrincipal)
    {
        EhSecreta = !EhPromo && Numero > totalPrincipal;
    }

    // Lista as variantes que esta carta tem, na ordem Normal, Foil, Reverse.
    public IEnumerable<Variante> VariantesExistentes()
    {
        if (TemNormal) yield return Variante.Normal;
        if (TemFoil) yield return Variante.Foil;
        if (TemReverse) yield return Variante.Reverse;
    }
}
