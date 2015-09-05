using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services.Entities
{
    /// <summary>
    /// A course template. Used to avoid duplication in the course table. 
    /// </summary>
    class CourseTemplate
    {
        /// <summary>
        /// The ID of the course.
        /// Example: 1
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The templateID for the course template.
        /// Example: T-514-VEFT
        /// </summary>
        public string TemplateID { get; set; }
        /// <summary>
        /// The name of the course. 
        /// Example: Web services
        /// </summary>
        public string Name { get; set; }
    }
}
