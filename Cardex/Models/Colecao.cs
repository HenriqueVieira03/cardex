using SQLite;

namespace Cardex.Models;

[Table("Colecoes")]
public class Colecao
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Nome { get; set; } = "";

    public string NomeIngles { get; set; } = "";

    // Único: a validação no ViewModel avisa antes; o índice garante no banco.
    [NotNull, Unique]
    public string Codigo { get; set; } = "";

    public int TotalPrincipal { get; set; }

    public int TotalCartas { get; set; }

    public DateTime DataLancamento { get; set; } = DateTime.Today;

    public bool Favorita { get; set; }
}
