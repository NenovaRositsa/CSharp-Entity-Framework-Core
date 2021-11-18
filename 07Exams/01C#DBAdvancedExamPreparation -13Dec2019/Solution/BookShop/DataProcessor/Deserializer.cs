namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
    using BookShop.DataProcessor.ImportDto;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportBookDTO[]), new XmlRootAttribute("Books"));

            var bookDTOs = (ImportBookDTO[])xmlSerializer.Deserialize(new StringReader(xmlString));

            List<Book> validBooks = new List<Book>();

            foreach (var bookDTO in bookDTOs)
            {
                if (!IsValid(bookDTO))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime publishedOn;
                bool isValidDate = DateTime.TryParseExact(bookDTO.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out publishedOn);

                if (!isValidDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Book validBook = new Book()
                {
                    Name = bookDTO.Name,
                    Genre = (Genre)bookDTO.Genre,
                    Price = bookDTO.Price,
                    Pages = bookDTO.Pages,
                    PublishedOn = publishedOn
                };

                validBooks.Add(validBook);
                sb.AppendLine(String.Format(SuccessfullyImportedBook, validBook.Name, validBook.Price));
            }

            context.Books.AddRange(validBooks);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            var importAouthorsDTOS = JsonConvert.DeserializeObject<ImportAuthorsDTO[]>(jsonString);

            List<Author> validAuthors = new List<Author>();

            foreach (var dto in importAouthorsDTOS)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (validAuthors.Any(a => a.Email == dto.Email))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Author currentAuthor = new Author()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Phone = dto.Phone

                };

                foreach (var bookDto in dto.Books)
                {
                    if (!bookDto.Id.HasValue)
                    {
                        continue;
                    }

                    Book currentBook = context.Books
                        .FirstOrDefault(b => b.Id == bookDto.Id);

                    if (currentBook == null)
                    {
                        continue;
                    }

                    currentAuthor.AuthorsBooks.Add(new AuthorBook
                    {
                        Author = currentAuthor,
                        Book = currentBook
                    });

                   
                }

                if (currentAuthor.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                validAuthors.Add(currentAuthor);
                sb.AppendLine(String.Format(SuccessfullyImportedAuthor, (currentAuthor.FirstName + ' ' + currentAuthor.LastName),
                    currentAuthor.AuthorsBooks.Count));


            }

            context.Authors.AddRange(validAuthors);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}