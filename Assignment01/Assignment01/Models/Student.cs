using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Assignment01.Models
{
    public class Student
    {
        [Required]
        public string SSN { get; set; }
        [Required]
        public string Name { get; set; }
    }
}