# Книги в библиотека (C# / .NET 10 / WPF / SQLite)

Отваряне: двоен клик на `BookLibrary.sln` във Visual Studio 2026 (поддържа .NET 10) -> F5.
NuGet пакетът Microsoft.Data.Sqlite се възстановява автоматично.

- База: `library.db` се създава автоматично до .exe файла (bin\Debug\net10.0-windows\). SQL скриптът е в `BookLibrary\schema.sql`.
- Таблици: Authors (Id, Name, Country), Books (Id, Title, AuthorId, Year).
- Екрани: Книги (търсене по заглавие, CRUD), Автори (CRUD), Онлайн каталог (Open Library API).
- API: https://openlibrary.org/search.json – безплатен, без ключ.
