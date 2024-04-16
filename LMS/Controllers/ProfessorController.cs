using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Formats.Asn1.AsnWriter;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
         

            var q = from C in db.Classes
                    join E in db.Enrolleds on C.ClassId equals E.ClassId
                    where (C.SemSeason == season && C.SemYear == year) &&
                          (C.CIdNavigation.Subject == subject && C.CIdNavigation.Number == num)
                    select new
                    {
                        fname = E.UIdNavigation.FName,
                        lname = E.UIdNavigation.LName,
                        uid = E.UIdNavigation.UId,
                        dob = E.UIdNavigation.Dob,
                        grade = E.Grade
                    };
            return Json(q.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var query = from C in db.Classes
                        where (C.SemSeason == season && C.SemYear == year) &&
                              (C.CIdNavigation.Subject == subject && C.CIdNavigation.Number == num)
                        select C.ClassId;

            IQueryable<object> assignmentsQuery;

            if (category is null)
            {
                assignmentsQuery = from AC in db.AssignmentCategories
                             join A in db.Assignments on AC.AcId equals A.AcId

                             where query.Contains(AC.ClassId)
                             select new
                             {
                                 aname = A.Name,
                                 cname = AC.Name,
                                 due = A.Due,
                                 submissions = A.Submissions.Count()
                             };
            }
            else
            {
                assignmentsQuery = from AC in db.AssignmentCategories
                                   join A in db.Assignments on AC.AcId equals A.AcId

                                   where query.Contains(AC.ClassId) && AC.Name == category
                                   select new
                                   {
                                       aname = A.Name,
                                       cname = AC.Name,
                                       due = A.Due,
                                       submissions = A.Submissions.Count()
                                   };
            }


            return Json(assignmentsQuery.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from C in db.Classes
                        join AC in db.AssignmentCategories on C.ClassId equals AC.ClassId
                        where (C.SemSeason == season && C.SemYear == year) &&
                              (C.CIdNavigation.Subject == subject && C.CIdNavigation.Number == num)
                        select new
                        {
                            name = AC.Name,
                            weight = AC.Weight
                        };



            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            //check if cat already exist in the class
            var query = from AC in db.AssignmentCategories
                        where (AC.Class.SemSeason==season && AC.Class.SemYear == year) &&
                              (AC.Class.CIdNavigation.Subject == subject && AC.Class.CIdNavigation.Number == num) && AC.Name == category && AC.Weight == catweight
                        select AC.ClassId;

            if (query.Count() > 0)
            {
                return Json(new { success = false });

            }

            //otherwise, get the classId:

            var classIdQuery = from C in db.Classes
                        where (C.SemSeason == season && C.SemYear == year) &&
                              (C.CIdNavigation.Subject == subject && C.CIdNavigation.Number == num)
                        select C.ClassId;

            if (classIdQuery.Count() == 0) 
            {
                System.Diagnostics.Debug.WriteLine("unable to find class, located: CreateAssignmentCategory()");
                return Json(new { success = false });
            }

            try
            {
                AssignmentCategory ac = new AssignmentCategory
                {

                    Name = category,
                    Weight = (byte)catweight,
                    ClassId = classIdQuery.SingleOrDefault()
                };
                db.AssignmentCategories.Add(ac);
                db.SaveChanges();

                return Json(new { success = true });
            }

            catch (Exception ex)
            {
                return Json(new { success = false });

            }

        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {

            var query = from C in db.AssignmentCategories
                        where C.Name == category &
                        C.Class.SemSeason == season
                        &
                        C.Class.SemYear == year
                        &
                        C.Class.CIdNavigation.Subject == subject
                        & C.Class.CIdNavigation.Number == num
                        select new { AcId = C.AcId , ClassId = C.ClassId };

            //check if category exist
            if (query.Count() > 0) {
                Assignment a = new Assignment
                {
                    Name = asgname,
                    Points = (uint)asgpoints,
                    Contents = asgcontents,
                    Due = asgdue,
                    AcId = (uint)query.FirstOrDefault().AcId
                };
                db.Assignments.Add(a);
                db.SaveChanges();

                try
                {
                    var x = query.FirstOrDefault();
                    CalculateGrade(x.ClassId);

                }
                catch 
                {
                    System.Diagnostics.Debug.WriteLine("Fail to calculate grade from CreateAssignment()");
                }

                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });

            }
        }
           


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from s in db.Submissions
                        where s.AIdNavigation.Name == asgname &
                        s.AIdNavigation.Ac.Name == category &
                        s.AIdNavigation.Ac.Class.SemYear == year &
                        s.AIdNavigation.Ac.Class.SemSeason == season &
                        s.AIdNavigation.Ac.Class.CIdNavigation.Subject == subject &
                        s.AIdNavigation.Ac.Class.CIdNavigation.Number == num 
                        select new
                        {
                            fname   = s.UIdNavigation.FName,
                            lname   = s.UIdNavigation.LName,
                            uid     = s.UIdNavigation.UId,
                            time    = s.Time,
                            score   = s.Score
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            try
            {
                var query = from s in db.Submissions
                        where s.UId == uid &
                              s.AIdNavigation.Name == asgname &
                              s.AIdNavigation.Ac.Name == category &
                              s.AIdNavigation.Ac.Class.SemYear == year &
                              s.AIdNavigation.Ac.Class.SemSeason == season &
                              s.AIdNavigation.Ac.Class.CIdNavigation.Subject == subject &
                              s.AIdNavigation.Ac.Class.CIdNavigation.Number == num
                        select new { Submission = s, ClassId = s.AIdNavigation.Ac.ClassId };

                var extracted = query.SingleOrDefault();
                if (extracted != null)
                    extracted.Submission.Score = (uint)score;

                db.SaveChanges();


                try
                {
                    
                    CalculateGrade(extracted.ClassId, uid);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Fail to calculate grade from GradeSubmission()");
                }

                return Json(new { success = true });

            }
            catch
            {
                return Json(new { success = false });

            }
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from C in db.Classes
                        where C.UId == uid
                        select new
                        {
                            subject = C.CIdNavigation.Subject,
                            number = C.CIdNavigation.Number,
                            name = C.CIdNavigation.Name,
                            season = C.SemSeason,
                            year = C.SemYear

                        };

            return Json(query.ToArray());
        }
        /// <summary>
        /// Calculate GPA Letter Grade for either 1 student or all students in the specified class.
        /// No submission : 0 points
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="num"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>
        /// <param name="category"></param>
        /// <param name="asgname"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IActionResult CalculateGrade (uint ClassID, string uid = null)
        {

            //get the categories from the class specified in parameters and all students:
            var query = from C in db.Classes
                        where C.ClassId == ClassID
                        select new
                        {
                            enroll = C.Enrolleds,
                            categories = C.AssignmentCategories
                        };

            if (uid is not null)
            {
                var queryStudent = from e in db.Enrolleds
                                   where e.ClassId == ClassID && e.UId == uid
                                   select new { 
                                       
                                       enroll = e,
                                       categories = e.Class.AssignmentCategories 

                                   };

                uint totalScore = 0;
                byte totalWeight = 0;
                foreach (var category in queryStudent.FirstOrDefault().categories)
                {
                    var queryCat = from c in db.AssignmentCategories
                                   where c.AcId == category.AcId && c.Name == category.Name
                                   select c.Assignments;


                    var x = queryCat.SingleOrDefault();
                    foreach (var y in x)
                    {
                        uint score = PointsEarnedOverMaxPoints(category, uid);
                        totalScore += score;
                    }
                    /* if (queryCat.Count() > 0)
                         totalWeight += category.Weight;
                     uint score = PointsEarnedOverMaxPoints(category, uid);
                     totalScore += score;*/
                    totalWeight += category.Weight;

                }
               
                int scalingFactor = 100 / totalWeight;
                double totalPercentage = Double.Round((int)totalScore * scalingFactor, 2);
                string letterGrade = PercentageToLetterGrade(totalPercentage);


                //update letter grade in the database
                queryStudent.FirstOrDefault().enroll.Grade = letterGrade;
                db.SaveChanges();
                return Json(new { success = true });
            }

            //for each student calculate the grade          
            foreach (var enroll in query.FirstOrDefault().enroll)
            {

                uint totalScore = 0;
                byte totalWeight = 0;
                foreach (var category in query.FirstOrDefault().categories)
                {
                    if (category.Assignments.Count() == 0)
                        continue;
                        
                    totalWeight += category.Weight;
                    uint score = PointsEarnedOverMaxPoints(category, enroll.UId);
                    totalScore += score;
                    

                }
                int scalingFactor = 100 / totalWeight;


                double totalPercentage = Double.Round((int)totalScore * scalingFactor, 2);
                string letterGrade = PercentageToLetterGrade(totalPercentage);


                //update letter grade in the database
                enroll.Grade = letterGrade;
                db.SaveChanges();

            }
            return Json(new { success = true });

        }
        /// <summary>
        /// Calculate points earned over total max point in a category, then multiply by cat weight.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        private uint PointsEarnedOverMaxPoints(AssignmentCategory category, string uid )
        {
           
            var queryMaxPoints = (from AC in db.AssignmentCategories
                                  where AC == category
                                  from assignment in AC.Assignments
                                  select (int)assignment.Points).Sum();

            var queryPointsEarned = (from AC in db.AssignmentCategories
                                  where AC == category
                                  from assignment in AC.Assignments
                                  from submission in assignment.Submissions
                                  where submission.UId == uid
                                  select (int)submission.Score).Sum();

            
            return (uint)((queryPointsEarned / queryMaxPoints) * category.Weight);
        }

        private string PercentageToLetterGrade(double percentage)
        {
            switch (percentage)
            {
                case var p when p >= 93 && p <=100:
                    return "A";
                case var p when p >= 90 && p < 93:
                    return "A-";
                case var p when p >= 87 && p < 90:
                    return "B+";
                case var p when p >= 83 && p < 87:
                    return "B";
                case var p when p >= 80 && p < 83:
                    return "B-";
                case var p when p >= 77 && p < 80:
                    return "C+";
                case var p when p >= 73 && p < 77:
                    return "C";
                case var p when p >= 70 && p < 73: 
                    return "C-";
                case var p when p >= 67 && p < 70:
                    return "D+";
                case var p when p >= 63 && p < 67:
                    return "D";
                case var p when p >= 60 && p < 63:
                    return "D-";
                default:
                    return "E";
            }
        }
        /*******End code to modify********/
    }
}

