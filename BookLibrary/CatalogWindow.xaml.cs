using System.Windows;

namespace BookLibrary;

public partial class CatalogWindow : Window
{
    public CatalogItem? Selected { get; private set; }

    public CatalogWindow()
    {
        InitializeComponent();
        QueryBox.Focus();
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        string query = QueryBox.Text.Trim();
        if (query.Length == 0)
        {
            MessageBox.Show(this, "Моля, въведете текст за търсене.", "Невалидни данни",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SearchButton.IsEnabled = false;
        StatusText.Text = "Търсене...";
        try
        {
            var items = await OpenLibraryClient.SearchAsync(query);
            ResultsGrid.ItemsSource = items;
            StatusText.Text = items.Count == 0 ? "Няма намерени резултати." : $"Намерени: {items.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = "";
            MessageBox.Show(this,
                "Каталогът не може да бъде достъпен. Проверете интернет връзката си и опитайте отново.\n\n" +
                $"Подробности: {ex.Message}", "Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SearchButton.IsEnabled = true;
        }
    }

    private void Use_Click(object sender, RoutedEventArgs e)
    {
        if (ResultsGrid.SelectedItem is not CatalogItem item)
        {
            MessageBox.Show(this, "Изберете книга от резултатите.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        Selected = item;
        DialogResult = true;
    }
}
