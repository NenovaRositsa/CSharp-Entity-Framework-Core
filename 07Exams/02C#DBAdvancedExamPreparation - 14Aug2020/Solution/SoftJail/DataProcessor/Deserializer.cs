namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data";
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            Department[] allDepartments = JsonConvert.DeserializeObject<Department[]>(jsonString);

            List<Department> validDepartments = new List<Department>();

            foreach (var dep in allDepartments)
            {
                bool isValid = IsValid(dep) && dep.Cells.All(x => IsValid(x));
      

                
                if (isValid)
                {
                    validDepartments.Add(dep);
                    sb.AppendLine($"Imported {dep.Name} with {dep.Cells.Count} cells");
                }
                else
               {
                    sb.AppendLine("Invalid Data");
                    continue;
                }
               

               

            }

            context.Departments.AddRange(validDepartments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            throw new NotImplementedException();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            throw new NotImplementedException();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}