﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from cl in db.Classes
                        join en in db.Enrolleds on cl.ClassId equals en.ClassId into e
                        from j1 in e.DefaultIfEmpty()
                        where j1.UId == uid

                        select new
                        {
                            subject = cl.CIdNavigation.Subject,
                            number = cl.CIdNavigation.Number,
                            name = cl.CIdNavigation.Name,
                            season = cl.SemSeason,
                            year = cl.SemYear,
                            grade = j1 == null ? "--" : (string?)j1.Grade

                        };
                    
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query1 = from a in db.Assignments
                         join cl in db.Classes on a.Ac.ClassId equals cl.ClassId into acl
                         from j1 in acl
                         where j1.CIdNavigation.Subject == subject && j1.CIdNavigation.Number == num &&
                         j1.SemSeason == season && j1.SemYear == year
                         select new
                         {
                             AssignName = a.Name,
                             Categoryname = a.Ac.Name,
                             aID = a.AId,
                             AssignDue = a.Due
                         };

            var query2 = from q in query1
                         join s in db.Submissions on new { A = q.aID, B = uid } equals new { A = s.AId, B = s.UId } into joined
                         from j in joined.DefaultIfEmpty()
                         select new
                         {
                             aname = q.AssignName,
                             cname = q.Categoryname,
                             due = q.AssignDue,
                             score = j == null ? null : (uint?)j.Score
                         };
                    
            return Json(query2.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var query1 = from co in db.Classes
                         join a in db.AssignmentCategories on co.ClassId equals a.ClassId into coa
                         from j1 in coa
                         where co.CIdNavigation.Subject == subject && co.CIdNavigation.Number == num &&
                         co.SemSeason == season && co.SemYear == year && j1.Name == category
                         select new
                         {
                             course_subject = co.CIdNavigation.Subject,
                             course_num = co.CIdNavigation.Number,
                             cls_season = co.SemSeason,
                             cls_year = co.SemYear,
                             a_category = j1.Name,
                             acid = j1.AcId
                         };
            var query2 = from q in query1
                         join aa in db.Assignments on q.acid equals aa.AcId into aaq
                         from j2 in aaq
                         where j2.Name == asgname
                         select new
                         {
                             aid = j2.AId,
                         };
            var query3 = from s in db.Submissions
                         where s.UId == uid && s.AIdNavigation.Name == asgname
                         select s;
            if (query2.Count() > 0)
            {
                try
                {
                    if (query3.Count() > 0)
                        db.Submissions.RemoveRange(query3);

                    Submission submission = new Submission();
                    submission.Time = DateTime.Now;
                    submission.Score = 0;
                    submission.UId = uid;
                    submission.Contents = contents;
                    submission.AId = query2.FirstOrDefault().aid;
                   


                    db.Submissions.Add(submission);
                    db.SaveChanges();

                }
                catch 
                {
                    return Json(new { success = false });
                }
            }
           
            
            return Json(new { success = true });
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var query = from e in db.Classes
                        where e.CIdNavigation.Subject == subject && e.CIdNavigation.Number == num && e.SemSeason == season && e.SemYear == year
                        select new { e.ClassId };
            var query1 = from q in query
                         join en in db.Enrolleds on new { A = q.ClassId, B = uid } equals new { A = en.ClassId, B = en.UId } into q1
                         select q1;
            if (query1.Count() == 0) 
            {
                
                Enrolled enroll = new Enrolled();
                enroll.UId = uid;
                enroll.ClassId = query.SingleOrDefault().ClassId;
                db.Enrolleds.Add(enroll);
                db.SaveChanges();

            }
            return Json(new { success = false});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            return Json(null);
        }
                
        /*******End code to modify********/

    }
}

