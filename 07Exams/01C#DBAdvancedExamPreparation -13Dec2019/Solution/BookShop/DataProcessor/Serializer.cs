namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context.Authors.
                Select(a => new
                {
                    AuthorName = a.FirstName + ' ' + a.LastName,
                    Books = a.AuthorsBooks
                    .OrderByDescending(ab => ab.Book.Price)
                    .Select(ab => new
                    {
                        BookName = ab.Book.Name,
                        BookPrice = ab.Book.Price.ToString("f2")
                    })
                    .ToArray()
                    
                })
                .ToArray()
                .OrderByDescending(a => a.Books.Length)
                .ThenBy(an => an.AuthorName)
                .ToArray();

            var json = JsonConvert.SerializeObject(authors, Formatting.Indented);

            return json;
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var books = context.Books
                .Where(b => b.PublishedOn < date && b.Genre == Genre.Science)
                .OrderByDescending(p => p.Pages)
                .ThenByDescending(d => d.PublishedOn)
                .Take(10)
                .Select(b => new ExportBookDTO()
                {
                    Name = b.Name,
                    Pages = b.Pages,
                    Date = b.PublishedOn.ToString("d", CultureInfo.InvariantCulture),
                })
                .ToArray();
               

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportBookDTO[]), new XmlRootAttribute("Books"));

          var namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName() });

            var sb = new StringBuilder();

            xmlSerializer.Serialize(new StringWriter(sb), books, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}