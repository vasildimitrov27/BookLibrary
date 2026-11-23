using System.Windows;

namespace BookLibrary;

public partial class AuthorDialog : Window
{
    private readonly int _id;
    public Author Result { get; private set; } = new();

    public AuthorDialog(Author? author)
    {
        InitializeComponent();
        _id = author?.Id ?? 0;
        Title = _id == 0 ? "Нов автор" : "Редакция на автор";
        if (author != null)
        {
            NameBox.Text = author.Name;
            CountryBox.Text = author.Country;
        }
        NameBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string name = NameBox.Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show(this, "Моля, въведете име на автора.", "Невалидни данни",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            NameBox.Focus();
            return;
        }
        Result = new Author { Id = _id, Name = name, Country = CountryBox.Text.Trim() };
        DialogResult = true;
    }
}
