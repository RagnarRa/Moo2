using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models.ViewModels
{
    /// <summary>
    /// Used for adding a student to a course. 
    /// </summary>
    public class StudentAddToCourseViewModel
    {
        /// <summary>
        /// The SSN of the student.
        /// Example: 1234567890 (except, a legitimate SSN, and not that)
        /// </summary>
        [Required]
        public string SSN { get; set; }
    }
}
