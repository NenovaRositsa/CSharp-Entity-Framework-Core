using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProductShop.Data;

using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            ProductShopContext db = new ProductShopContext();

            // db.Database.EnsureDeleted();
            // db.Database.EnsureCreated();

            // string inputJson = File.ReadAllText("../../../Datasets/users.json");

            // string result = ImportUsers(db, inputJson);

            // string inputJson = File.ReadAllText("../../../Datasets/products.json");

            // string result = ImportProducts(db, inputJson);

            // string inputJson = File.ReadAllText("../../../Datasets/categories.json");

            // string result = ImportCategories(db, inputJson);

            // string inputJson = File.ReadAllText("../../../Datasets/categories-products.json");

            // string result = ImportCategoryProducts(db, inputJson);


            // Console.WriteLine(result);

            if (!Directory.Exists("../../../Datasets/Result/"))
            {
                Directory.CreateDirectory("../../../Datasets/Result/");
            }

            // File.WriteAllText("../../../Datasets/Result/products-in-range.json", GetProductsInRange(db));

            // File.WriteAllText("../../../Datasets/Result/users-sold-products.json", GetSoldProducts(db));

            File.WriteAllText("../../../Datasets/Result/users-and-products.json", GetUsersWithProducts(db));




        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            User[] users = JsonConvert.DeserializeObject<User[]>(inputJson);

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            Product[] products = JsonConvert.DeserializeObject<Product[]>(inputJson);

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            //JsonSerializerSettings settings = new JsonSerializerSettings
            //{
            //    NullValueHandling = NullValueHandling.Ignore,

            //}; 



            Category[] categories = JsonConvert.DeserializeObject<Category[]>(inputJson)
                .Where(x => x.Name != null)
                .ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            CategoryProduct[] categoriesProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

            context.CategoryProducts.AddRange(categoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Length}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context
                .Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new
                {
                    name = p.Name,
                    price = p.Price,
                    seller = p.Seller.FirstName + ' ' + p.Seller.LastName
                })
                .OrderBy(x => x.price)
                .ToList();

            string json = JsonConvert.SerializeObject(products, Formatting.Indented);

            return json;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId != null))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    soldProducts = u.ProductsSold
                    .Where(p => p.BuyerId != null)
                    .Select(p => new
                    {
                        name = p.Name,
                        price = p.Price,
                        buyerFirstName = p.Buyer.FirstName,
                        buyerLastName = p.Buyer.LastName
                    }).ToArray()
                })
                .ToArray();

            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            return json;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context
                .Categories

                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoryProducts.Count,
                    averagePrice = c.CategoryProducts.Average(cp => cp.Product.Price).ToString("f2"),
                    totalRevenue = c.CategoryProducts
                   .Sum(cp => cp.Product.Price)
                   .ToString("f2")
                })
                .OrderByDescending(c => c.productsCount)
                .ToArray();

            string json = JsonConvert.SerializeObject(categories, Formatting.Indented);

            return json;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {

            var users = context.Users

                .Where(u => u.ProductsSold.Any(ps => ps.Buyer != null))

                .Select(u => new
                {
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Count(p => p.Buyer != null),
                        products = u.ProductsSold
                        .Where(x => x.Buyer != null)
                        .Select(p => new
                        {
                            name = p.Name,
                            price = p.Price
                        }).ToArray()


                    }
                })

                .OrderByDescending(x => x.soldProducts.count)
                .ToArray();
              

            var resultObj = new
            {
                usersCount = users.Length,
                users = users
            };

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };




            string json = JsonConvert.SerializeObject(resultObj, settings);

            return json;

        }
    }
}