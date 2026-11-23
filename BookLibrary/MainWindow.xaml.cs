using Microsoft.Data.Sqlite;
using System.Windows;
using System.Windows.Controls;

namespace BookLibrary;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => { LoadAuthors(); LoadBooks(); };
    }

    private static void ShowError(string message, Exception ex) =>
        MessageBox.Show($"{message}\n\nПодробности: {ex.Message}", "Грешка",
            MessageBoxButton.OK, MessageBoxImage.Error);

    private static bool Confirm(string text) =>
        MessageBox.Show(text, "Потвърждение", MessageBoxButton.YesNo,
            MessageBoxImage.Question) == MessageBoxResult.Yes;

    // ---------- Зареждане ----------
    private void LoadBooks()
    {
        try { BooksGrid.ItemsSource = Db.GetBooks(SearchBox.Text.Trim()); }
        catch (Exception ex) { ShowError("Книгите не могат да бъдат заредени.", ex); }
    }

    private void LoadAuthors()
    {
        try { AuthorsGrid.ItemsSource = Db.GetAuthors(); }
        catch (Exception ex) { ShowError("Авторите не могат да бъдат заредени.", ex); }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded) LoadBooks();
    }

    // ---------- Книги ----------
    private void AddBook_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var authors = Db.GetAuthors();
            if (authors.Count == 0)
            {
                MessageBox.Show("Първо добавете поне един автор.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Tabs.SelectedIndex = 1;
                return;
            }
            var dlg = new BookDialog(authors, null) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                Db.AddBook(dlg.Result);
                LoadBooks();
            }
        }
        catch (Exception ex) { ShowError("Книгата не може да бъде добавена.", ex); }
    }

    private void EditBook_Click(object sender, RoutedEventArgs e)
    {
        if (BooksGrid.SelectedItem is not Book selected)
        {
            MessageBox.Show("Изберете книга от списъка.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        try
        {
            var dlg = new BookDialog(Db.GetAuthors(), selected) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                Db.UpdateBook(dlg.Result);
                LoadBooks();
            }
        }
        catch (Exception ex) { ShowError("Книгата не може да бъде редактирана.", ex); }
    }

    private void DeleteBook_Click(object sender, RoutedEventArgs e)
    {
        if (BooksGrid.SelectedItem is not Book selected)
        {
            MessageBox.Show("Изберете книга от списъка.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (!Confirm($"Да се изтрие ли книгата „{selected.Title}“?")) return;
        try
        {
            Db.DeleteBook(selected.Id);
            LoadBooks();
        }
        catch (Exception ex) { ShowError("Книгата не може да бъде изтрита.", ex); }
    }

    private void Catalog_Click(object sender, RoutedEventArgs e)
    {
        var search = new CatalogWindow { Owner = this };
        if (search.ShowDialog() != true || search.Selected is not CatalogItem item) return;

        try
        {
            // Ако авторът го няма в базата, се създава автоматично
            int authorId = string.IsNullOrWhiteSpace(item.Author)
                ? 0 : Db.GetOrCreateAuthor(item.Author);
            LoadAuthors();

            var authors = Db.GetAuthors();
            if (authors.Count == 0)
            {
                MessageBox.Show("Първо добавете поне един автор.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var prefilled = new Book { Title = item.Title, AuthorId = authorId, Year = item.Year };
            var dlg = new BookDialog(authors, prefilled, isNew: true) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                Db.AddBook(dlg.Result);
                LoadBooks();
            }
        }
        catch (Exception ex) { ShowError("Книгата от каталога не може да бъде добавена.", ex); }
    }

    // ---------- Автори ----------
    private void AddAuthor_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new AuthorDialog(null) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        try
        {
            Db.AddAuthor(dlg.Result);
            LoadAuthors();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show("Автор с това име вече съществува.", "Грешка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex) { ShowError("Авторът не може да бъде добавен.", ex); }
    }

    private void EditAuthor_Click(object sender, RoutedEventArgs e)
    {
        if (AuthorsGrid.SelectedItem is not Author selected)
        {
            MessageBox.Show("Изберете автор от списъка.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var dlg = new AuthorDialog(selected) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        try
        {
            Db.UpdateAuthor(dlg.Result);
            LoadAuthors();
            LoadBooks();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show("Автор с това име вече съществува.", "Грешка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex) { ShowError("Авторът не може да бъде редактиран.", ex); }
    }

    private void DeleteAuthor_Click(object sender, RoutedEventArgs e)
    {
        if (AuthorsGrid.SelectedItem is not Author selected)
        {
            MessageBox.Show("Изберете автор от списъка.", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (!Confirm($"Да се изтрие ли авторът „{selected.Name}“?")) return;
        try
        {
            Db.DeleteAuthor(selected.Id);
            LoadAuthors();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show("Авторът не може да бъде изтрит, защото има книги в библиотеката.\n" +
                            "Първо изтрийте или променете неговите книги.", "Грешка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex) { ShowError("Авторът не може да бъде изтрит.", ex); }
    }
}
