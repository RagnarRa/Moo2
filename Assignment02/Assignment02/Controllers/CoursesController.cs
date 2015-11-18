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
    /// The course controller. Represents the API for various operations regarding courses. 
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
        /// Gets a list of courses for a given semester.
        /// If no semester is given, the current one is used.
        /// </summary>
        /// <param name="semester">The semester. Example: 20153</param>
        /// <returns>A list of courses if there are any courses for the semester. Otherwise 404.</returns>
        [HttpGet]
        [Route("")]
        [ResponseType(typeof(List<CourseDTO>))]
        public IHttpActionResult GetCourses(string semester = null)
        {
            List<CourseDTO> courseList = _service.GetCoursesBySemester(semester);

            if (courseList.Count() == 0)
            {
                return NotFound();
            }

            return Ok(courseList);
        }

        /// <summary>
        /// Gets a course by ID. 
        /// </summary>
        /// <param name="ID">The ID of the course.</param>
        /// <returns>The given course, or 404 if we can't find it.</returns>
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
        /// <returns>412 if a required property is not present. NotFound if we can't find the course, or 200 if it's successful.</returns>
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

            return Ok(); //Skila líklega Ok(object) eða content(StatusCode.Ok, CourseDTO); 
        }

        /// <summary>
        /// Deletes a course.
        /// Will return 204 NoContent if successful, otherwise NotFound.
        /// </summary>
        /// <param name="ID">The ID of the course to delete.</param>
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
        /// <returns>A list of students in the course if we find any, if we don't find the course, returns 404.</returns>
        [HttpGet] 
        [Route("{courseID:int}/students")]
        [ResponseType(typeof(List<StudentDTO>))]
        public IHttpActionResult GetStudents(int courseID)
        {
            List<StudentDTO> students = _service.GetStudentsByCourse(courseID);

            //Null means the course didn't exist..
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
                throw new HttpResponseException(HttpStatusCode.NotFound); //Í IHttpActionResult.. getur gert return StatusCode(...)
            }

            //Would return Created(location, studentDTO) but the API has no location for this object.. 
            //Instead of this, want to use IHttpActionResult and [ResponseType].. then return Content(HttpStatusCode.Created, studentDTO) 
            return new HttpResponseMessage(HttpStatusCode.Created);
        }

        /// <summary>
        /// Adds a course.
        /// </summary>
        /// <param name="courseVM">The course VM.</param>
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

            //If the courseDTO is null, it means we didn't find a CourseTemplate with the given ID
            if (courseDTO == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var location = Url.Link("GetCourseByID", new { ID = courseDTO.ID });
            return Created(location, courseDTO);
        }
    }
}
