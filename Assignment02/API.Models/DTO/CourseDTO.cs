using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models.DTO
{
    /// <summary>
    /// A less detailed representation of a course. 
    /// Typically used when you don't want all the details of a course to display. 
    /// </summary>
    public class CourseDTO
    {
        /// <summary>
        /// The ID of the course.
        /// Example: 1
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The start date of the course. 
        /// Example: 2015-08-17
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// The semester of the course. 
        /// Example: 20153
        /// </summary>
        public string Semester { get; set; }
        /// <summary>
        /// The name of the course.
        /// Example: Web services
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The number of students in a course.
        /// Example: 1
        /// </summary>
        public int StudentCount { get; set; }
    }
}
