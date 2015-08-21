using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Assignment01.Models
{
    public class Course
    {
        [Required]
        public int ID { get; set; }
        /// <summary>
        /// F. example "T-514-VEFT"
        /// </summary>
        [Required]
        public string TemplateID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        public List<Student> Students { get; set; }
    }
}