using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {
            try
            {
                Department dp = new Department();
                dp.Name = name;
                dp.Subject = subject;

                db.Departments.Add(dp);
                db.SaveChanges();
            }
            catch (Exception ex) 
            {
                return Json(new { success = false });

            }
            return Json(new { success = true});
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query = from d in db.Departments
                        join c in db.Courses on d.Subject equals c.Subject into co
                        from j1 in co
                        where j1.Subject == subject
                        select new
                        {
                            number = j1.Number,
                            name = j1.Name
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query = from d in db.Departments
                        join p in db.Professors on d.Subject equals p.Subject into prof
                        from j1 in prof
                        where j1.Subject == subject
                        select new
                        {
                            lname = j1.LName,
                            fname = j1.FName,
                            uid = j1.UId
                        };
            return Json(query.ToArray());
            
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            
                try
                {
                    Course course = new Course();
                    course.Subject = subject;
                    course.Number = (ushort)number;
                    course.Name = name;
                    db.Courses.Add(course);
                    db.SaveChanges();

                }
                catch
                {
                    return Json(new { success = false });
                }
            
            
            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {
            System.Diagnostics.Debug.WriteLine("start to create classs");
            
            var query1 = from c in db.Classes
                         where (c.SemSeason == season && c.SemYear == year) && 
                         (
                         (c.Loc == location && 
                         start.Hour == c.Start.Hour && start.Minute>=c.Start.Minute & end.Hour == c.End.Hour && end.Minute <=c.End.Minute) 
                         ||                       
                         (c.CIdNavigation.Subject == subject && c.CIdNavigation.Number == number)
                         )
                         select c;
            System.Diagnostics.Debug.WriteLine("query: ", Json(query1.ToArray()));

            if (query1 == null | query1.Count()==0)
            {
                try
                {
                    Class cls = new Class();
                    cls.SemSeason = season;
                    cls.SemYear = (uint)year;
                    cls.Loc = location;
                    cls.UId = instructor;

                    var query2 = from c in db.Courses
                                 where c.Subject == subject && c.Number == number
                                 select c.CId;
                    if (query2 == null)
                        return Json(new { success = false });
                    else 
                        cls.CId =(uint) query2.SingleOrDefault();
                    cls.Start = TimeOnly.FromDateTime(start);
                    cls.End = TimeOnly.FromDateTime(end);
                    db.Classes.Add(cls);
                    db.SaveChanges();
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("fail to create class");

                    return Json(new { success = false });
                }

                System.Diagnostics.Debug.WriteLine("succesfully created");

                return Json(new { success = true });

            }
            System.Diagnostics.Debug.WriteLine("unable to create class");

            return Json(new { success = false});
        }


        /*******End code to modify********/

    }
}

