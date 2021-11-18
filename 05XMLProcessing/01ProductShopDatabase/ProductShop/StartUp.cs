using AutoMapper;
using ProductShop.Data;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using ProductShop.Dtos.Export;
using System.Text;

namespace ProductShop
{
    public class StartUp
    {
        

        public static void Main(string[] args)
        {

            ProductShopContext db = new ProductShopContext();

            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            //MapperConfiguration mapperConfiguration = new MapperConfiguration(cfg =>
            //{
            //    cfg.AddProfile<ProductShopProfile>();
            //});

            //IMapper mapper = mapperConfiguration.CreateMapper();


            //  db.Database.EnsureDeleted();
            //  db.Database.EnsureCreated();

            //string xmlUsers = File.ReadAllText("../../../Datasets/users.xml");

            //Console.WriteLine(ImportUsers(db, xmlUsers));

            //string xmlProducts = File.ReadAllText("../../../Datasets/products.xml");

            //Console.WriteLine(ImportProducts(db, xmlProducts));

            //string xmlCategories = File.ReadAllText("../../../Datasets/categories.xml");

            //Console.WriteLine(ImportCategories(db, xmlCategories));

            //string xmlCategoriesProducts = File.ReadAllText("../../../Datasets/categories-products.xml");

            //Console.WriteLine(ImportCategoryProducts(db, xmlCategoriesProducts));

            if (!Directory.Exists("../../../Datasets/Result"))
            {
                Directory.CreateDirectory("../../../Datasets/Result");
            }

            // File.WriteAllText("../../../Datasets/Result/products-in-range.xml", GetProductsInRange(db));

            // File.WriteAllText("../../../Datasets/Result/users-sold-products.xml", GetSoldProducts(db));

            // File.WriteAllText("../../../Datasets/Result/categories-by-products.xml", GetCategoriesByProductsCount(db));

            File.WriteAllText("../../../Datasets/Result/users-and-products.xml", GetUsersWithProducts(db));

        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(UserDto[]), new XmlRootAttribute("Users"));

            var usersDTO = (UserDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            var users = Mapper.Map<User[]>(usersDTO);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProductDto[]), new XmlRootAttribute("Products"));

            var productsDto = (ProductDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            //var products = mapper.Map<Product[]>(productsDto);


            List<Product> products = new List<Product>();

            foreach (var productDto in productsDto)
            {
                Product product = new Product
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    SellerId = productDto.SellerId,
                    BuyerId = productDto.BuyerId
                };

                products.Add(product);
            }
            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count}";
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            XmlSerializer xml = new XmlSerializer(typeof(CategoryDto[]), new XmlRootAttribute("Categories"));

            var categoriesDto = (CategoryDto[])xml.Deserialize(new StringReader(inputXml));



            var categories = categoriesDto.Where(x => x.Name != null)
                 .Select(c => new Category
                 {
                     Name = c.Name
                 }).ToArray();

            context.Categories.AddRange(categories);

            context.SaveChanges();

           
            return $"Successfully imported {categories.Length}";


        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CategoryProductDto[]), new XmlRootAttribute("CategoryProducts"));

            var categoryProductDtos = (CategoryProductDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            List<CategoryProduct> dbCategoriesProducts = new List<CategoryProduct>();

            foreach (var dto in categoryProductDtos)
            {
                if (context.Products.Any(p => p.Id == dto.ProductId)
                    && context.Categories.Any(c => c.Id == dto.CategoryId))
                {
                    CategoryProduct categoryProduct = new CategoryProduct
                    {
                        CategoryId = dto.CategoryId,
                        ProductId = dto.ProductId
                    };

                    dbCategoriesProducts.Add(categoryProduct);
                }
                
            }

            context.CategoryProducts.AddRange(dbCategoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {dbCategoriesProducts.Count}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportProductsInRangeDto[]), new XmlRootAttribute("Products"));

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(x => new ExportProductsInRangeDto
                {
                    Name = x.Name,
                    Price = x.Price,
                    BuyerFullName =  $"{x.Buyer.FirstName} {x.Buyer.LastName}" 
                }) 
                .OrderBy(x => x.Price)
                .Take(10)
                .ToArray();
                
                

            StringBuilder sb = new StringBuilder();

            xmlSerializer.Serialize(new StringWriter(sb), products, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Any())
                .Select(u => new ExportUserSoldProductsDto
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    SoldProducts = u.ProductsSold
                    .Select(p => new ExportSoldProductsDto
                    {
                        Name = p.Name,
                        Price = p.Price
                    }).ToArray()
                })
                .OrderBy(l => l.LastName)
                .ThenBy(f => f.FirstName)
                .Take(5)
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportUserSoldProductsDto[]), new XmlRootAttribute("Users"));

            StringBuilder sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            xmlSerializer.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(c => new ExportCategoriesByProductsDto
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AveragePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price)
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue)
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCategoriesByProductsDto[]), new XmlRootAttribute("Categories"));

            StringBuilder sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);


            xmlSerializer.Serialize(new StringWriter(sb), categories, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            ExportUserAndProductsDto users = new ExportUserAndProductsDto
            {
                Count = context.Users.Count(u => u.ProductsSold.Any()),
                Users = context.Users
                        // .ToArray()
                        .Where(u => u.ProductsSold.Any())
                        
                        .Select(u => new ExportUserDto
                        {
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            Age = u.Age,
                            SoldProducts = new SoldProductsDto
                            {
                                Count = u.ProductsSold.Count,
                                Products = u.ProductsSold.Select(p => new ExportProductDto
                                {
                                    Name = p.Name,
                                    Price = p.Price
                                })
                                .OrderByDescending(p => p.Price)
                                .ToArray()
                                
                            }
                        })
                        .OrderByDescending(u => u.SoldProducts.Count)
                        .Take(10)
                        .ToArray()
            };

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportUserAndProductsDto), new XmlRootAttribute("Users"));

            StringBuilder sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);


            xmlSerializer.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}