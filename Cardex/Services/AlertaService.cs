using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace Cardex.Services;

public class AlertaService : IAlertaService
{
    public Task MostrarToastAsync(string mensagem)
    {
        return Toast.Make(mensagem, ToastDuration.Short).Show();
    }

    public Task MostrarSnackbarAsync(string mensagem, string? textoAcao = null, Func<Task>? aoTocar = null)
    {
        Action? acao = aoTocar is null ? null : () => _ = aoTocar();
        return Snackbar.Make(mensagem, acao, textoAcao ?? "OK", TimeSpan.FromSeconds(4)).Show();
    }

    public Task<bool> ConfirmarAsync(string titulo, string mensagem, string sim = "Sim", string nao = "Cancelar")
    {
        return PaginaAtual.DisplayAlertAsync(titulo, mensagem, sim, nao);
    }

    public Task AvisarAsync(string titulo, string mensagem)
    {
        return PaginaAtual.DisplayAlertAsync(titulo, mensagem, "OK");
    }

    // A página que está na tela: o modal aberto, se houver; senão a página raiz da janela.
    private static Page PaginaAtual
    {
        get
        {
            var raiz = Application.Current!.Windows[0].Page!;
            return raiz.Navigation.ModalStack.LastOrDefault() ?? raiz;
        }
    }
}
