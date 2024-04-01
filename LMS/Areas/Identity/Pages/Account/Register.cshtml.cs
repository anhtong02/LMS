// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace LMS.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        //private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly LMSContext db;
        //private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            LMSContext _db
            /*IEmailSender emailSender*/)
        {
            _userManager = userManager;
            _userStore = userStore;
            //_emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            db = _db;
            //_emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required]
            [Display( Name = "Role" )]
            public string Role { get; set; }

            public List<SelectListItem> Roles { get; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "Student", Text = "Student" },
                new SelectListItem { Value = "Professor", Text = "Professor" },
                new SelectListItem { Value = "Administrator", Text = "Administrator"  }
            };

            public string Department { get; set; }

            public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>
            {
                new SelectListItem{Value = "None", Text = "NONE"}
            };

            [Required]
            [Display( Name = "First Name" )]
            public string FirstName { get; set; }

            [Required]
            [Display( Name = "Last Name" )]
            public string LastName { get; set; }

            [Required]
            [Display( Name = "Date of Birth" )]
            [BindProperty, DisplayFormat( DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true )]
            [DataType( DataType.Date )]
            public System.DateTime DOB { get; set; } = DateTime.Now;

            [Required]
            //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType( DataType.Password )]
            [Display( Name = "Password" )]
            public string Password { get; set; }

            [DataType( DataType.Password )]
            [Display( Name = "Confirm password" )]
            [Compare( "Password", ErrorMessage = "The password and confirmation password do not match." )]
            public string ConfirmPassword { get; set; }

        }


        public async Task OnGetAsync( string returnUrl = null )
        {
            ReturnUrl = returnUrl;
            ExternalLogins = ( await _signInManager.GetExternalAuthenticationSchemesAsync() ).ToList();



        }

        public async Task<IActionResult> OnPostAsync( string returnUrl = null )
        {
            returnUrl ??= Url.Content( "~/" );
            ExternalLogins = ( await _signInManager.GetExternalAuthenticationSchemesAsync() ).ToList();
            if ( ModelState.IsValid )
            {
                var uid = CreateNewUser(Input.FirstName, Input.LastName, Input.DOB, Input.Department, Input.Role);
                var user = new ApplicationUser { UserName = uid };

                await _userStore.SetUserNameAsync( user, uid, CancellationToken.None );
                var result = await _userManager.CreateAsync(user, Input.Password);

                if ( result.Succeeded )
                {
                    _logger.LogInformation( "User created a new account with password." );
                    await _userManager.AddToRoleAsync( user, Input.Role );

                    var userId = await _userManager.GetUserIdAsync(user);

                    await _signInManager.SignInAsync( user, isPersistent: false );
                    return LocalRedirect( returnUrl );

                }
                foreach ( var error in result.Errors )
                {
                    ModelState.AddModelError( string.Empty, error.Description );
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException( $"Can't create an instance of '{nameof( ApplicationUser )}'. " +
                    $"Ensure that '{nameof( IdentityUser )}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml" );
            }
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a new user of the LMS with the specified information and add it to the database.
        /// Assigns the user a unique uID consisting of a 'u' followed by 7 digits.
        /// </summary>
        /// <param name="firstName">The user's first name</param>
        /// <param name="lastName">The user's last name</param>
        /// <param name="DOB">The user's date of birth</param>
        /// <param name="departmentAbbrev">The department abbreviation that the user belongs to (ignore for Admins) </param>
        /// <param name="role">The user's role: one of "Administrator", "Professor", "Student"</param>
        /// <returns>The uID of the new user</returns>
         string CreateNewUser( string firstName, string lastName, DateTime DOB, string departmentAbbrev, string role )
        {

            var maxUIDAdministrators = (from ad in db.Administrators
                                        select ad.UId).Max();

            var maxUIDProfessors = (from ad in db.Professors
                                              select ad.UId).Max();

            var maxUIDStudents = (from ad in db.Students
                                            select ad.UId).Max();


            int? maxUID = null;

            if (maxUIDAdministrators != null)
                maxUID = int.Parse(maxUIDAdministrators.Substring(1));
            else if (maxUIDProfessors != null )
            {
                int num = int.Parse(maxUIDProfessors.Substring(1));
                if (maxUID!=null && num > maxUID || maxUID is null)
                {
                    maxUID = num;
                }
              
            }

            else if (maxUIDStudents != null)
            {
                int num = int.Parse(maxUIDStudents.Substring(1));
                if (maxUID != null && num > maxUID || maxUID is null)
                {
                    maxUID = num;
                }
            }
            string nextUID = "";
            if (maxUID is null)
            {
                nextUID = "u0000000";

            }
            else
            {
                int nextMax = (int)maxUID + 1;

                nextUID = "u" + nextMax.ToString().PadLeft(7, '0');
            }
           


            if (role.Equals("Student"))
            {
                try
                {
                    Student st = new Student();
                    st.FName = firstName;
                    st.LName = lastName;
                    st.Dob = DateOnly.FromDateTime(DOB);
                    st.Subject = departmentAbbrev;
                    st.UId = nextUID;

                    db.Students.Add(st);
                    db.SaveChanges();

                }
                catch (Exception ex) 
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            if (role.Equals("Professor"))
            {
                try
                {
                    Professor pf = new Professor();
                    pf.FName = firstName;
                    pf.LName = lastName;
                    pf.Dob = DateOnly.FromDateTime(DOB);
                    pf.Subject = departmentAbbrev;
                    pf.UId = nextUID;
                    db.Professors.Add(pf);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            else
            {
                try
                {
                    Administrator ad = new Administrator();
                    ad.FName = firstName;
                    ad.LName = lastName;
                    ad.Dob = DateOnly.FromDateTime(DOB);
                    ad.UId = nextUID;
                   
                    db.Administrators.Add(ad);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            return nextUID;
        }

        /*******End code to modify********/
    }
}
