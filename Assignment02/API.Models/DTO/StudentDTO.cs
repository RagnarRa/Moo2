using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models.DTO
{
    /// <summary>
    /// A representation of a student. 
    /// </summary>
    public class StudentDTO
    {
        /// <summary>
        /// The ID of the student. 
        /// Example: 1
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The SSN of the Student.
        /// Example: 1234567890
        /// </summary>
        public string SSN { get; set; }
        /// <summary>
        /// The name of the Student.
        /// Example: Jón Jónsson
        /// </summary>
        public string Name { get; set; }
    }
}
