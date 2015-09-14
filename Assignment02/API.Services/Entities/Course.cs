using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services.Entities
{
    /// <summary>
    /// A representaiton of a course. 
    /// </summary>
    class Course
    {
        /// <summary>
        /// The ID of the course.
        /// Example: 1
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The templateID for the course.
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
    }
}
