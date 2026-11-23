using System.Windows;

namespace BookLibrary;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            Db.Init();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Базата данни не може да бъде създадена.\n\n{ex.Message}",
                "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }
        base.OnStartup(e);
    }
}
