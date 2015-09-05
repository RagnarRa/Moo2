using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models.ViewModels
{
    /// <summary>
    /// For user input to create a course with. 
    /// </summary>
    public class CourseViewModel
    {
        /// <summary>
        /// The template ID of the course. 
        /// Example: T-514-VEFT
        /// </summary>
        [Required]
        public string TemplateID { get; set; }
        /// <summary>
        /// The semester the course is on.
        /// Example: 20153
        /// </summary>
        [Required]
        public string Semester { get; set; }
        /// <summary>
        /// The start datea of the course.
        /// Example: 2015-08-17
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// The end date of the course.
        /// Example: 2015-08-17
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The maximum allowed students in the course.
        /// Example: 1
        /// </summary>
        [Required]
        public int MaxStudents { get; set; }
    }
}
