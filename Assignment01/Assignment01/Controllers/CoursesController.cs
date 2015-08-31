using Assignment01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Assignment01.Controllers
{
    /// <summary>
    /// Known bugs: State is not stored. You can post to the in-memory list, but it will forget it for the next request. 
    /// Was told this would be okay since the assignment's focus was not on storage but rather creating a basic Web API. 
    /// </summary>
    [RoutePrefix("api/v1/assignment01")]
    public class CoursesController : ApiController
    {
        private static List<Course> _courses;
        private static List<Student> _students;
        /// <summary>
        /// In the absence of a DB.. I use this to keep track of the next ID to use. 
        /// </summary>
        private static int _courseID; 

        public CoursesController()
        {
            //If I haven't initiailized the list with data before.. 
            if (_students == null)
            {
                _students = new List<Student>
                {
                    new Student {
                        SSN = "1111111111",
                        Name = "Ragnar Borgþór Ragnarsson"
                    },
                    new Student {
                        SSN = "0000000000",
                        Name = "Jón Agnar Stefánsson"
                    },
                    new Student {
                        SSN = "0101010101",
                        Name = "Snævar Dagur Pétursson"
                    },
                    new Student {
                        SSN = "1010101010",
                        Name = "Guðjón Hólm Sigurðsson"
                    }
                };

                    _courses = new List<Course>
                {
                    new Course {
                        ID = 1, 
                        Name = "Web services",
                        TemplateID = "T-514-VEFT",
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(3),
                        Students = new List<Student> {
                            _students.ElementAt(2),
                            _students.ElementAt(3)
                        }
                    },
                    new Course {
                        ID = 2, 
                        Name = "Computer networking",
                        TemplateID = "T-409-TSAM",
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(3),
                        Students = new List<Student> {
                            _students.ElementAt(0),
                            _students.ElementAt(1)
                        }
                    }
                };

                _courseID = 3;
            }
        }
       

        /// <summary>
        /// Note to self: Returns 200 (OK) as well as the list. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("courses")]
        public List<Course> Courses()
        {
            return _courses;
        }

        /// <summary>
        /// If given the Course ID parameter it is ignored. Adds a course.
        /// </summary>
        /// <param name="newCourse">The course object.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("courses")]
        [ResponseType(typeof(Course))]
        public IHttpActionResult AddCourse(Course newCourse)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.PreconditionFailed);
            }

            var course = new Course
            {
                ID = _courseID,
                TemplateID = newCourse.TemplateID,
                Name = newCourse.Name,
                StartDate = newCourse.StartDate,
                EndDate = newCourse.EndDate,
                Students = new List<Student>()
            };

            _courses.Add(course);
            _courseID++; 

            //We utilize the name in the route attribute of the function
            var location = Url.Link("GetCourseByID", new { ID = course.ID }); 
            return Created(location, course);
        }
        /// <summary>
        /// Gets a course.
        /// </summary>
        /// <param name="ID">The ID of the course to get.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("courses/{ID:int}", Name="GetCourseByID")]
        [ResponseType(typeof(Course))]
        public IHttpActionResult GetCourse(int ID)
        {
            Course retCourse = _courses.Where(c => c.ID == ID).SingleOrDefault(); 
            if (retCourse == null)
            {
                //throw new HttpResponseException(HttpStatusCode.NotFound);
                return NotFound();
            }

            return Ok(retCourse); 
        }

        /// <summary>
        /// Updates a course. 
        /// </summary>
        /// <param name="ID">ID of the course to update.</param>
        /// <param name="updatedCourse">A course object. The ID is ignored.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("courses/{ID:int}")]
        public HttpResponseMessage UpdateCourse(int ID, Course updatedCourse)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.PreconditionFailed);
            }

            foreach (Course course in _courses)
            {
                if (course.ID == ID)
                {
                    course.TemplateID = updatedCourse.TemplateID;
                    course.Name = updatedCourse.Name;
                    course.StartDate = updatedCourse.StartDate;
                    course.EndDate = updatedCourse.EndDate;
                    return new HttpResponseMessage(HttpStatusCode.OK); //Could also make the func return IHttp and return StatusCode
                    
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Note to self: Could also just return void. NoContent is the default reply. 
        /// </summary>
        /// <param name="ID">ID of the course to delete.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("courses/{ID:int}")]
        public HttpResponseMessage DeleteCourse(int ID) 
        {
            int index = 0; 
            foreach (Course course in _courses) 
            {
                if (course.ID == ID) {
                    _courses.RemoveAt(index);
                    return new HttpResponseMessage(HttpStatusCode.NoContent); //204, could also return IHttp... and return StatusCode(..)
                }

                index++; 
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Gets a list of students for the given course.
        /// </summary>
        /// <param name="courseID">The ID of the course for which to get the students..</param>
        /// <returns></returns>
        [HttpGet]
        [Route("courses/{courseID:int}/students")]
        public List<Student> GetStudents(int courseID)
        {
            Course course = _courses.Where(c => c.ID == courseID).SingleOrDefault();
            if (course == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return course.Students;
        }

        /// <summary>
        /// Adds a student to the course.
        /// </summary>
        /// <param name="courseID">ID of the course to which the student should be added.</param>
        /// <param name="student">The object for the student being created.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("courses/{courseID:int}/students")]
        public HttpResponseMessage AddStudent(int courseID, Student student)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.PreconditionFailed);
            }

            _students.Add(student);

            foreach (Course course in _courses)
            {
                if (courseID == course.ID)
                {
                    course.Students.Add(student);
                    return new HttpResponseMessage(HttpStatusCode.Created); //Could also return IHttp... and return StatusCode(..) or Created() BUT since there is no way for the API to access a student directly we would not use Created(). 
                }
            }

            _students.RemoveAt(_students.Count() - 1); //Course not found.. so.. we'll delete the student.. 

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}
