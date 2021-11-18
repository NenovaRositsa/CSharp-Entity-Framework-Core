using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO.Export;
using CarDealer.DTO.Import;
using CarDealer.Models;
using Newtonsoft.Json;

namespace CarDealer
{
    public class StartUp
    {
        private const string RESULT = "Successfully imported {0}.";
        public static void Main(string[] args)
        {

            CarDealerContext db = new CarDealerContext();

            //db.Database.EnsureDeleted();
            //db.Database.EnsureCreated();


            //string inputJson = File.ReadAllText("../../../Datasets/suppliers.json");
            //Console.WriteLine(ImportSuppliers(db, inputJson));

            //string inputJson1 = File.ReadAllText("../../../Datasets/parts.json");
            //Console.WriteLine(ImportParts(db, inputJson1));

            //string inputJson2 = File.ReadAllText("../../../Datasets/cars.json");
            //Console.WriteLine(ImportCars(db, inputJson2));

            //string inputJson3 = File.ReadAllText("../../../Datasets/customers.json");
            //Console.WriteLine(ImportCustomers(db, inputJson3));

            //string inputJson4 = File.ReadAllText("../../../Datasets/sales.json");
            //Console.WriteLine(ImportSales(db, inputJson4));


            if (!Directory.Exists("../../../Datasets/Result/"))
            {
                Directory.CreateDirectory("../../../Datasets/Result/");
            }

            File.WriteAllText("../../../Datasets/Result/sales-discounts.json", GetSalesWithAppliedDiscount(db));


        }

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            Supplier[] suppliers = JsonConvert.DeserializeObject<Supplier[]>(inputJson);

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}.";
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {


            Part[] parts = JsonConvert.DeserializeObject<Part[]>(inputJson);
            var suppliers = context.Suppliers
              .Select(s => s.Id).ToArray();
            parts = parts.Where(p => suppliers.Any(s => s == p.SupplierId)).ToArray();


            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Length}.";
        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {


            var carsInput = JsonConvert.DeserializeObject<List<CarDto>>(inputJson);

            List<Car> listOfcars = new List<Car>();
            foreach (var carJson in carsInput)
            {
                Car car = new Car()
                {
                    Make = carJson.Make,
                    Model = carJson.Model,
                    TravelledDistance = carJson.TravelledDistance
                };
                foreach (var partId in carJson.PartsId.Distinct())
                {
                    car.PartCars.Add(new PartCar()
                    {
                        Car = car,
                        PartId = partId
                    });
                }
                listOfcars.Add(car);
            }

            context.Cars.AddRange(listOfcars);
            context.SaveChanges();
            return $"Successfully imported {listOfcars.Count}.";
        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            Customer[] customers = JsonConvert.DeserializeObject<Customer[]>(inputJson);

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Length}.";
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            Sale[] sales = JsonConvert.DeserializeObject<Sale[]>(inputJson);

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Length}.";

        }
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            List<CustomerDTO> customersDTO = context.Customers
                  .OrderBy(c => c.BirthDate)
                  .ThenBy(x => x.IsYoungDriver)
                  .Select(c => new CustomerDTO()
                  {
                      Name = c.Name,
                      BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                      IsYoungDriver = c.IsYoungDriver
                  })

                  .ToList();

            string json = JsonConvert.SerializeObject(customersDTO, Formatting.Indented);

            return json;
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var carsFromMakeToyota = context.Cars
                .Where(c => c.Make == "Toyota")
                .Select(c => new
                {
                    Id = c.Id,
                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance
                })
                .OrderBy(m => m.Model)
                .ThenByDescending(d => d.TravelledDistance)
                .ToList();

            string json = JsonConvert.SerializeObject(carsFromMakeToyota, Formatting.Indented);

            return json;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var localSuppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                }).ToArray();

            string json = JsonConvert.SerializeObject(localSuppliers, Formatting.Indented);

            return json;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                 .Select(c => new
                 {
                     car = new
                     {
                         Make = c.Make,
                         Model = c.Model,
                         TravelledDistance = c.TravelledDistance
                     },
                     parts = c.PartCars.Select(p => new
                     {
                         Name = p.Part.Name,
                         Price = $"{p.Part.Price:F2}"
                     }).ToArray()

                 }).ToArray();

           
            string json = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return json;
        }
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(c => c.Sales.Count >= 1)
                .Select(c => new
                {
                    fullName = c.Name,
                    boughtCars = c.Sales.Count,
                    spentMoney = c.Sales.Sum(s => s.Car.PartCars.Sum(pc => pc.Part.Price))
                })
                .OrderByDescending(x => x.spentMoney)
                .ThenByDescending(x => x.boughtCars)
                .ToArray();

            string json = JsonConvert.SerializeObject(customers,Formatting.Indented);

            return json;
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Take(10)
                .Select(s => new
                {
                    car = new
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance
                    },

                    customerName = s.Customer.Name,
                    Discount = s.Discount.ToString("f2"),
                    price = s.Car.PartCars.Sum(p => p.Part.Price).ToString("f2"),
                    priceWithDiscount = (((100 - s.Discount) / 100) * s.Car.PartCars.Sum(x => x.Part.Price)).ToString("f2")
                }).ToArray();

            string json = JsonConvert.SerializeObject(sales, Formatting.Indented);

            return json;

        }
    }
}