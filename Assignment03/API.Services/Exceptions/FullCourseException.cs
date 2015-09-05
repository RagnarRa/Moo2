using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services.Exceptions
{
    /// <summary>
    /// Used when a user tries to add a student to a course that is full. 
    /// </summary>
    public class FullCourseException : ApplicationException
    {
    }
}
