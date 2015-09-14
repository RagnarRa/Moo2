using API.Models;
using API.Models.DTO;
using API.Models.ViewModels;
using API.Services.Entities;
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
    /// TODO: Í stað þess að skila null.. kasta custom exceptions og grípa í controller.. 
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
                                       TemplateID = c.TemplateID
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
        /// <param name="ID">The ID of the course to update.</param>
        /// <param name="courseVM">The course update viewmodel.</param>
        /// <returns>Returns true if the update was successful, otherwise false (if not found).</returns>
        public bool UpdateCourse(int ID, CourseUpdateViewModel courseVM)
        {
            Course courseToUpdate = _context.Courses.Where(c => c.ID == ID).SingleOrDefault();

            if (courseToUpdate == null)
                return false; 

            courseToUpdate.StartDate = courseVM.StartDate;
            courseToUpdate.EndDate = courseVM.EndDate;
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Deletes the course. 
        /// </summary>
        /// <param name="ID">The ID of the course.</param>
        /// <returns>True if successful, false if course wasn't found.</returns>
        public bool DeleteCourse(int ID)
        {
             Course courseToRemove = _context.Courses.Where(c => c.ID == ID).SingleOrDefault();

             if (courseToRemove == null)
             {
                 return false;
             }

             _context.Courses.Remove(courseToRemove);
             _context.SaveChanges();
             return true;
        }

        /// <summary>
        /// Gets the list of students for this course.
        /// </summary>
        /// <param name="courseID">The ID of the course.</param>
        /// <returns>List of students for the course. Empty list if course has no students. Null if course was not found.</returns>
        public List<StudentDTO> GetStudentsByCourse(int courseID)
        {
            //See if course exists
            Course course = _context.Courses.Where(c => c.ID == courseID).SingleOrDefault();

            if (course == null)
            {
                return null;
            }

            List<StudentDTO> students = (from st in _context.Students
                                         join se in _context.StudentEnrollment
                                         on st.ID equals se.StudentID
                                         where se.CourseID == courseID
                                         select new StudentDTO
                                         {
                                             ID = st.ID,
                                             Name = st.Name,
                                             SSN = st.SSN
                                         }).ToList();

            return students;
        }

        /// <summary>
        /// Adds a student to a course. If the student doesn't exist, it is created. 
        /// </summary>
        /// <param name="courseID">The ID of the course.</param>
        /// <param name="studentVM">The student VM.</param>
        /// <returns>True if successful, false if the course does not exist.</returns>
        public bool AddStudentToCourse(int courseID, StudentViewModel studentVM) 
        {
            //Does the course exist?
            Course course = _context.Courses.Where(c => c.ID == courseID).SingleOrDefault();
            if (course == null)
            {
                return false;
            }

            Student student = _context.Students.Where(s => s.SSN == studentVM.SSN).SingleOrDefault();

            if (student == null)
            {
                student = new Student { Name = studentVM.Name, SSN = studentVM.SSN };
                _context.Students.Add(student);
                _context.SaveChanges();
            }

            StudentEnrollment studentEnrollment = new StudentEnrollment { StudentID = student.ID, CourseID = courseID };
            _context.StudentEnrollment.Add(studentEnrollment);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Adds a course. 
        /// </summary>
        /// <param name="courseVM">The course view model.</param>
        /// <returns>Returns the course DTO. Or null if there's no matching course template.</returns>
        public CourseDTO AddCourse(CourseViewModel courseVM)
        {
            //Find the name for the course
            string courseName = (from c in _context.Courses 
                                 join ct in _context.CourseTemplates
                                 on c.TemplateID equals ct.TemplateID
                                 where ct.TemplateID == courseVM.TemplateID
                                 select ct.Name).FirstOrDefault();

            if (string.IsNullOrEmpty(courseName))
            {
                return null; //If there is no course with the given templateID, we return null..
            }

            Course course = new Course { TemplateID = courseVM.TemplateID, StartDate = courseVM.StartDate, EndDate = courseVM.EndDate, Semester = courseVM.Semester };
            _context.Courses.Add(course);
            _context.SaveChanges();

            //EF is brilliant.. adds the ID to the Course.. 
            return new CourseDTO { ID = course.ID, Name = courseName, Semester = course.Semester, StartDate = course.StartDate };
        }
    }
}
