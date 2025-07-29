using Microsoft.AspNetCore.Mvc;
using Student_Management.ViewModels;
using Microsoft.AspNetCore.Identity;
using Student_Management.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

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


		[HttpGet]
		public IActionResult Login()
		{
			try
			{
				 
				return View();
			}
			catch (Exception ex)
			{
				
				return View("Error");
			}
		}



		[HttpPost]
		public async Task<IActionResult> Login(LoginViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.Rememberme, false);

					if (result.Succeeded)
					{
						return RedirectToAction("Index", "Home");
					}
					else
					{
						ModelState.AddModelError("", "Email or password is incorrect.");
						return View(model);
					}
				}
				return View(model);
			}
			catch (Exception ex)
			{
				
				ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");

				return View(model);
			}
		}


		[HttpGet]
		public IActionResult Register()
		{
			try
			{
				
				return View();
			}
			catch (Exception ex)
			{
				
				return View("Error", new { message = "An error occurred while loading the Register page." });
			}
		}



		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
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

					var result = await userManager.CreateAsync(user, model.Password);

					if (result.Succeeded)
					{
						if (signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
						{
							return RedirectToAction("ListUsers", "Administration");
						}

						await signInManager.SignInAsync(user, isPersistent: false);
						return RedirectToAction("index", "home");
					}

					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}
				catch (Exception ex)
				{
					
					ModelState.AddModelError(string.Empty, "An unexpected error occurred while processing your request. Please try again later.");
				}
			}

			return View(model);
		}


		[HttpGet]
		public IActionResult VerifyEmail()
		{
			try
			{
				
				return View();
			}
			catch (Exception ex)
			{
				
				Console.WriteLine($"An error occurred: {ex.Message}");

			
				return View("Error");
			}
		}



		[HttpPost]
		public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
		{
			try
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
			catch (Exception)
			{
			
				ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
				return View(model);
			}
		}



		[HttpGet]
		public IActionResult ChangePassword(string username)
		{
			try
			{
				if (string.IsNullOrEmpty(username))
				{
					return RedirectToAction("VerifyEmail", "Account");
				}
				return View(new ChangePasswordViewModel { Email = username });
			}
			catch (Exception ex)
			{
			
				return RedirectToAction("Error", "Home");
			}
		}



		[HttpPost]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var user = await userManager.FindByNameAsync(model.Email);
					if (user != null)
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
					ModelState.AddModelError("", "Something went wrong. Try again.");
					return View(model);
				}
			}
			catch (Exception ex)
			{
				
				ModelState.AddModelError("", "An error occurred while changing the password. Please try again later.");

				return View(model);
			}
		}


		public async Task<IActionResult> Logout()
		{
			try
			{
				await signInManager.SignOutAsync();
				return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				
               return RedirectToAction("Error", "Home");
			}
		}



		[HttpGet]
		[AllowAnonymous]
		public IActionResult AccessDenied()
		{
			try
			{
				return View();
			}
			catch (Exception ex)
			{
			
				return View("Error");
			}
		}



		[Authorize]
		public async Task<IActionResult> Profile()
		{
			try
			{
				var user = await userManager.GetUserAsync(User);
				if (user == null)
				{
					return NotFound();
				}

				var model = new UserProfileViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Grade = user.Grade,
					Gender = user.Gender,
					Address = user.Address,
					DateofBirth = user.DateofBirth,
					PhoneNumber = user.PhoneNumber,
				};

				return View(model);
			}
			catch (Exception ex)
			{
				
				return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
			}
		}

	}
}
