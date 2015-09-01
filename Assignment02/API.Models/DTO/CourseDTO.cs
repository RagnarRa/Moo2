using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Models.DTO
{
    public class CourseDTO
    {
        public int ID { get; set; }
        public DateTime StartDate { get; set; }
        public string Semester { get; set; }
        public string Name { get; set; }
        public int StudentCount { get; set; }
    }
}
