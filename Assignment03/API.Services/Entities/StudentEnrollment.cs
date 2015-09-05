using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services.Entities
{
    /// <summary>
    /// The table used for student enrollment. 
    /// </summary>
    class StudentEnrollment
    {
        /// <summary>
        /// The ID of the student that is enrolled in a course.
        /// Example: 1
        /// </summary>
        [Key]
        [Column(Order=1)]
        public int StudentID { get; set; }
        /// <summary>
        /// The ID of the course that a student is enrolled in.
        /// Example: 1
        /// </summary>
        [Key]
        [Column(Order=2)]
        public int CourseID { get; set; }

        /// <summary>
        /// True if the user is on the waiting list for the course, false otherwise. 
        /// Example: true
        /// </summary>
        public bool IsOnWaitingList { get; set; }
        /// <summary>
        /// True if the user is deleted, false otherwise.
        /// Example: true
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
