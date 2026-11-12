using Cardex.ViewModels;

namespace Cardex.Views;

public partial class FicharioPage : ContentPage
{
    private readonly FicharioViewModel _viewModel;

    public FicharioPage(FicharioViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Quando a grade ganha largura, recalcula a altura das cartas (proporção 63 × 88).
        Grade.SizeChanged += (_, _) => _viewModel.AjustarTamanho(Grade.Width);
    }

    // Recarrega ao entrar na aba e ao voltar do detalhe ou do formulário.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.CarregarCommand.CanExecute(null))
            _viewModel.CarregarCommand.Execute(null);
    }
}
