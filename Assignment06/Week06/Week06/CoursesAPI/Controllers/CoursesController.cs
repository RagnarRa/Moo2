using System.Web.Http;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Services;
using System.Net;
using WebApi.OutputCache.V2;

namespace CoursesAPI.Controllers
{
	[RoutePrefix("api/courses")]
	public class CoursesController : ApiController
	{
		private readonly CoursesServiceProvider _service;

		public CoursesController()
		{
			_service = new CoursesServiceProvider(new UnitOfWork<AppDataContext>());
		}

        [HttpGet]
        [AllowAnonymous]
        [CacheOutput(ClientTimeSpan = 86400, ServerTimeSpan = 86400)] //Cache for 86400 s. Put the e-tag in If-None-Match 
        public IHttpActionResult GetCoursesBySemester(string semester = null, int page = 1)
        {
            // TODO: figure out the requested language (if any!)
            // and pass it to the service provider!
            return Ok(_service.GetCourseInstancesBySemester(semester, page));
        }

        /// <summary>
        /// A stub used for invalidating the cache for the get courses method.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [InvalidateCacheOutput("GetCoursesBySemester")]
        [Authorize]
        public IHttpActionResult CreateCourse() 
        {
            return StatusCode(HttpStatusCode.Created);
        }

        /// <summary>
        /// A stub used to try authentication. Returns hard-coded data.
        /// </summary>
        /// <param name="id">ID of the course</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [Authorize]
        public CourseInstanceDTO GetCourseById(int id)
        {
            return new CourseInstanceDTO { CourseInstanceID = 1, MainTeacher = "Dabs", Name = "Vefþjónustur", TemplateID = "T-514-VEFT" };
        }

		/// <summary>
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("{id}/teachers")]
		public IHttpActionResult AddTeacher(int id, AddTeacherViewModel model)
		{
			var result = _service.AddTeacherToCourse(id, model);
			return Created("TODO", result);
		}
	}
}
