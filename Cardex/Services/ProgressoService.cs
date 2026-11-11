using Cardex.Models;

namespace Cardex.Services;

// Resultado de um nível de progresso. Ex.: Tenho 57, Total 120 → "57/120", 0.475.
public record Progresso(int Tenho, int Total)
{
    public double Fracao => Total == 0 ? 0 : (double)Tenho / Total;

    public string Texto => $"{Tenho}/{Total}";

    public bool Completo => Total > 0 && Tenho >= Total;
}

// Regras de progresso da coleção (seção 8 da especificação).
// Não depende de banco nem de tela: recebe listas e devolve números, por isso é fácil de testar.
public class ProgressoService
{
    // Set base: tenho cada carta de 1 a TotalPrincipal, em qualquer variante.
    public Progresso SetBase(Colecao colecao, IEnumerable<Carta> cartas, IEnumerable<Exemplar> exemplares)
    {
        return PorFaixaDeNumeros(colecao.TotalPrincipal, cartas, exemplares);
    }

    // Set completo: tenho cada carta de 1 a TotalCartas, em qualquer variante.
    public Progresso SetCompleto(Colecao colecao, IEnumerable<Carta> cartas, IEnumerable<Exemplar> exemplares)
    {
        return PorFaixaDeNumeros(colecao.TotalCartas, cartas, exemplares);
    }

    // Master set: cada variante existente de cada carta (1..TotalCartas), mais todas as promos.
    public Progresso MasterSet(Colecao colecao, IEnumerable<Carta> cartas, IEnumerable<Exemplar> exemplares)
    {
        var doMaster = cartas
            .Where(c => c.EhPromo || (c.Numero >= 1 && c.Numero <= colecao.TotalCartas))
            .ToList();

        // Pares (carta, variante) que eu tenho.
        var tenho = exemplares
            .Where(e => e.Quantidade > 0)
            .Select(e => (e.CartaId, e.Variante))
            .ToHashSet();

        int total = 0, possuidas = 0;
        foreach (var carta in doMaster)
        {
            foreach (var variante in carta.VariantesExistentes())
            {
                total++;
                if (tenho.Contains((carta.Id, variante)))
                    possuidas++;
            }
        }
        return new Progresso(possuidas, total);
    }

    // Cartas diferentes que tenho (pelo menos um exemplar, qualquer variante).
    public int CartasUnicas(IEnumerable<Exemplar> exemplares)
    {
        return exemplares.Where(e => e.Quantidade > 0).Select(e => e.CartaId).Distinct().Count();
    }

    public int TotalExemplares(IEnumerable<Exemplar> exemplares)
    {
        return exemplares.Sum(e => e.Quantidade);
    }

    // Repetidas = cópias além da primeira, por carta + variante, somando condições e idiomas.
    // Ex.: 3 cópias da 045 normal (2 NM + 1 SP) = 2 repetidas.
    public int Repetidas(IEnumerable<Exemplar> exemplares)
    {
        return exemplares
            .GroupBy(e => (e.CartaId, e.Variante))
            .Sum(g => Math.Max(0, g.Sum(e => e.Quantidade) - 1));
    }

    // Conta quantos números da faixa 1..limite eu tenho (promos ficam de fora).
    private static Progresso PorFaixaDeNumeros(int limite, IEnumerable<Carta> cartas, IEnumerable<Exemplar> exemplares)
    {
        var cartasComExemplar = exemplares.Where(e => e.Quantidade > 0).Select(e => e.CartaId).ToHashSet();

        int tenho = cartas
            .Where(c => !c.EhPromo && c.Numero >= 1 && c.Numero <= limite && cartasComExemplar.Contains(c.Id))
            .Select(c => c.Numero)
            .Distinct()
            .Count();

        return new Progresso(tenho, limite);
    }
}
