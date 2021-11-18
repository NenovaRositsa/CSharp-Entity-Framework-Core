using AutoMapper;
using CarDealer.Data;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            CarDealerContext db = new CarDealerContext();

            //Mapper.Initialize(cfg =>
            // {
            //     cfg.AddProfile<CarDealerProfile>();
            // });


            //db.Database.EnsureDeleted();
            //db.Database.EnsureCreated();

            //string xmlSuppliers = File.ReadAllText("../../../Datasets/suppliers.xml");
            //Console.WriteLine(ImportSuppliers(db, xmlSuppliers));

            //string xmlParts = File.ReadAllText("../../../Datasets/parts.xml");
            //Console.WriteLine(ImportParts(db, xmlParts));

            //string xmlCars = File.ReadAllText("../../../Datasets/cars.xml");
            //Console.WriteLine(ImportCars(db, xmlCars));

            //string xmlCustomers = File.ReadAllText("../../../Datasets/customers.xml");
            //Console.WriteLine(ImportCustomers(db, xmlCustomers));

            string xmlSales = File.ReadAllText("../../../Datasets/sales.xml");
            Console.WriteLine(ImportSales(db, xmlSales));


        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSupplierDTO[]), new XmlRootAttribute("Suppliers"));

            var suppliersDtos = (ImportSupplierDTO[])xmlSerializer.Deserialize(new StringReader(inputXml));

            List<Supplier> suppliers = new List<Supplier>();

            foreach (var dto in suppliersDtos)
            {
                Supplier supplier = new Supplier
                {
                    Name = dto.Name,
                    IsImporter = dto.IsImporter
                };

                suppliers.Add(supplier);
            }

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportPartDTO[]), new XmlRootAttribute("Parts"));

            var partsDtos = (ImportPartDTO[])xmlSerializer.Deserialize(new StringReader(inputXml));



            //Part[] parts = Mapper.Map<Part[]>(partsDtos);



            List<Part> dbParts = new List<Part>();



            foreach (var dto in partsDtos)
            {
                if (!context.Suppliers.Any(s => s.Id == dto.SupplierId))
                {
                    continue;
                }
                else
                {
                    Part part = new Part
                    {
                        Name = dto.Name,
                        Price = dto.Price,
                        Quantity = dto.Quantity,
                        SupplierId = dto.SupplierId
                    };

                    dbParts.Add(part);
                }
            }

            context.Parts.AddRange(dbParts);
            context.SaveChanges();

            return $"Successfully imported {dbParts.Count}";

        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCarDTO[]), new XmlRootAttribute("Cars"));

            var carsDtos = (ImportCarDTO[])xmlSerializer.Deserialize(new StringReader(inputXml));

            List<Car> cars = new List<Car>();


            foreach (var carDto in carsDtos)
            {
                var uniqueParts = carDto.Parts.Select(x => x.Id).Distinct().ToArray();
                var realParts = uniqueParts.Where(id => context.Parts.Any(x => x.Id == id));

                Car car = new Car
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TravelledDistance,
                    PartCars = realParts.Select(y => new PartCar
                    {
                        PartId = y

                    }).ToArray()
                };

                cars.Add(car);
            }

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCustomerDTO[]), new XmlRootAttribute("Customers"));

            var customersDto = (ImportCustomerDTO[])xmlSerializer.Deserialize(new StringReader(inputXml));

            List<Customer> customers = new List<Customer>();

            foreach (var dto in customersDto)
            {
               
                Customer customer = new Customer()
                {
                    Name = dto.Name,
                    BirthDate = dto.BirthDate,
                    IsYoungDriver = dto.IsYoungDriver
                };

                customers.Add(customer);
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSalesDTO[]), new XmlRootAttribute("Sales"));

            var salesDtos = (ImportSalesDTO[])xmlSerializer.Deserialize(new StringReader(inputXml));

            List<Sale> sales = new List<Sale>();

            foreach (var dto in salesDtos)
            {

                if (!context.Cars.Any(c => c.Id == dto.CarId))
                {
                    continue;
                }
                else
                {
                    Sale sale = new Sale()
                    {
                        CarId = dto.CarId,
                        CustomerId = dto.CustomerId,
                        Discount = dto.Discount
                    };

                    sales.Add(sale);
                }
               
            }

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}";
        }


    }
}