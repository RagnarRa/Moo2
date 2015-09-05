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
    /// The AppDataContext for the database. 
    /// </summary>
    class AppDataContext : DbContext 
    {
        public AppDataContext()
            : base("AppDataContext")
        {
            Database.SetInitializer<AppDataContext>(new CourseDBInitializer());
        }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseTemplate> CourseTemplates { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentEnrollment> StudentEnrollment { get; set; }

    }
}
