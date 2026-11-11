using Cardex.ViewModels;

namespace Cardex.Views;

public partial class StatusPage : ContentPage
{
    private readonly StatusViewModel _viewModel;

    // O ViewModel chega pela injeção de dependência (registrado no MauiProgram).
    public StatusPage(StatusViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CarregarCommand.Execute(null);
    }
}
