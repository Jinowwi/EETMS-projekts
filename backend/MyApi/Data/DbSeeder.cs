using MyApi.Data;
using MyApi.Models;
using System.Linq;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        // // 1. Rem / Real_Estate
        // Rem realEstate;
        // if (!context.Real_Estate.Any())
        // {
        //     realEstate = new Rem
        //     {
        //         email = "rem1@example.com",
        //         FirstName = "Aaaa",
        //         LastName = "Bbbb",
        //         PhoneNumber = "+3712222222"
        //     };
        //     context.Real_Estate.Add(realEstate);
        //     context.SaveChanges();          
        // }
        // else
        // {
        //     realEstate = context.Real_Estate.First();
        // }

        // // 2. Company
        // Company company;
        // if (!context.Companies.Any())
        // {
        //     var companies = new[]
        //     {
        //         new Company { PhoneNumber = "+37121212133", 
        //             Country = Country.Latvia, 
        //             Address = "Brīvības iela 1, Rīga", 
        //             RegistrationNumber = "40001019191", 
        //             CompanyName = "Coca Cola Bottlers LV", 
        //             RemID = realEstate.RemID }, 

        //         new Company { PhoneNumber = "+37124444444", 
        //             Country = Country.Latvia, 
        //             Address = "Kaļķu iela 12, Centrs, Rīga", 
        //             RegistrationNumber = "40002020202", 
        //             CompanyName = "PepsiCo Latvia",
        //             RemID = realEstate.RemID },
                
        //         new Company { PhoneNumber = "+37125555555", 
        //             Country = Country.Latvia, 
        //             Address = "Miera iela 10, Teika, Rīga", 
        //             RegistrationNumber = "40103030303", 
        //             CompanyName = "Orkla Latvia",
        //             RemID = realEstate.RemID },

        //         new Company { PhoneNumber = "+37126666666", 
        //             Country = Country.Latvia, 
        //             Address = "Lāčplēša 45, Āgenskalns, Rīga", 
        //             RegistrationNumber = "40104040404", 
        //             CompanyName = "Unilever Baltic",
        //             RemID = realEstate.RemID },

        //         new Company { PhoneNumber = "+37127777777", 
        //             Country = Country.Latvia, 
        //             Address = "Aleksandra Čaka 23, Rīga", 
        //             RegistrationNumber = "40105050505", 
        //             CompanyName = "Procter & Gamble Baltics",
        //             RemID = realEstate.RemID },
        //     };        
        //     context.Companies.AddRange(companies); 
        //     context.SaveChanges(); 
        //     company = companies[0]; 
        // }
        // else
        // {
        //     company = context.Companies.First();
        // }

        // // 3. Employee
        // if (!context.Employees.Any())
        // {
        //     var employees = new []
        //     {
        //         new ExternalEmployee { PhoneNumber = "+37123456789", CompanyID = context.Companies.First().CompanyID },
        //         new ExternalEmployee { PhoneNumber = "+37123456790", CompanyID = context.Companies.Skip(1).First().CompanyID },
        //         new ExternalEmployee { PhoneNumber = "+37123456791", CompanyID = context.Companies.Skip(2).First().CompanyID },
        //         new ExternalEmployee { PhoneNumber = "+37123456792", CompanyID = context.Companies.Skip(3).First().CompanyID },
        //         new ExternalEmployee { PhoneNumber = "+37123456793", CompanyID = context.Companies.Skip(4).First().CompanyID }
        //     };
        //     context.Employees.AddRange(employees);
        //     context.SaveChanges();
        // }

        // // 4. Shops
        // if (!context.Shops.Any())
        // {
        //     var shops = new []
        //     {
        //         new Shop { Email = "hyper1@shops.lv", Address = "Brīvības iela 100, Rīga", Type = ShopType.Hyper, Code = "LV1100" },
        //         new Shop { Email = "super1@shops.lv", Address = "Kaļķu iela 25, Centrs, Rīga", Type = ShopType.Super, Code = "LV1200" },
        //         new Shop { Email = "hyper2@shops.lv", Address = "Miera iela 50, Teika, Rīga", Type = ShopType.Hyper, Code = "LV2100" },
        //         new Shop { Email = "super2@shops.lv", Address = "Lāčplēša 80, Āgenskalns, Rīga", Type = ShopType.Super, Code = "LV2200" },
        //         new Shop { Email = "hyper3@shops.lv", Address = "Aleksandra Čaka 15, Rīga", Type = ShopType.Hyper, Code = "LV3100" }
        //     };
        //     context.Shops.AddRange(shops); 
        //     context.SaveChanges(); 
        // }

        // // 5. Reasons 
        // if (!context.Reasons.Any())
        // {
        //     var reasons = new []
        //     {
        //         new Reason { Name = "Cleaning", CompanyID = context.Companies.First().CompanyID },
        //         new Reason { Name = "Cameras", CompanyID = context.Companies.First().CompanyID },
        //         new Reason { Name = "Plumber", CompanyID = context.Companies.Skip(1).First().CompanyID },
        //         new Reason { Name = "Guard", CompanyID = context.Companies.Skip(1).First().CompanyID }
        //     }; 
        //     context.Reasons.AddRange(reasons); 
        //     context.SaveChanges(); 
        // }

        // // 6. Shifts 
        // if (!context.Shifts.Any())
        // {
        //     var shifts = new []
        //     {
        //         new Shift { start_date = new DateOnly(2025,1,10), end_date = new DateOnly(2025,1,10), 
        // start_time = new TimeOnly(9,0), end_time = new TimeOnly(17,0), EmployeeID = context.Employees.First().EmployeeID, 
        // ShopID = context.Shops.First().ShopID, ReasonID = context.Reasons.First().ReasonID },
        //         new Shift { start_date = new DateOnly(2025,1,11), end_date = new DateOnly(2025,1,11), 
        // start_time = new TimeOnly(14,0), end_time = new TimeOnly(22,0), EmployeeID = context.Employees.Skip(1).First().EmployeeID, 
        // ShopID = context.Shops.Skip(1).First().ShopID, ReasonID = context.Reasons.First().ReasonID },
        //         new Shift { start_date = new DateOnly(2025,1,12), end_date = new DateOnly(2025,1,12), 
        // start_time = new TimeOnly(8,0), end_time = new TimeOnly(16,0), EmployeeID = context.Employees.Skip(2).First().EmployeeID, 
        // ShopID = context.Shops.Skip(2).First().ShopID, ReasonID = context.Reasons.First().ReasonID },
        //         new Shift { start_date = new DateOnly(2025,1,13), end_date = new DateOnly(2025,1,13), 
        // start_time = new TimeOnly(22,0), end_time = new TimeOnly(6,0), EmployeeID = context.Employees.Skip(3).First().EmployeeID, 
        // ShopID = context.Shops.Skip(3).First().ShopID, ReasonID = context.Reasons.First().ReasonID },
        //         new Shift { start_date = new DateOnly(2025,1,14), end_date = new DateOnly(2025,1,14), 
        // start_time = new TimeOnly(7,0), end_time = new TimeOnly(15,0), EmployeeID = context.Employees.Skip(4).First().EmployeeID, 
        // ShopID = context.Shops.First().ShopID, ReasonID = context.Reasons.First().ReasonID }
        //     }; 
        //     context.Shifts.AddRange(shifts); 
        //     context.SaveChanges(); 
        // }

        // // 7. Reminders 
        // if (!context.Reminders.Any())
        // {
        //     var reminders = new []
        //     {
        //         new Reminder { Body = "Missed punch at 2025-01-10 09:00", 
        //         RemID = context.Real_Estate.First().RemID, ShiftID = context.Shifts.First().ShiftID },
        //         new Reminder { Body = "Late arrival on 2025-01-11 14:00",
        //         RemID = context.Real_Estate.First().RemID, ShiftID = context.Shifts.First().ShiftID },
        //         new Reminder { Body = "No endTime for shift 2025-01-12",
        //         RemID = context.Real_Estate.First().RemID, ShiftID = context.Shifts.First().ShiftID },
        //         new Reminder { Body = "Early leave detected 2025-01-13",
        //         RemID = context.Real_Estate.First().RemID, ShiftID = context.Shifts.First().ShiftID },
        //         new Reminder { Body = "Night shift overlap 2025-01-14",
        //         RemID = context.Real_Estate.First().RemID, ShiftID = context.Shifts.First().ShiftID }
        //     }; 
        //     context.Reminders.AddRange(reminders); 
        //     context.SaveChanges(); 
        // }
    }
}
