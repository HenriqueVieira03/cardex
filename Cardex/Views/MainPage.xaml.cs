namespace Cardex.Views;

public partial class MainPage : FlyoutPage
{
    public MainPage(MenuPage menu, InicioPage inicio)
    {
        InitializeComponent();
        Flyout = menu;
        Detail = new NavigationPage(inicio);
    }
}
