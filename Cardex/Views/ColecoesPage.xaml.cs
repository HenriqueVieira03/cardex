using Cardex.ViewModels;

namespace Cardex.Views;

public partial class ColecoesPage : ContentPage
{
    private readonly ColecoesViewModel _viewModel;

    public ColecoesPage(ColecoesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // Recarrega ao aparecer: também roda quando o modal do formulário fecha.
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.CarregarCommand.CanExecute(null))
            _viewModel.CarregarCommand.Execute(null);
    }
}
