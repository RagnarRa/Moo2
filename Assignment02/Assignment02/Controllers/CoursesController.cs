using API.Models;
using API.Models.DTO;
using API.Models.ViewModels;
using API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Assignment02.Controllers
{
    /// <summary>
    /// This controller represents...
    /// </summary>
    [RoutePrefix("api/courses")]
    public class CoursesController : ApiController
    {
        private readonly CoursesServiceProvider _service;

        public CoursesController()
        {
            _service = new CoursesServiceProvider();
        }

        /// <summary>
        /// Todo: Approve. 
        /// </summary>
        /// <param name="semester"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(List<CourseDTO>))]
        public IHttpActionResult GetCourses(string semester)
        {
            List<CourseDTO> courseList = _service.GetCoursesBySemester(semester);

            if (courseList == null)
            {
                return NotFound();
            }

            return Ok(courseList);
        }

        [HttpGet] 
        [Route("{ID:int}", Name="GetCourseByID")]
        [ResponseType(typeof(CourseDetailsDTO))]
        public IHttpActionResult GetCourse(int ID)
        {
            //CourseDetailDTO
            CourseDetailsDTO retCourse = _service.GetCourseByID(ID);

            if (retCourse == null)
            {
                return NotFound();
            }

            return Ok(retCourse);
        }

        
        /// <summary>
        /// Updates the given course. Only the StartDate and EndDate properties are mutable. 
        /// </summary>
        /// <param name="ID">The ID of the course.</param>
        /// <param name="courseVM">The course VM.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{ID:int}")]
        public IHttpActionResult UpdateCourse(int ID, CourseUpdateViewModel courseVM)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.PreconditionFailed);
            }

            bool updateSuccessful = _service.UpdateCourse(ID, courseVM);

            if (!updateSuccessful) {
                return NotFound();
            }

            return Ok();
        }

        /// <summary>
        /// Will return 204 NoContent if successful, otherwise NotFound.
        /// </summary>
        /// <param name="ID"></param>
        [HttpDelete]
        [Route("{ID:int}")]
        public void DeleteCourse(int ID)
        {
            bool deleteSuccessful = _service.DeleteCourse(ID);

            if (!deleteSuccessful)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        /// <summary>
        /// Gets a list of students for the given course.
        /// </summary>
        /// <param name="courseID">The ID of the course.</param>
        /// <returns>A list of students.</returns>
        [HttpGet] 
        [Route("{courseID:int}/students")]
        [ResponseType(typeof(List<StudentDTO>))]
        public IHttpActionResult GetStudents(int courseID)
        {
            List<StudentDTO> students = _service.GetStudentsByCourse(courseID);

            if (students == null)
            {
                return NotFound();
            }

            return Ok(students);
        }

        /// <summary>
        /// Adds a student to a course.
        /// </summary>
        /// <param name="courseID">ID of the course.</param>
        /// <param name="studentVM">The student VM.</param>
        /// <returns>Returns 201 Created if successful, 404 if course wasn't found, and 412 if the preconditions failed.</returns>
        [HttpPost]
        [Route("{courseID:int}/students")]
        public HttpResponseMessage AddStudent(int courseID, StudentViewModel studentVM)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.PreconditionFailed);
            }

            bool studentAddedSuccessfully = _service.AddStudentToCourse(courseID, studentVM);

            if (!studentAddedSuccessfully)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //Would return Created(location, studentDTO) but the API has no location for this object.. 
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        /// <summary>
        /// Adds a course.
        /// </summary>
        /// <param name="courseVM"></param>
        /// <returns>If the required fields aren't there, a precondition failed status is returned. Otherwise 201 Created and a CourseDTO object.</returns>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(CourseDTO))]
        public IHttpActionResult AddCourse(CourseViewModel courseVM)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.PreconditionFailed);
            }

            CourseDTO courseDTO = _service.AddCourse(courseVM);

            var location = Url.Link("GetCourseByID", new { ID = courseDTO.ID });
            return Created(location, courseDTO);
        }
    }
}
