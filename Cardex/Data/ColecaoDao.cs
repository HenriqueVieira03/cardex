using Cardex.Models;

namespace Cardex.Data;

public class ColecaoDao : DaoBase<Colecao>
{
    public ColecaoDao(CardexDatabase database) : base(database)
    {
    }

    public override async Task<List<Colecao>> ObterTodosAsync()
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<Colecao>().OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task<Colecao?> ObterPorCodigoAsync(string codigo)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<Colecao>().FirstOrDefaultAsync(c => c.Codigo == codigo);
    }

    // Usado na validação do formulário. "ignorarId" = a própria coleção, no modo edição.
    public async Task<bool> CodigoExisteAsync(string codigo, int ignorarId = 0)
    {
        var conexao = await Database.ObterConexaoAsync();
        int total = await conexao.Table<Colecao>()
            .CountAsync(c => c.Codigo == codigo && c.Id != ignorarId);
        return total > 0;
    }

    public async Task<Colecao?> ObterFavoritaAsync()
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<Colecao>().FirstOrDefaultAsync(c => c.Favorita);
    }

    // Só uma coleção pode ser favorita (é ela que aparece no Início).
    public override async Task<int> InserirAsync(Colecao item)
    {
        if (item.Favorita)
            await DesmarcarFavoritasAsync();
        return await base.InserirAsync(item);
    }

    public override async Task<int> AtualizarAsync(Colecao item)
    {
        if (item.Favorita)
            await DesmarcarFavoritasAsync();
        return await base.AtualizarAsync(item);
    }

    private async Task DesmarcarFavoritasAsync()
    {
        var conexao = await Database.ObterConexaoAsync();
        await conexao.ExecuteAsync("UPDATE Colecoes SET Favorita = 0 WHERE Favorita = 1");
    }

    // Quantos registros a exclusão em cascata vai apagar (mostrado na confirmação).
    public async Task<(int Cartas, int Exemplares)> ContarDependentesAsync(int colecaoId)
    {
        var conexao = await Database.ObterConexaoAsync();
        int cartas = await conexao.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Cartas WHERE ColecaoId = ?", colecaoId);
        int exemplares = await conexao.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Exemplares WHERE CartaId IN (SELECT Id FROM Cartas WHERE ColecaoId = ?)", colecaoId);
        return (cartas, exemplares);
    }

    // Exclusão em cascata: exemplares → cartas → coleção.
    // Tudo numa transação: ou apaga tudo, ou não apaga nada.
    public override async Task<int> ExcluirAsync(Colecao item)
    {
        var conexao = await Database.ObterConexaoAsync();
        int apagados = 0;
        await conexao.RunInTransactionAsync(db =>
        {
            apagados += db.Execute(
                "DELETE FROM Exemplares WHERE CartaId IN (SELECT Id FROM Cartas WHERE ColecaoId = ?)", item.Id);
            apagados += db.Execute("DELETE FROM Cartas WHERE ColecaoId = ?", item.Id);
            apagados += db.Delete(item);
        });
        return apagados;
    }
}
