namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using BookShopContext db = new BookShopContext();
            // DbInitializer.ResetDatabase(db);

            // string input = Console.ReadLine();
            // string result = GetBooksByAgeRestriction(db, input);

            // string result = GetGoldenBooks(db);

            // string result = GetBooksByPrice(db);

            // int year = int.Parse(Console.ReadLine());
            // string result = GetBooksNotReleasedIn(db, year);

            // string input = Console.ReadLine();
            // string result = GetBooksByCategory(db, input);

            // int lengthCheck = int.Parse(Console.ReadLine());



            string result = GetMostRecentBooks(db);

            // int result = CountBooks(db, lengthCheck);




            Console.WriteLine(result);
        }


        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            List<string> bookTitles = context
                .Books
                .AsEnumerable() // unattached from DB
                .Where(b => b.AgeRestriction.ToString().ToLower() == command.ToLower())
                .Select(b => b.Title)
                .OrderBy(x => x)
                .ToList();

            return String.Join(Environment.NewLine, bookTitles);
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            List<string> coldenBooks = context
                .Books
                .Where(x => x.Copies < 5000 && x.EditionType == EditionType.Gold)
                .OrderBy(b => b.BookId)
                .Select(x => x.Title)

                .ToList();



            return String.Join(Environment.NewLine, coldenBooks);
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var booksByPrice = context
                .Books
                .Where(x => x.Price > 40)
                .OrderByDescending(x => x.Price)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var item in booksByPrice)
            {
                sb.AppendLine($"{item.Title} - ${item.Price:f2}");
            }

            return sb.ToString().TrimEnd();

        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            List<string> booksNotReleasedIn = context
                .Books
                .Where(x => x.ReleaseDate.Value.Year != year)
                .OrderBy(x => x.BookId)
                .Select(b => b.Title)
                .ToList();

            return String.Join(Environment.NewLine, booksNotReleasedIn);
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            List<string> categories = input
                  .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                  .Select(c => c.ToLower())
                  .ToList();

            List<string> bookTitlesByCategory = new List<string>();

            foreach (var cat in categories)
            {
                List<string> currCatBookTitles = context
                    .Books
                    .Where(b => b.BookCategories.Any(bc => bc.Category.Name.ToLower() == cat))
                    .Select(b => b.Title)
                    .ToList();

                bookTitlesByCategory.AddRange(currCatBookTitles);
            }

            bookTitlesByCategory = bookTitlesByCategory
                .OrderBy(x => x)
                .ToList();

            return String.Join(Environment.NewLine, bookTitlesByCategory);



        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            DateTime parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var books = context
                .Books
                .Where(d => d.ReleaseDate < parsedDate)
                .OrderByDescending(x => x.ReleaseDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.EditionType} - ${book.Price:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {


            return string.Join(Environment.NewLine, context.Authors
                .Where(a => a.FirstName != null && a.FirstName.EndsWith(input))
                .Select(a => $"{a.FirstName} {a.LastName}")
                .OrderBy(n => n));
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            List<string> bookTitles = context
                .Books
                .Where(t => t.Title.ToLower().Contains(input.ToLower()))
                .Select(b => b.Title)
                .OrderBy(x => x)
                .ToList();

            return string.Join(Environment.NewLine, bookTitles);



        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var booksByAuthor = context
                .Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(b => b.BookId)
                .Select(b => new
                {
                    b.Title,
                    b.Author.FirstName,
                    b.Author.LastName
                })
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var b in booksByAuthor)
            {
                sb.AppendLine($"{b.Title} ({b.FirstName} {b.LastName})");
            }

            return sb.ToString().TrimEnd();

        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context
                .Books

                .Where(b => b.Title.Length > lengthCheck)
                .ToList();

            return books.Count();
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context
                 .Authors
                 .Select(a => new
                 {
                     FullName = $"{a.FirstName} {a.LastName}",
                     Copies = a.Books.Sum(b => b.Copies)
                 })
                 .OrderByDescending(a => a.Copies)
                 .ToArray();

            string result = string.Join(Environment.NewLine, authors.Select(a => $"{a.FullName} - {a.Copies}"));
            return result;
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            StringBuilder sb = new StringBuilder();

            var categoryProfit = context
                 .Categories
                 .Select(c => new
                 {
                     Name = c.Name,
                     Profit = c.CategoryBooks.Sum(b => b.Book.Price * b.Book.Copies)
                 })
                 .OrderByDescending(x => x.Profit)
                 .ThenBy(x => x.Name)
                 .ToList();

            foreach (var cp in categoryProfit)
            {
                sb.AppendLine($"{cp.Name} ${cp.Profit:f2}");
            }

            return sb.ToString().TrimEnd();

        }

        public static string GetMostRecentBooks(BookShopContext context)
        {

            StringBuilder sb = new StringBuilder();

            var categories = context
                 .Categories
                 .Select(c => new
                 {
                     CategoryName = c.Name,
                     Books = c.CategoryBooks
                         .Select(bc => new
                         {
                             Title = bc.Book.Title,
                             ReleaseDate = bc.Book.ReleaseDate
                         })
                         .OrderByDescending(bc => bc.ReleaseDate)
                         .Take(3)
                 })
                 .OrderBy(c => c.CategoryName)
                 .ToArray();

            
            foreach (var category in categories)
            {
                sb.AppendLine($"--{category.CategoryName}");
                foreach (var book in category.Books)
                {
                    sb.AppendLine($"{book.Title} ({book.ReleaseDate.Value.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            Book[] books = context
               .Books
               .Where(b => b.ReleaseDate.Value.Year < 2010)
               .ToArray();

            foreach (Book book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();

        }

        public static int RemoveBooks(BookShopContext context)
        {
            Book[] books = context
                .Books
                .Where(b => b.Copies < 4200)
                
                .ToArray();

            context.Books.RemoveRange(books);
            context.SaveChanges();

            int deletedBooks = books.Count();

            return deletedBooks;

            
        }
    }
}

