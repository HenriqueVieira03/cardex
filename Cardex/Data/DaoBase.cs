namespace Cardex.Data;

// Implementação comum do CRUD, herdada por ColecaoDao, CartaDao e ExemplarDao.
// Os métodos são "virtual" para que um DAO possa mudar o comportamento
// (ex.: ColecaoDao.ExcluirAsync apaga em cascata).
public abstract class DaoBase<T> : IDao<T> where T : new()
{
    protected readonly CardexDatabase Database;

    protected DaoBase(CardexDatabase database)
    {
        Database = database;
    }

    public virtual async Task<List<T>> ObterTodosAsync()
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.Table<T>().ToListAsync();
    }

    public virtual async Task<T?> ObterPorIdAsync(int id)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.FindAsync<T>(id); // busca pela chave primária; null se não achar
    }

    // Depois do Insert, o sqlite-net preenche o Id (autoincremento) no próprio objeto.
    public virtual async Task<int> InserirAsync(T item)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.InsertAsync(item);
    }

    public virtual async Task<int> AtualizarAsync(T item)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.UpdateAsync(item);
    }

    public virtual async Task<int> ExcluirAsync(T item)
    {
        var conexao = await Database.ObterConexaoAsync();
        return await conexao.DeleteAsync(item);
    }
}
