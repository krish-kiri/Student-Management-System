using Microsoft.AspNetCore.Mvc;
using Student_Management.ViewModels;
using Microsoft.AspNetCore.Identity;
using Student_Management.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;

namespace Student_Management.Controllers
{
	public class AccountController : Controller
	{
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly UserManager<ApplicationUser> userManager;

		public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
		}

		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			if (ModelState.IsValid)
			{
				var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.Rememberme, false);
				
				if(result.Succeeded)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					ModelState.AddModelError("", "Email or passwoed is incorrect.");
					return View(model);
				}
			}
			return View(model);
		}

		public IActionResult Register()
		{
			return View();
		}

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
				// Copy data from RegisterViewModel to ApplicationUser
				var user = new ApplicationUser
				{
					UserName = model.Email,
					Email = model.Email,
					Gender = model.Gender,
					FirstName = model.FirstName,
					LastName = model.LastName,
					DateofBirth = model.DateofBirth,
					Address = model.Address,
					PhoneNumber = model.PhoneNumber,
					

				}; 

                // Store user data in AspNetUsers database table
                var result = await userManager.CreateAsync(user, model.Password);

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    // If the user is signed in and in the Admin role, then it is
                    // the Admin user that is creating a new user. 
                    // So redirect the Admin user to ListUsers action of Administration Controller
                    if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }

                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "home");
                }

                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public IActionResult VerifyEmail()
		{
			
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByNameAsync(model.Email);

				if (user == null)
				{
					ModelState.AddModelError("", "Something is wrong!");
					return View(model);
				}
				else
				{
					return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });

				}

			} 
			return View(model);
		}

		public IActionResult ChangePassword(string username)
		{
			if (string.IsNullOrEmpty(username))
			{
				return RedirectToAction("VerifyEmail", "Account");
			}
			return View(new ChangePasswordViewModel { Email = username});
		}

		[HttpPost]
		public async Task<IActionResult>ChangePassword(ChangePasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByNameAsync(model.Email);
				if (user !=null)
				{
					var result = await userManager.RemovePasswordAsync(user);
					if (result.Succeeded)
					{
						 result = await userManager.AddPasswordAsync(user, model.NewPassword);
						return RedirectToAction("Login", "Account");
					}

					else
					{
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}
						return View(model);
					}

				}
				else
				{
					ModelState.AddModelError("", "Email not found!");
					return View(model);
				}

			}
			else
			{
				ModelState.AddModelError("", "Somthing went wrong. try again.");
				return View(model);

			}	
		}

		public async Task<IActionResult>Logout()
		{
			await signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User); if (user == null) { return NotFound(); }
            var model = new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
				LastName = user.LastName,
				Gender = user.Gender,
                Address=user.Address,
				DateofBirth = user.DateofBirth,
				PhoneNumber = user.PhoneNumber,

			

            };

            return View(model);
        }
    }
}
