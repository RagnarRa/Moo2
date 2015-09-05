using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services.Exceptions
{
    /// <summary>
    /// Used when the application can't find a requested object. 
    /// </summary>
    public class AppObjectNotFoundException : ApplicationException
    {
    }
}
