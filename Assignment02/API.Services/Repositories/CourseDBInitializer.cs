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
            context.Courses.Add(new Course { ID = 1, TemplateID = "T-514-VEFT", Semester ="20143", StartDate = new DateTime(2014, 8, 17), EndDate = new DateTime(2014, 11, 14) } );
            context.Courses.Add(new Course { ID = 2, TemplateID = "T-514-VEFT", Semester ="20153", StartDate = new DateTime(2015, 8, 17), EndDate = new DateTime(2015, 11, 14) } );
            context.Courses.Add(new Course { ID = 3, TemplateID = "T-111-PROG", Semester ="20143", StartDate = new DateTime(2014, 8, 17), EndDate = new DateTime(2014, 11, 14) } );
            context.SaveChanges();

            context.CourseTemplates.Add(new CourseTemplate { ID = 1, Name = "Web services", TemplateID = "T-514-VEFT" });
            context.CourseTemplates.Add(new CourseTemplate { ID = 2, Name = "Programming", TemplateID = "T-111-PROG" });

            context.Students.Add(new Student { ID = 1, Name = "Jón Jónsson", SSN = "1234567890" });
            context.Students.Add(new Student { ID = 2, Name = "Guðrún Jónsdóttir", SSN = "9876543210" });
            context.Students.Add(new Student { ID = 3, Name = "Gunnar Sigurðsson", SSN = "6543219870" });
            context.Students.Add(new Student { ID = 4, Name = "Jóna Halldórsdóttir", SSN = "4567891230" });

            context.SaveChanges();
            //base.Seed(context);
        }
    }
}
