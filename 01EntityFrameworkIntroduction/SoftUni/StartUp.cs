using Microsoft.EntityFrameworkCore;
using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            SoftUniContext context = new SoftUniContext();

            //string result = GetEmployeesFullInformation(context);

            //string result = GetEmployeesWithSalaryOver50000(context);

            //string result = GetEmployeesFromResearchAndDevelopment(context);


            //string result = AddNewAddressToEmployee(context);

            //string result = GetEmployeesInPeriod(context);

            //string result = GetAddressesByTown(context);

            //string result = GetEmployee147(context);

            //string result = GetDepartmentsWithMoreThan5Employees(context);

            //string result = GetLatestProjects(context);

            //string result = IncreaseSalaries(context);

            string result = GetEmployeesByFirstNameStartingWithSa(context);

            //string result = DeleteProjectById(context);

            //string result = RemoveTown(context);



            Console.WriteLine(result);
            
        }

        // Problem 03
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Employee e in context.Employees.OrderBy(x => x.EmployeeId))
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} {e.MiddleName} {e.JobTitle} {e.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        //Problem 04

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)

        {
            StringBuilder sb = new StringBuilder();

            var employeesSalaryOver50000 = context.Employees
                .Where(e => e.Salary > 50000)
                .Select(e => new
                {
                    e.FirstName,
                    e.Salary
                }
                )
                .ToList();

            foreach (var e in employeesSalaryOver50000.OrderBy(x => x.FirstName))
            {

                sb.AppendLine($"{e.FirstName} - {e.Salary:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        // Problem 05

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .Where(x => x.Department.Name == "Research and Development")
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    DepartmentName = x.Department.Name,
                    x.Salary
                }
                )

                .OrderBy(x => x.Salary)
                .ThenByDescending(x => x.FirstName)
                .ToList();


            foreach (var e in employees)
            {

                sb.AppendLine($"{e.FirstName} {e.LastName} from {e.DepartmentName} - ${e.Salary:f2}");
            }

            return sb.ToString().TrimEnd();

        }

        // Problem 06

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            Address newAddress = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            Employee employeeNakov = context.Employees.First(e => e.LastName == "Nakov");

            employeeNakov.Address = newAddress;

            context.SaveChanges();

            var addresses = context
                .Employees
                .OrderByDescending(a => a.AddressId)
                .Take(10)
                .Select(a => a.Address.AddressText)
                .ToList();

            foreach (var address in addresses)
            {
                sb.AppendLine(address);
            }

            return sb.ToString().TrimEnd();

        }

        ////Problem 07

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employess = context
                .Employees
                .Where(e => e.EmployeesProjects
                .Any(p => p.Project.StartDate.Year >= 2001 &&
                     p.Project.StartDate.Year <= 2003))
                .Take(10)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,
                    Projects = e.EmployeesProjects
                   .Select(ep => new
                   {
                       ProjectName = ep.Project.Name,
                       StartDate = ep.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt",
                      CultureInfo.InvariantCulture),
                       EndDate = ep.Project.EndDate.HasValue ?
                                ep.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt",
                                CultureInfo.InvariantCulture) : "not finished"



                   }).ToList()


                }
                ).ToList();

            foreach (var e in employess)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName} {e.ManagerLastName}");

                foreach (var p in e.Projects)
                {
                    sb.AppendLine($"--{p.ProjectName} - {p.StartDate} - {p.EndDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        ////Problem 08

        public static string GetAddressesByTown(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var addresses = context.Addresses
                .OrderByDescending(a => a.Employees.Count())
                .ThenBy(a => a.Town.Name)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .Select(a => new
                {
                    a.AddressText,
                    TownName = a.Town.Name,
                    EmployeeCount = a.Employees.Count()

                })
                .ToList();

            foreach (var a in addresses)
            {
                sb.AppendLine($"{a.AddressText}, {a.TownName} - {a.EmployeeCount} employees");

            }

            return sb.ToString().TrimEnd();
        }

        ////Problem 09

        public static string GetEmployee147(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employee147 = context.Employees
                .Where(e => e.EmployeeId == 147)
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    JobTitle = e.JobTitle,
                    Projects = e.EmployeesProjects
                           .Select(p => new
                           {
                               ProjectName = p.Project.Name
                           }).OrderBy(p => p.ProjectName).ToList()

                }
                ).ToList();

            foreach (var employee in employee147)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

                foreach (var p in employee.Projects)
                {
                    sb.AppendLine(p.ProjectName);
                }
            }

            return sb.ToString().TrimEnd();
        }

        ////Problem 10

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var departments = context.Departments

                .Where(d => d.Employees.Count() > 5)
                .OrderBy(d => d.Employees.Count())
                .ThenBy(d => d.Name)
                .Select(d => new
                {
                    d.Name,
                    ManagerFirstName = d.Manager.FirstName,
                    ManagerLastName = d.Manager.LastName,
                    DepEmployees = d.Employees
                    .Select(e => new
                    {
                        EmployeeFirstname = e.FirstName,
                        EmployeeLastName = e.LastName,
                        e.JobTitle

                    })
                    .OrderBy(e => e.EmployeeFirstname)
                    .ThenBy(e => e.EmployeeLastName)
                    .ToList()

                }).ToList();





            foreach (var department in departments)
            {
                sb.AppendLine($"{department.Name} - {department.ManagerFirstName} {department.ManagerLastName}");

                foreach (var employee in department.DepEmployees)
                {
                    sb.AppendLine($"{employee.EmployeeFirstname} {employee.EmployeeLastName} - {employee.JobTitle}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        ////Problem 11

        public static string GetLatestProjects(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var projects = context.Projects
                .OrderByDescending(sd => sd.StartDate)
                .Take(10)
                .Select(p => new
                {

                    ProjectName = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                }
                ).OrderBy(x => x.ProjectName)
                .ToList();

            foreach (var p in projects)
            {
                sb.AppendLine(p.ProjectName);
                sb.AppendLine(p.Description);
                sb.AppendLine(p.StartDate);
            }

            return sb.ToString().TrimEnd();
        }


        ////Problem 12

        public static string IncreaseSalaries(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            List<string> selectedDepartments = new List<string>
                {
                  "Engineering",
                  "Tool Design",
                  "Marketing",
                  "Information Services"
                };


            var employeesWithIncreaseSalaries = context.Employees
                .Where(x => selectedDepartments.Contains(x.Department.Name))
                .Select(e => new
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Salary = e.Salary + e.Salary * 0.12m

                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            context.SaveChanges();

            foreach (var e in employeesWithIncreaseSalaries)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:f2})");
            }


            return sb.ToString().TrimEnd();
        }

        ////Problem 13

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employees = context.Employees
                .Where(f => f.FirstName.StartsWith("Sa"))
                .Select(e => new
                {

                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary

                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})");
            }

            return sb.ToString().TrimEnd();
        }

        ////Problem 14

        public static string DeleteProjectById(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var project = context.Projects.Find(2);

            var eployeesProjects = context.EmployeesProjects
                .Where(x => x.ProjectId == 2)
                .ToList();

            context.EmployeesProjects.RemoveRange(eployeesProjects);

            context.Remove(project);

            context.SaveChanges();

            var projects = context.Projects
                .Select(x => new
                {
                    x.Name
                })
                .Take(10)
                .ToList();

            foreach (var item in projects)
            {
                sb.AppendLine(item.Name);
            }

            return sb.ToString().TrimEnd();
        }

        ////Problem 15

        public static string RemoveTown(SoftUniContext context)

        {
            StringBuilder sb = new StringBuilder();

            Town townToDel = context.Towns
                .First(x => x.Name == "Seattle");

            IQueryable<Address> addressesToDel = context.Addresses
                 .Where(a => a.TownId == townToDel.TownId);

            int addressesCount = addressesToDel.Count();

            IQueryable<Employee> employeesSetToNull = context.Employees
                .Where(e => addressesToDel.Any(a => a.AddressId == e.AddressId));

            foreach (Employee employee in employeesSetToNull)
            {
                employee.AddressId = null;
            }

            foreach (Address address in addressesToDel)
            {
                context.Addresses.Remove(address);
            }

            context.Towns.Remove(townToDel);
            context.SaveChanges();

            return $"{addressesCount} addresses in {townToDel.Name} were deleted";

        }
    }
}

