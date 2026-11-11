using Cardex.Models;

namespace Cardex.Data;

public class ExemplarDao : DaoBase<Exemplar>
{
    public ExemplarDao(CardexDatabase database) : base(database)
    {
    }

    public async Task<List<Exemplar>> ObterPorCartaAsync(int cartaId)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<Exemplar>()
            .Where(e => e.CartaId == cartaId)
            .OrderBy(e => e.Variante)
            .ToListAsync();
    }

    public async Task<List<Exemplar>> ObterParaTrocaAsync()
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<Exemplar>().Where(e => e.ParaTroca).ToListAsync();
    }

    // Todos os exemplares das cartas de uma coleção (usado no progresso e nas repetidas).
    public async Task<List<Exemplar>> ObterPorColecaoAsync(int colecaoId)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.QueryAsync<Exemplar>(
            "SELECT e.* FROM Exemplares e JOIN Cartas c ON c.Id = e.CartaId WHERE c.ColecaoId = ?",
            colecaoId);
    }

    // Procura um exemplar "igual" (mesma carta + variante + condição + idioma)
    // para oferecer somar a quantidade em vez de criar um registro duplicado.
    public async Task<Exemplar?> ObterIgualAsync(int cartaId, Variante variante, Condicao condicao, Idioma idioma, int ignorarId = 0)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<Exemplar>().FirstOrDefaultAsync(e =>
            e.CartaId == cartaId && e.Variante == variante && e.Condicao == condicao &&
            e.Idioma == idioma && e.Id != ignorarId);
    }
}
