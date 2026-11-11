using Cardex.Models;

namespace Cardex.Data;

public class CartaDao : DaoBase<Carta>
{
    public CartaDao(CardexDatabase database) : base(database)
    {
    }

    // Ordem do fichário: cartas da coleção por número, depois as promos.
    public async Task<List<Carta>> ObterPorColecaoAsync(int colecaoId)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<Carta>()
            .Where(c => c.ColecaoId == colecaoId)
            .OrderBy(c => c.EhPromo)
            .ThenBy(c => c.Numero)
            .ToListAsync();
    }

    // "45" encontra a carta 045; texto procura no nome (LIKE ignora maiúsculas/minúsculas).
    public async Task<List<Carta>> BuscarPorNomeOuNumeroAsync(int colecaoId, string termo)
    {
        termo = termo.Trim();
        if (termo.Length == 0)
            return await ObterPorColecaoAsync(colecaoId);

        var conexao = await Database.ObterConexaoAsync();
        int numero = int.TryParse(termo, out var n) ? n : -1;
        return await conexao.QueryAsync<Carta>(
            "SELECT * FROM Cartas WHERE ColecaoId = ? AND (Numero = ? OR Nome LIKE ?) ORDER BY EhPromo, Numero",
            colecaoId, numero, $"%{termo}%");
    }

    // Número único dentro da coleção. Promos têm numeração própria (MEP 088 não
    // conflita com a 088 da coleção), por isso a checagem é feita por grupo.
    public async Task<bool> NumeroExisteAsync(int colecaoId, int numero, bool ehPromo, int ignorarId = 0)
    {
        var conexao = await Database.ObterConexaoAsync();
        int total = await conexao.Table<Carta>().CountAsync(c =>
            c.ColecaoId == colecaoId && c.Numero == numero && c.EhPromo == ehPromo && c.Id != ignorarId);
        return total > 0;
    }

    // Antes de salvar, calcula EhSecreta a partir do TotalPrincipal da coleção.
    public override async Task<int> InserirAsync(Carta item)
    {
        await CalcularSecretaAsync(item);
        return await base.InserirAsync(item);
    }

    public override async Task<int> AtualizarAsync(Carta item)
    {
        await CalcularSecretaAsync(item);
        return await base.AtualizarAsync(item);
    }

    // Usado pelo seed: insere todas as cartas do catálogo de uma vez (em uma transação).
    public async Task<int> InserirVariasAsync(IEnumerable<Carta> cartas)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.InsertAllAsync(cartas);
    }

    private async Task CalcularSecretaAsync(Carta carta)
    {
        var conexao = await Database.ObterConexaoAsync();
        var colecao = await conexao.FindAsync<Colecao>(carta.ColecaoId);
        carta.CalcularSecreta(colecao?.TotalPrincipal ?? int.MaxValue);
    }

    public async Task<int> ContarExemplaresAsync(int cartaId)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<Exemplar>().CountAsync(e => e.CartaId == cartaId);
    }

    // Só cartas personalizadas podem ser excluídas; as do catálogo, não.
    // Os exemplares da carta são apagados junto (cascata).
    public override async Task<int> ExcluirAsync(Carta item)
    {
        if (!item.Personalizada)
            throw new InvalidOperationException("Cartas importadas do catálogo não podem ser excluídas.");

        var conexao = await Database.ObterConexaoAsync();
        int apagados = 0;
        await conexao.RunInTransactionAsync(db =>
        {
            apagados += db.Execute("DELETE FROM Exemplares WHERE CartaId = ?", item.Id);
            apagados += db.Delete(item);
        });
        return apagados;
    }
}
