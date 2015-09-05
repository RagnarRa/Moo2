using API.Models;
using API.Models.DTO;
using API.Models.ViewModels;
using API.Services.Entities;
using API.Services.Exceptions;
using API.Services.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Services
{
    /// <summary>
    /// Used for business logic/database related operations related to courses. 
    /// </summary>
    public class CoursesServiceProvider
    {
        private readonly AppDataContext _context;

        public CoursesServiceProvider()
        {
            _context = new AppDataContext();
        }
        /// <summary>
        /// Returns a list of courses belonging to a given semester. 
        /// If no semester is provided, the "current" semester will be used. 
        /// </summary>
        /// <param name="semester">The semester. Example: 20153</param>
        /// <returns>List of courses if there are courses for the semester. Otherwise null.</returns>
        public List<CourseDTO> GetCoursesBySemester(string semester = null)
        {
            if (string.IsNullOrEmpty(semester))
            {
                semester = "20153";
            }

            var result = (from course in _context.Courses
                          join courseTemplate in _context.CourseTemplates
                          on course.TemplateID equals courseTemplate.TemplateID
                          where course.Semester == semester
                          select new CourseDTO
                          {
                              ID = course.ID,
                              StartDate = course.StartDate,
                              Semester = course.Semester,
                              Name = courseTemplate.Name,
                              MaxStudents = course.MaxStudents,
                              StudentCount = (from se in _context.StudentEnrollment
                                              where se.CourseID == course.ID
                                              select se).Count() 
                          }).ToList();

            return result;
        }

        /// <summary>
        /// Gets the course with the given ID.
        /// </summary>
        /// <param name="ID">The ID of the course to get.</param>
        /// <returns>Returns null if the answer is empty. Otherwise the given course.</returns>
        public CourseDetailsDTO GetCourseByID(int ID)
        {
            CourseDetailsDTO retCourse = (from c in _context.Courses
                                   join ct in _context.CourseTemplates
                                   on c.TemplateID equals ct.TemplateID
                                   where c.ID == ID
                                   select new CourseDetailsDTO
                                   {
                                       ID = c.ID,
                                       Name = ct.Name,
                                       Semester = c.Semester,
                                       StartDate = c.StartDate,
                                       EndDate = c.EndDate,
                                       TemplateID = c.TemplateID,
                                       MaxStudents = c.MaxStudents
                                   }).SingleOrDefault();

            if (retCourse == null)
            {
                return null;
            }

            List<StudentDTO> studentList = (from s in _context.Students
                                            join se in _context.StudentEnrollment
                                            on s.ID equals se.StudentID
                                            where se.CourseID == retCourse.ID
                                            select new StudentDTO
                                            {
                                                ID = s.ID,
                                                SSN = s.SSN,
                                                Name = s.Name
                                            }).ToList();

            retCourse.Students = studentList;

            return retCourse;
        }

        /// <summary>
        /// Updates the given course. 
        /// </summary>
        /// <exception cref="AppObjectNotFoundException">Thrown if we can't find the course.</exception>
        /// <param name="ID">The ID of the course to update.</param>
        /// <param name="courseVM">The course update viewmodel.</param>
        /// <returns>Returns a course DTO for the updated course.</returns>
        public CourseDTO UpdateCourse(int ID, CourseUpdateViewModel courseVM)
        {
            Course courseToUpdate = _context.Courses.Where(c => c.ID == ID).SingleOrDefault();

            if (courseToUpdate == null)
                throw new AppObjectNotFoundException();

            courseToUpdate.StartDate = courseVM.StartDate;
            courseToUpdate.EndDate = courseVM.EndDate;
            courseToUpdate.MaxStudents = courseVM.MaxStudents;
            _context.SaveChanges();

            //Create the return object
            string courseName = _context.CourseTemplates.Where(c => c.TemplateID == courseToUpdate.TemplateID).Select(c => c.Name).SingleOrDefault();

            CourseDTO courseDTO = new CourseDTO
            {
                ID = courseToUpdate.ID,
                Name = courseName,
                MaxStudents = courseToUpdate.MaxStudents,
                Semester = courseToUpdate.Semester,
                StartDate = courseToUpdate.StartDate,
                StudentCount = _context.StudentEnrollment.Count(se => se.CourseID == courseToUpdate.ID)
            };

            return courseDTO;
        }

        /// <summary>
        /// Deletes the course. 
        /// </summary>
        /// <exception cref="AppObjectNotFoundException">Thrown if we can't find the course.</exception>
        /// <param name="ID">The ID of the course.</param>
        public void DeleteCourse(int ID)
        {
             Course courseToRemove = _context.Courses.Where(c => c.ID == ID).SingleOrDefault();

             if (courseToRemove == null)
             {
                 throw new AppObjectNotFoundException();
             }

             _context.Courses.Remove(courseToRemove);
             _context.SaveChanges();
        }

        /// <summary>
        /// Gets the list of students for this course.
        /// </summary>
        /// <exception cref="AppObjectNotFoundException">hrown if we can't find the given course.</exception>
        /// <param name="courseID">The ID of the course.</param>
        /// <returns>List of students for the course. Empty list if course has no students.</returns>
        public List<StudentDTO> GetStudentsByCourse(int courseID)
        {
            //See if course exists
            Course course = _context.Courses.Where(c => c.ID == courseID).SingleOrDefault();

            if (course == null)
            {
                throw new AppObjectNotFoundException();
            }

            List<StudentDTO> students = (from st in _context.Students
                                         join se in _context.StudentEnrollment
                                         on st.ID equals se.StudentID
                                         where se.CourseID == courseID
                                         && se.IsDeleted == false 
                                         select new StudentDTO
                                         {
                                             ID = st.ID,
                                             Name = st.Name,
                                             SSN = st.SSN
                                         }).ToList();

            return students;
        }

        /// <summary>
        /// Adds a student to a course. Assumes the student already exists.
        /// Will not allow more than MaxStudents students. 
        /// </summary>
        /// <exception cref="AppObjectNotFoundException">Thrown if either the course or the student doesn't exist.</exception>
        /// <param name="courseID">The ID of the course.</param>
        /// <param name="SSN">The SSN of the student.</param>
        /// <returns>A student object.</returns>
        public StudentDTO AddStudentToCourse(int courseID, string SSN) 
        {
            //Does the course exist?
            Course course = _context.Courses.Where(c => c.ID == courseID).SingleOrDefault();
            if (course == null)
            {
                throw new AppObjectNotFoundException();
            }

            Student student = _context.Students.Where(s => s.SSN == SSN).SingleOrDefault();

            if (student == null)
            {
                throw new AppObjectNotFoundException();
            }

            //Verify that we won't violate the MaxStudents property
            int numStudents = _context.StudentEnrollment.Count(se => se.CourseID == courseID && se.IsDeleted == false && se.IsOnWaitingList == false);

            if (numStudents == course.MaxStudents)
            {
                throw new FullCourseException();
            }

            StudentEnrollment studentEnrollment = new StudentEnrollment { StudentID = student.ID, CourseID = courseID, IsOnWaitingList = false, IsDeleted = false };
            
            //Verify that the user isn't already registered.. 
            if (_context.StudentEnrollment.SingleOrDefault(se => se.StudentID == student.ID && se.CourseID == courseID && se.IsDeleted == false && se.IsOnWaitingList == false) != null)
            {
                throw new DuplicateCourseRegistrationException();
            }

            //If the user is already on a waiting list, we remove it from the waiting list..
            StudentEnrollment enrollment = _context.StudentEnrollment.SingleOrDefault(se => se.StudentID == student.ID && se.CourseID == courseID && se.IsOnWaitingList == true);

            if (!(enrollment == null))
            {
                _context.StudentEnrollment.Remove(enrollment);
            }

            //If the user has already registered on the course but the enrollment has been deleted.. we simply "undelete" it..
            enrollment = _context.StudentEnrollment.SingleOrDefault(se => se.CourseID == courseID && se.StudentID == student.ID && se.IsDeleted == true);

            if (enrollment != null)
            {
                enrollment.IsDeleted = false;
            }
            else
            {
                _context.StudentEnrollment.Add(studentEnrollment);
            }

            _context.SaveChanges();
            return new StudentDTO { ID = student.ID, Name = student.Name, SSN = student.SSN };
        }

        /// <summary>
        /// Adds a course. 
        /// </summary>
        /// <exception cref="AppObjectNotFoundException">Thrown if we can't find a course with the given templateID.</exception>
        /// <param name="courseVM">The course view model.</param>
        /// <returns>Returns the course DTO.</returns>
        public CourseDTO AddCourse(CourseViewModel courseVM)
        {
            //Find the name for the course
            string courseName = _context.CourseTemplates.Where(ct => ct.TemplateID == courseVM.TemplateID).Select(ct => ct.Name).SingleOrDefault();

            if (string.IsNullOrEmpty(courseName))
            {
                throw new AppObjectNotFoundException();
            }

            Course course = new Course { TemplateID = courseVM.TemplateID, StartDate = courseVM.StartDate, EndDate = courseVM.EndDate, Semester = courseVM.Semester, MaxStudents = courseVM.MaxStudents };
            _context.Courses.Add(course);
            _context.SaveChanges();

            //EF is brilliant.. adds the ID to the Course.. 
            return new CourseDTO { ID = course.ID, Name = courseName, Semester = course.Semester, StartDate = course.StartDate, MaxStudents = course.MaxStudents };
        }

        /// <summary>
        /// Gets the waiting list for the given course.
        /// </summary>
        /// <exception cref="AppObjectNotFoundException">Thrown if the waiting list is not found.</exception>
        /// <param name="courseID"></param>
        /// <returns></returns>
        public List<StudentDTO> GetWaitingListByCourseID(int courseID)
        {
            List<StudentDTO> waitingList = (from s in _context.Students
                                            join se in _context.StudentEnrollment
                                            on s.ID equals se.StudentID
                                            where se.IsOnWaitingList == true
                                            select new StudentDTO
                                            {
                                                ID = s.ID,
                                                Name = s.Name,
                                                SSN = s.SSN,
                                            }).ToList();

            if (waitingList == null)
            {
                throw new AppObjectNotFoundException();
            }

            return waitingList;
        }

        /// <summary>
        /// Adds a student to the waiting list of a course. 
        /// </summary>
        /// <exception cref="AppObjectNotFoundException">Thrown if the course is not found.</exception>
        /// <exception cref="DuplicateCourseRegistrationException">Thrown if the student is already enrolled to the course or to the waiting list.</exception>
        /// <param name="courseID">ID of the course.</param>
        /// <param name="studentID">ID of the student.</param>
        public void AddStudentToWaitingList(int courseID, StudentViewModel student) 
        {
            //Check if course exists
            Course course = _context.Courses.SingleOrDefault(c => c.ID == courseID);

            if (course == null)
            {
                throw new AppObjectNotFoundException();
            }

            int studentID = _context.Students.Where(s => s.SSN == student.SSN).Select(s => s.ID).SingleOrDefault();
            
            if (studentID == 0)
            {
                throw new AppObjectNotFoundException();
            }

            //Check to see if the user is already on the waiting list (or registered)
            StudentEnrollment enrollment = _context.StudentEnrollment.SingleOrDefault(se => se.StudentID == studentID && se.CourseID == courseID && se.IsDeleted == false);

            if (enrollment != null)
            {
                throw new DuplicateCourseRegistrationException();
            }

            //Check if the user was deleted before from the course.. if so, we undelete him and put ont he waitin glist
            enrollment = _context.StudentEnrollment.SingleOrDefault(se => se.StudentID == studentID && se.CourseID == courseID && se.IsDeleted == true);

            if (enrollment != null)
            {
                enrollment.IsDeleted = false;
                enrollment.IsOnWaitingList = true;
            }

            else
            {
                enrollment  = new StudentEnrollment
                {
                    CourseID = courseID,
                    StudentID = studentID,
                    IsOnWaitingList = true,
                };

                _context.StudentEnrollment.Add(enrollment);
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Removes a student from a course.
        /// </summary>
        /// <exception cref="AppObjectNotFoundException">Thrown when either the student or the enrollment is not found.</exception>
        /// <param name="courseID">The ID of the course.</param>
        /// <param name="SSN">The SSN of the student.</param>
        public void RemoveStudentFromCourse(int courseID, string SSN)
        {
            //Find the enrollment
            Student student = _context.Students.SingleOrDefault(s => s.SSN == SSN);

            if (student == null)
            {
                throw new AppObjectNotFoundException();
            }

            StudentEnrollment studentEnrollment = _context.StudentEnrollment.SingleOrDefault(se => se.StudentID == student.ID && se.CourseID == courseID);

            if (studentEnrollment == null)
            {
                throw new AppObjectNotFoundException();
            }

            studentEnrollment.IsDeleted = true;
            _context.SaveChanges();
        }
    }
}
