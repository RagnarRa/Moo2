using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services.Exceptions
{
    /// <summary>
    /// Thrown when a user tries to add a student to a course when the given student is already registered to it. 
    /// </summary>
    public class DuplicateCourseRegistrationException : ApplicationException
    {
    }
}
