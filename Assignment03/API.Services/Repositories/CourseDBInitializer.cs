using API.Services.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services.Repositories
{
    /// <summary>
    /// The class used for seeding the database. 
    /// </summary>
    class CourseDBInitializer : DropCreateDatabaseAlways<AppDataContext>
    {
        protected override void Seed(AppDataContext context)
        {
            context.CourseTemplates.Add(new CourseTemplate { ID = 1, Name = "Vefþjónustur", TemplateID = "T-514-VEFT" });
            context.SaveChanges();

            context.Students.Add(new Student { ID = 1, Name = "Herp McDerpsson 1", SSN = "1234567890" });
            context.Students.Add(new Student { ID = 2, Name = "Herpina Derpy 1", SSN = "1234567891" });
            context.Students.Add(new Student { ID = 3, Name = "Herp McDerpsson 2", SSN = "1234567892" });
            context.Students.Add(new Student { ID = 4, Name = "Herpina Derpy 2", SSN = "1234567893" });
            context.Students.Add(new Student { ID = 5, Name = "Herp McDerpsson 3", SSN = "1234567894" });
            context.Students.Add(new Student { ID = 6, Name = "Herpina Derpy 3", SSN = "1234567895" });
            context.Students.Add(new Student { ID = 7, Name = "Herp McDerpsson 4", SSN = "1234567896" });
            context.Students.Add(new Student { ID = 8, Name = "Herpina Derpy 4", SSN = "1234567897" });
            context.Students.Add(new Student { ID = 9, Name = "Herp McDerpsson 5", SSN = "1234567898" });
            context.Students.Add(new Student { ID = 10, Name = "Herpina Derpy 5", SSN = "1234567899" });

            context.SaveChanges();
            //base.Seed(context);
        }
    }
}
