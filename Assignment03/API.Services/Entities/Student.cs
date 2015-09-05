using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services.Entities
{
    /// <summary>
    /// The student table. Includes student records. 
    /// </summary>
    class Student
    {
        /// <summary>
        /// The ID of the student.
        /// Example: 1
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The name of the student.
        /// Example: Jón Jónsson
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The SSN of the student.
        /// Example: 1234567890
        /// </summary>
        public string SSN { get; set; }
    }
}
