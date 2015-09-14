using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models.ViewModels
{
    /// <summary>
    /// Used for updating a course. 
    /// </summary>
    public class CourseUpdateViewModel
    {
        /// <summary>
        /// The date when the course starts. 
        /// Example: 2015-08-17
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }
        /// <summary>
        /// The date when the course ends.
        /// Example: 2015-08-17
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }
    }
}
