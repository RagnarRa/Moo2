using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;

namespace CoursesAPI.Services.Services
{
	public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
		}

		/// <summary>
		/// You should implement this function, such that all tests will pass.
		/// </summary>
		/// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
		/// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
		/// <returns>Should return basic information about the person.</returns>
		public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
            var course = _courseInstances.All().SingleOrDefault(c => c.ID == courseInstanceID);

            if (course == null)
            {
                throw new AppObjectNotFoundException();
            }

            var teacher = _persons.All().SingleOrDefault(t => t.SSN == model.SSN);

            if (teacher == null)
            {
                throw new AppObjectNotFoundException();
            }


            var isCurrentTeacher = _teacherRegistrations.All().SingleOrDefault(tr => tr.CourseInstanceID == courseInstanceID && tr.SSN == model.SSN);

            if (isCurrentTeacher != null)
            {
                throw new AppValidationException("PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE");
            }

            var currentMainTeacherOfCourse = _teacherRegistrations.All().SingleOrDefault(tr => tr.CourseInstanceID == courseInstanceID && tr.Type == TeacherType.MainTeacher);

            if (currentMainTeacherOfCourse != null)
            {
                throw new AppValidationException("COURSE_ALREADY_HAS_A_MAIN_TEACHER");
            }

            TeacherRegistration teacherRegistration = new TeacherRegistration
            {
                SSN = model.SSN,
                Type = model.Type,
                CourseInstanceID = courseInstanceID
            };

            _teacherRegistrations.Add(teacherRegistration);
            _uow.Save();
            return new PersonDTO { Name = teacher.Name, SSN = teacher.SSN };
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public List<CourseInstanceDTO> GetCourseInstancesBySemester(string semester = null)
		{
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}

            //LHS
            var coursesLeftQuery = (from c in _courseInstances.All()
                                   join ct in _courseTemplates.All()
                                   on c.CourseID equals ct.CourseID
                                   where c.SemesterID == semester
                                   select new CourseInstanceDTO
                                   {
                                       CourseInstanceID = c.ID,
                                       TemplateID = c.CourseID,
                                       Name = ct.Name
                                   }).ToList(); 

            //RHS, all main teachers.. 
            var teachersRightQuery = (from tr in _teacherRegistrations.All()
                                     join p in _persons.All() 
                                     on tr.SSN equals p.SSN
                                     where tr.Type == TeacherType.MainTeacher
                                     select new  //Anon object
                                     {
                                         ID = p.ID,
                                         SSN = tr.SSN,
                                         Name = p.Name,
                                         Email = p.Email,
                                         CourseInstanceID = tr.CourseInstanceID //til að join-a á niðri
                                     }).ToList();
            
            var query = (from course in coursesLeftQuery
                         join teacher in teachersRightQuery
                         on course.CourseInstanceID equals teacher.CourseInstanceID into gj
                         from subcourse in gj.DefaultIfEmpty()
                         select new CourseInstanceDTO
                         {
                             CourseInstanceID = course.CourseInstanceID,
                             Name = course.Name,
                             TemplateID = course.TemplateID,
                             MainTeacher = (subcourse == null ? string.Empty : subcourse.Name)
                         }).ToList();
			return query;
		}
	}
}
