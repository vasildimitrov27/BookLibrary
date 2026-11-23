-- SQL скрипт за базата данни (приложението го изпълнява автоматично при първо стартиране)
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Authors (
    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
    Name    TEXT NOT NULL UNIQUE,
    Country TEXT NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS Books (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Title    TEXT NOT NULL,
    AuthorId INTEGER NOT NULL,
    Year     INTEGER NOT NULL,
    FOREIGN KEY (AuthorId) REFERENCES Authors(Id) ON DELETE RESTRICT
);
