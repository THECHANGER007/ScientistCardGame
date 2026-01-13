using ScientistCardGame.Views;

namespace ScientistCardGame
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainMenuPage());
        }
    }
}