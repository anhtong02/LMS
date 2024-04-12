using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {            

            var query = from D in db.Departments
                        select new {name = D.Name,
                                    subject = D.Subject};

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query = from d in db.Departments
                        select new
                        {
                            subject = d.Subject,
                            dname = d.Name,
                            courses = from c in d.Courses
                                      select new { number = c.Number, cname = c.Name }
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var query = from c in db.Classes
                        where c.CIdNavigation.Subject == subject && c.CIdNavigation.Number == number
                        select new
                        {
                            season = c.SemSeason,
                            year = c.SemYear,
                            location = c.Loc,
                            start = c.Start.ToString("hh:mm:ss"),
                            end = c.End.ToString("hh:mm:ss"),
                            fname = c.UIdNavigation.FName,
                            lname = c.UIdNavigation.LName
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from ac in db.AssignmentCategories
                        where ac.Class.CIdNavigation.Subject == subject &&
                        ac.Class.CIdNavigation.Number == num && ac.Class.SemSeason == season &&
                        ac.Class.SemYear == year && ac.Name == category
                        join a in db.Assignments on ac.AcId equals a.AcId into aac
                        from j1 in aac
                        where j1.Name == asgname
                        select new { j1.Contents };
            var content = query.FirstOrDefault().Contents;
            return Content(content, "text/plain");
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            var query1 = from ac in db.AssignmentCategories
                         where ac.Class.CIdNavigation.Subject == subject && ac.Class.CIdNavigation.Number == num &&
                         ac.Class.SemSeason == season && ac.Class.SemYear == year && ac.Name == category
                         join a in db.Assignments on ac.AcId equals a.AcId into aac
                         from j1 in aac
                         where j1.Name == asgname
                         select new { j1.AId };

            var query2 = from q in query1
                         join s in db.Submissions on new { a = q.AId, b = uid } equals new { a = s.AId, b = s.UId } into qs
                         from j2 in qs
                         select new { j2.Contents };
            System.Diagnostics.Debug.WriteLine("query2 : ", Json(query2.ToArray()));

            try
            {
                if (query2.Count() > 0)
                {
                    return Content(query2.SingleOrDefault().Contents, "text/plain");

                }
                else if (query2.Count() == 0)
                {
                    return Content("");
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Unable to return content");
            }
            return Content("");
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var query1 = (from s in db.Students
                        where s.UId == uid
                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            department = s.SubjectNavigation.Name
                        }).FirstOrDefault();
            var query2 = (from p in db.Professors
                          where p.UId == uid
                          select new
                          {
                              fname = p.FName,
                              lname = p.LName,
                              uid = p.UId,
                              Department = p.SubjectNavigation.Name
                          }).FirstOrDefault();
            var query3 = (from a in db.Administrators
                          where a.UId == uid
                          select new
                          {
                              fname = a.FName,
                              lname = a.LName,
                              uid = a.UId
                          }).FirstOrDefault();

            System.Diagnostics.Debug.WriteLine("query1 :", Json(query1));
            if (query1 != null)
            {
                return Json(query1);
            }
            else if (query2 != null)
            {
                return Json(query2);
            }
            else
            {
                return Json(query3);
            }
            
        }


        /*******End code to modify********/
    }
}

