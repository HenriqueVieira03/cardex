using SQLite;

namespace Cardex.Models;

// Um exemplar = cópia(s) física(s) que eu tenho de uma carta,
// numa combinação de variante + condição + idioma.
[Table("Exemplares")]
public class Exemplar
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int CartaId { get; set; }

    public Variante Variante { get; set; }

    public Condicao Condicao { get; set; }

    public Idioma Idioma { get; set; }

    public int Quantidade { get; set; } = 1;

    public bool ParaTroca { get; set; }

    [MaxLength(200)]
    public string? Observacao { get; set; }

    public DateTime DataAquisicao { get; set; } = DateTime.Today;
}
