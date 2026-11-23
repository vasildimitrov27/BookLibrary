using System.Windows;

namespace BookLibrary;

public partial class BookDialog : Window
{
    private readonly int _id;
    public Book Result { get; private set; } = new();

    /// <param name="book">Книга за редакция или за предварително попълване.</param>
    /// <param name="isNew">true = записът още не е в базата (Id = 0).</param>
    public BookDialog(List<Author> authors, Book? book, bool isNew = false)
    {
        InitializeComponent();
        AuthorCombo.ItemsSource = authors;
        _id = isNew ? 0 : book?.Id ?? 0;
        Title = _id == 0 ? "Нова книга" : "Редакция на книга";

        if (book != null)
        {
            TitleBox.Text = book.Title;
            AuthorCombo.SelectedValue = book.AuthorId;
            YearBox.Text = book.Year > 0 ? book.Year.ToString() : "";
        }
        TitleBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        string title = TitleBox.Text.Trim();
        if (title.Length == 0)
        {
            Warn("Моля, въведете заглавие на книгата.");
            TitleBox.Focus();
            return;
        }
        if (AuthorCombo.SelectedValue is not int authorId)
        {
            Warn("Моля, изберете автор.");
            return;
        }
        int maxYear = DateTime.Now.Year;
        if (!int.TryParse(YearBox.Text.Trim(), out int year) || year < 1000 || year > maxYear)
        {
            Warn($"Годината трябва да е число между 1000 и {maxYear}.");
            YearBox.Focus();
            return;
        }

        Result = new Book { Id = _id, Title = title, AuthorId = authorId, Year = year };
        DialogResult = true;
    }

    private void Warn(string text) =>
        MessageBox.Show(this, text, "Невалидни данни", MessageBoxButton.OK, MessageBoxImage.Warning);
}
