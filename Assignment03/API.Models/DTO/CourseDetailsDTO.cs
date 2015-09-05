using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models.DTO
{
    /// <summary>
    /// The detailed representation of a course. 
    /// Typically used when the user wants a detailed description of a course. 
    /// </summary>
    public class CourseDetailsDTO
    {
        /// <summary>
        /// The ID of the course.
        /// Example: 1
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The templateID of the course.
        /// Example: T-514-VEFT
        /// </summary>
        public string TemplateID { get; set; }
        /// <summary>
        /// The start date of the course.
        /// Example: 2015-08-17
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// The end date of the course.
        /// Example: 2015-08-17
        /// </summary>
        public DateTime EndDate { get; set; }
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
        /// The list of students for this course. 
        /// Example: Well.. it's a list of Student objects. Check the documentation there.
        /// </summary>
        public List<StudentDTO> Students { get; set; }
        /// <summary>
        /// The maximum amount of students allowed for this course. 
        /// Example: 1
        /// </summary>
        public int MaxStudents { get; set; }
    }
}
