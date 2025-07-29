using Student_Management.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Management.Data;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;


 

namespace Student_Management.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public AdministrationController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> ListRoles()
        {
            try
            {
                List<ApplicationRole> roles = await _roleManager.Roles.ToListAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while fetching roles: {ex.Message}");

                
                return View("Error", new ErrorViewModel { ErrorMessage = "An error occurred while fetching the roles." });
            }
        }

		[HttpGet]
		public IActionResult CreateRole()
		{
			try
			{
				return View();
			}
			catch (Exception ex)
			{
				
				TempData["error"] = "An unexpected error occurred. Please try again later.";
				return RedirectToAction("Index", "Home"); 
			}
		}


		[HttpPost]
		public async Task<IActionResult> CreateRole(CreateRoleViewModel roleModel)
		{
			if (ModelState.IsValid)
			{
				try
				{
					
					bool roleExists = await _roleManager.RoleExistsAsync(roleModel?.RoleName);
					if (roleExists)
					{
						ModelState.AddModelError("", "Role Already Exists");
					}
					else
					{
						
						ApplicationRole identityRole = new ApplicationRole
						{
							Name = roleModel?.RoleName,
							Description = roleModel?.Description
						};

						IdentityResult result = await _roleManager.CreateAsync(identityRole);

						if (result.Succeeded)
						{
							TempData["success"] = "Role Created Successfully";
							return RedirectToAction("Index", "Home");
						}

						
						foreach (IdentityError error in result.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}
					}
				}
				catch (Exception ex)
				{
					
					ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
				}
			}

			return View(roleModel);
		}


		[HttpGet]
		public async Task<IActionResult> EditRole(string roleId)
		{
			try
			{
				if (string.IsNullOrEmpty(roleId))
				{
					return View("Error");
				}

				ApplicationRole? role = await _roleManager.FindByIdAsync(roleId);
				if (role == null)
				{
					return View("Error");
				}

				var model = new EditRoleViewModel
				{
					Id = role.Id,
					RoleName = role.Name,
					Description = role.Description,
					Users = new List<string>()
				};

				foreach (var user in _userManager.Users.ToList())
				{
					if (await _userManager.IsInRoleAsync(user, role.Name))
					{
						model.Users.Add(user.UserName);
					}
				}

				return View(model);
			}
			catch (Exception ex)
			{
				
				Console.WriteLine($"An error occurred: {ex.Message}");

				return View("Error");
			}
		}




		[HttpPost]
		public async Task<IActionResult> EditRole(EditRoleViewModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var role = await _roleManager.FindByIdAsync(model.Id);
					if (role == null)
					{
						ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
						return View("NotFound");
					}

					role.Name = model.RoleName;
					role.Description = model.Description;

					var result = await _roleManager.UpdateAsync(role);
					if (result.Succeeded)
					{
						TempData["warning"] = "Role Edited Successfully";
						return RedirectToAction("ListRoles");
					}

					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "An error occurred while updating the role.");
					
					Console.WriteLine($"Error: {ex.Message}");
				}
			}

			return View(model);
		}


		[HttpPost]
		public async Task<IActionResult> DeleteRole(string roleId)
		{
			try
			{
				var role = await _roleManager.FindByIdAsync(roleId);
				if (role == null)
				{
					ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
					return View("NotFound");
				}

				var result = await _roleManager.DeleteAsync(role);
				if (result.Succeeded)
				{
					TempData["error"] = "Role Deleted Successfully";
					return RedirectToAction("ListRoles");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

				return View("ListRoles", await _roleManager.Roles.ToListAsync());
			}
			catch (Exception ex)
			{
				
				Console.WriteLine($"Error occurred while deleting role: {ex.Message}");

				
				TempData["error"] = "An unexpected error occurred while deleting the role.";

				return RedirectToAction("ListRoles");
			}
		}


		[HttpGet]
		public async Task<IActionResult> EditUsersInRole(string roleId)
		{
			try
			{
				ViewBag.roleId = roleId;

				var role = await _roleManager.FindByIdAsync(roleId);

				if (role == null)
				{
					ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
					return View("NotFound");
				}

				ViewBag.RollName = role.Name;
				var model = new List<UserRoleViewModel>();

				foreach (var user in _userManager.Users.ToList())
				{
					var userRoleViewModel = new UserRoleViewModel
					{
						Id = user.Id,
						UserName = user.UserName,
						IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
					};

					model.Add(userRoleViewModel);
				}

				return View(model);
			}
			catch (Exception ex)
			{
				
				Console.WriteLine($"An error occurred: {ex.Message}");
				return View("Error");
			}
		}


		[HttpPost]
		public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
		{
			try
			{
				var role = await _roleManager.FindByIdAsync(roleId);

				if (role == null)
				{
					ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
					return View("NotFound");
				}

				for (int i = 0; i < model.Count; i++)
				{
					var user = await _userManager.FindByIdAsync(model[i].Id);

					IdentityResult? result;

					if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
					{
						result = await _userManager.AddToRoleAsync(user, role.Name);
					}
					else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
					{
						result = await _userManager.RemoveFromRoleAsync(user, role.Name);
					}
					else
					{
						continue;
					}

					if (result.Succeeded)
					{
						TempData["warning"] = "User Edited Successfully";

						if (i < (model.Count - 1))
							continue;
						else
							return RedirectToAction("EditRole", new { roleId = roleId });
					}
				}
			}
			catch (Exception ex)
			{
				
				ViewBag.ErrorMessage = $"An error occurred while editing users in the role: {ex.Message}";
				return View("Error");
			}

			return RedirectToAction("EditRole", new { roleId = roleId });
		}


		[HttpGet]
		public IActionResult ListUsers()
		{
			try
			{
				var users = _userManager.Users;
				return View(users);
			}
			catch (Exception ex)
			{
				
				TempData["ErrorMessage"] = "An error occurred while retrieving the users.";
				return RedirectToAction("Error"); 
			}
		}



		[HttpGet]
		public async Task<IActionResult> EditUser(string Id)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(Id);

				if (user == null)
				{
					ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
					return View("NotFound");
				}

				var userClaims = await _userManager.GetClaimsAsync(user);
				var userRoles = await _userManager.GetRolesAsync(user);

				var model = new EditUserViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Grade = user.Grade,
					Email = user.Email,
					Gender = user.Gender,
					PhoneNumber = user.PhoneNumber,
					Address = user.Address,
					DateofBirth = user.DateofBirth,
					Roles = userRoles
				};

				return View(model);
			}
			catch (Exception ex)
			{
			

				ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again later.";
				return View("Error");
			}
		}



		[HttpPost]
		public async Task<IActionResult> EditUser(EditUserViewModel model)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(model.Id);

				if (user == null)
				{
					ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
					return View("NotFound");
				}
				else
				{
					user.Id = model.Id;
					user.UserName = model.UserName;
					user.FirstName = model.FirstName;
					user.LastName = model.LastName;
					user.Grade = model.Grade;
					user.Email = model.Email;
					user.Gender = model.Gender;
					user.PhoneNumber = model.PhoneNumber;
					user.Address = model.Address;
					user.DateofBirth = model.DateofBirth;

					var result = await _userManager.UpdateAsync(user);

					if (result.Succeeded)
					{
						TempData["warning"] = "User Edited Successfully";
						return RedirectToAction("ListUsers");
					}
					else
					{
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}
					}

					return View(model);
				}
			}
			catch (Exception ex)
			{
				
				ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
				return View("Error");
			}
		}


		[HttpPost]
		public async Task<IActionResult> DeleteUser(string Id)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(Id);

				if (user == null)
				{
					ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
					return View("NotFound");
				}
				else
				{
					var result = await _userManager.DeleteAsync(user);

					if (result.Succeeded)
					{
						TempData["error"] = "User Deleted Successfully";

				
						return RedirectToAction("ListUsers");
					}
					else
					{
						
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}
					}

					return View("ListUsers");
				}
			}
			catch (Exception ex)
			{
				
				ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again later.";

				return View("Error"); 
			}
		}


		[HttpGet]
		public async Task<IActionResult> ManageUserRoles(string Id)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(Id);

				if (user == null)
				{
					ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
					return View("NotFound");
				}

				ViewBag.UserId = Id;
				ViewBag.UserName = user.UserName;

				var model = new List<UserRolesViewModel>();

				foreach (var role in await _roleManager.Roles.ToListAsync())
				{
					var userRolesViewModel = new UserRolesViewModel
					{
						RoleId = role.Id,
						RoleName = role.Name,
						Description = role.Description
					};

					if (await _userManager.IsInRoleAsync(user, role.Name))
					{
						userRolesViewModel.IsSelected = true;
					}
					else
					{
						userRolesViewModel.IsSelected = false;
					}

					model.Add(userRolesViewModel);
				}

				return View(model);
			}
			catch (Exception ex)
			{
			

				ViewBag.ErrorMessage = "An error occurred while processing your request.";
				return View("Error");
			}
		}


		[HttpPost]
		public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string Id)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(Id);

				if (user == null)
				{
					ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
					return View("NotFound");
				}

				
				var roles = await _userManager.GetRolesAsync(user);
				var result = await _userManager.RemoveFromRolesAsync(user, roles);

				if (!result.Succeeded)
				{
					TempData["warning"] = "Unable to update roles.";
					ModelState.AddModelError("", "Cannot remove user's existing roles.");
					return View(model);
				}

		
				var selectedRoles = model.Where(x => x.IsSelected).Select(y => y.RoleName).ToList();

				if (selectedRoles.Count > 1)
				{
					ModelState.AddModelError("", "A user can only be assigned to one role at a time.");
					return View(model);
				}

				if (selectedRoles.Any())
				{
					
					result = await _userManager.AddToRoleAsync(user, selectedRoles.First());
					if (!result.Succeeded)
					{
						TempData["warning"] = "Unable to update roles.";
						ModelState.AddModelError("", "Cannot add the selected role to the user.");
						return View(model);
					}
				}

				return RedirectToAction("EditUser", new { Id = Id });
			}
			catch (Exception ex)
			{
				
				TempData["warning"] = "An unexpected error occurred.";
				ModelState.AddModelError("", $"An error occurred: {ex.Message}");
				return View(model);
			}
		}



		[HttpGet]
		public async Task<IActionResult> UsersWithStudentRole()
		{
			try
			{
				var usersWithStudentRole = await _userManager.GetUsersInRoleAsync("Student");
				var studentRoleUsers = await _dbContext.Users
					.Where(u => usersWithStudentRole.Contains(u))
					.Include(u => u.UserSubject)
					.ThenInclude(us => us.Subject)
					.Select(user => new AssignSubjectViewModel
					{
						UserId = user.Id,
						UserName = user.UserName,
						FirstName = user.FirstName,
						LastName = user.LastName,
						Email = user.Email,
						Address = user.Address,
						Grade = user.Grade,
						Gender = user.Gender,
						PhoneNumber = user.PhoneNumber,
						DateofBirth = user.DateofBirth,
						Class = user.Class,
						Division = user.Division,
						IsActive = user.IsActive,
						Subjects = user.UserSubject.Select(us => new SubjectViewModel
						{
							SubjectId = us.Subject.Id,
							SubjectName = us.Subject.SubjectName,
							Marks = us.Marks,
							IsSelected = true
						}).ToList()
					}).ToListAsync();

				return View(studentRoleUsers);
			}
			catch (Exception ex)
			{
				
				TempData["ErrorMessage"] = "An error occurred while fetching data. Please try again later.";
				return RedirectToAction("Error", "Home"); 
			}
		}



		[Authorize(Roles="Admin,Teacher")]
		[HttpGet]
		public async Task<IActionResult> EditStudent(string id)
		{
			try
			{
				if (id == null)
				{
					return NotFound();
				}

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var userWithSubjects = await _dbContext.Users
					.Include(u => u.UserSubject)
					.ThenInclude(us => us.Subject)
					.FirstOrDefaultAsync(u => u.Id == id);

				if (userWithSubjects == null)
				{
					return NotFound();
				}

				var allSubjects = await _dbContext.Subjects.ToListAsync();
				var selectedSubjectIds = userWithSubjects.UserSubject.Select(us => us.SubjectId).ToList();

				var model = new AssignSubjectViewModel
				{
					UserId = user.Id,
					UserName = user.UserName,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Email = user.Email,
					Address = user.Address,
					Grade = user.Grade,
					Gender = user.Gender,
					PhoneNumber = user.PhoneNumber,
					DateofBirth = user.DateofBirth,
					IsActive = user.IsActive,
					Subjects = allSubjects.Select(s => new SubjectViewModel
					{
						SubjectId = s.Id,
						SubjectName = s.SubjectName,
						IsSelected = selectedSubjectIds.Contains(s.Id),
						Marks = userWithSubjects.UserSubject.FirstOrDefault(us => us.SubjectId == s.Id)?.Marks
					}).ToList(),
					SelectedSubjectIds = selectedSubjectIds
				};

				return View(model);
			}
			catch (DbUpdateException ex)
			{
				
				return StatusCode(500, "A database error occurred while processing your request.");
			}
			catch (Exception ex)
			{
			
				return StatusCode(500, "An unexpected error occurred while processing your request.");
			}
		}


		[HttpPost]
		public async Task<IActionResult> EditStudent(AssignSubjectViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var user = await _userManager.FindByIdAsync(model.UserId);
				if (user == null)
				{
					return NotFound();
				}

			
				user.UserName = model.UserName;
				user.FirstName = model.FirstName;
				user.LastName = model.LastName;
				user.Email = model.Email;
				user.Address = model.Address;
				user.Grade = model.Grade;
				user.Gender = model.Gender;
				user.PhoneNumber = model.PhoneNumber;
				user.DateofBirth = model.DateofBirth;
				user.IsActive = model.IsActive;

				
				var userWithSubjects = await _dbContext.Users
					.Include(u => u.UserSubject)
					.FirstOrDefaultAsync(u => u.Id == model.UserId);

				if (userWithSubjects != null)
				{
					
					foreach (var existingSubject in userWithSubjects.UserSubject.ToList())
					{
						if (!model.SelectedSubjectIds.Contains(existingSubject.SubjectId))
						{
							userWithSubjects.UserSubject.Remove(existingSubject);
						}
					}

					
					foreach (var subjectId in model.SelectedSubjectIds)
					{
						var existingSubject = userWithSubjects.UserSubject.FirstOrDefault(us => us.SubjectId == subjectId);
						var marks = model.Subjects.FirstOrDefault(s => s.SubjectId == subjectId)?.Marks;

						if (existingSubject == null)
						{
							userWithSubjects.UserSubject.Add(new UserSubject
							{
								UserId = user.Id,
								SubjectId = subjectId,
								Marks = marks
							});
						}
						else
						{
							existingSubject.Marks = marks;
						}
					}

					_dbContext.Users.Update(user);
					await _dbContext.SaveChangesAsync();
				}

				var result = await _userManager.UpdateAsync(user);
				if (result.Succeeded)
				{
					return RedirectToAction("UsersWithStudentRole");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "An error occurred while updating the student.");
				Console.WriteLine($"Error in EditStudent: {ex.Message}");
			}

			return View(model);
		}


		[Authorize(Roles ="Admin,Teacher")]
		[HttpGet]
		public async Task<IActionResult> DetailsStudent(string id)
		{
			try
			{
				if (id == null)
				{
					return NotFound();
				}

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var userWithSubjects = await _dbContext.Users
					.Include(u => u.UserSubject)
					.ThenInclude(us => us.Subject)
					.FirstOrDefaultAsync(u => u.Id == id);

				var model = new AssignSubjectViewModel
				{
					UserId = user.Id,
					UserName = user.UserName,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Email = user.Email,
					Address = user.Address,
					Gender = user.Gender,
					Grade = user.Grade,
					PhoneNumber = user.PhoneNumber,
					DateofBirth = user.DateofBirth,
					Class = user.Class,
					Division = user.Division,
					IsActive = user.IsActive,
					Subjects = userWithSubjects.UserSubject.Select(us => new SubjectViewModel
					{
						SubjectId = us.Subject.Id,
						SubjectName = us.Subject.SubjectName,
						IsSelected = true,
						Marks = us.Marks
					}).ToList()
				};

				return View(model);
			}
			catch (Exception ex)
			{
				
				return StatusCode(500, "Internal server error. Please try again later.");
			}
		}

		
		[HttpGet]
		public async Task<IActionResult> DetailsStudent2(string id)
		{
			try
			{
			
				var loggedInUser = await _userManager.GetUserAsync(User);

				
				var isStudent = await _userManager.IsInRoleAsync(loggedInUser, "Student");

				
				if (isStudent)
				{
					id = loggedInUser.Id;
				}

			
				if (id == null)
				{
					return NotFound();
				}

				
				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				
				var userWithSubjects = await _dbContext.Users
					.Include(u => u.UserSubject)
					.ThenInclude(us => us.Subject)
					.FirstOrDefaultAsync(u => u.Id == id);

				
				var model = new AssignSubjectViewModel
				{
					UserId = user.Id,
					UserName = user.UserName,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Email = user.Email,
					Address = user.Address,
					Gender = user.Gender,
					Grade = user.Grade,
					PhoneNumber = user.PhoneNumber,
					DateofBirth = user.DateofBirth,
					Class = user.Class,
					Division = user.Division,
					IsActive = user.IsActive,
					Subjects = userWithSubjects.UserSubject.Select(us => new SubjectViewModel
					{
						SubjectId = us.Subject.Id,
						SubjectName = us.Subject.SubjectName,
						IsSelected = true,
						Marks = us.Marks
					}).ToList()
				};

				return View(model);
			}
			catch (Exception ex)
			{
				
				return StatusCode(500, "An error occurred while retrieving student details. Please try again later.");
			}
		}



		[Authorize(Roles = "Admin,Teacher")]
		[HttpGet]
		public async Task<IActionResult> DeleteStudent(string id)
		{
			try
			{
				if (id == null)
				{
					return NotFound();
				}

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var model = new StudentRoleUserViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName
				};

				return View(model);
			}
			catch (Exception ex)
			{
			

				return StatusCode(500, "Internal server error. Please try again later.");
			}
		}


		[HttpPost, ActionName("DeleteStudent")]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			try
			{
				if (id == null)
				{
					return NotFound();
				}

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var result = await _userManager.DeleteAsync(user);
				if (result.Succeeded)
				{
					TempData["error"] = "Student Deleted Successfully";
					return RedirectToAction("UsersWithStudentRole");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

				return View("DeleteStudent", new StudentRoleUserViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName
				});
			}
			catch (Exception ex)
			{
			
				TempData["error"] = "An error occurred while deleting the student: " + ex.Message;
				return RedirectToAction("UsersWithStudentRole");
			}
		}


		[HttpGet]
		public async Task<IActionResult> UsersWithTeacherRole()
		{
			try
			{
				var usersWithTeacherRole = await _userManager.GetUsersInRoleAsync("Teacher");
				var teacherRoleUsers = usersWithTeacherRole.Select(user => new TeacherRoleUserViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					DateofBirth = user.DateofBirth,
					Gender = user.Gender,
					Address = user.Address,
					PhoneNumber = user.PhoneNumber,
				}).ToList();

				return View(teacherRoleUsers);
			}
			catch
			{
				
				return View("Error");
			}
		}


		[HttpGet]
		public async Task<IActionResult> EditTeacher(string id)
		{
			try
			{
				if (id == null)
				{
					return NotFound();
				}

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var model = new TeacherRoleUserViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Address = user.Address,
					Gender = user.Gender,
					PhoneNumber = user.PhoneNumber,
					DateofBirth = user.DateofBirth,
				};

				return View(model);
			}
			catch (Exception)
			{
			
				return StatusCode(500, "Internal server error. Please try again later.");
			}
		}

		    
		[HttpPost]
		public async Task<IActionResult> EditTeacher(TeacherRoleUserViewModel model)
		{
			try
			{
				if (ModelState.IsValid)
				{
					var user = await _userManager.FindByIdAsync(model.Id);
					if (user == null)
					{
						return NotFound();
					}

					user.Id = model.Id;
					user.UserName = model.UserName;
					user.FirstName = model.FirstName;
					user.LastName = model.LastName;
					user.Gender = model.Gender;
					user.Email = model.Email;
					user.PhoneNumber = model.PhoneNumber;
					user.Address = model.Address;
					user.DateofBirth = model.DateofBirth;

					var result = await _userManager.UpdateAsync(user);
					if (result.Succeeded)
					{
						TempData["warning"] = "Teacher Edited Successfully";
						return RedirectToAction("UsersWithTeacherRole");
					}

					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("", error.Description);
					}
				}
			}
			catch (Exception)
			{
				
				TempData["error"] = "An unexpected error occurred while processing your request.";
			}

			return View(model);
		}


		[HttpGet]
		public async Task<IActionResult> DetailsTeacher(string id)
		{
			try
			{
				if (id == null)
				{
					return NotFound();
				}

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var model = new TeacherRoleUserViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					Email = user.Email,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Address = user.Address,
					Gender = user.Gender,
					PhoneNumber = user.PhoneNumber,
					DateofBirth = user.DateofBirth,
				};

				return View(model);
			}
			catch (Exception)
			{
				
				return StatusCode(500, "An unexpected error occurred. Please try again later.");
			}
		}


		[HttpGet]
		public async Task<IActionResult> DeleteTeacher(string id)
		{
			try
			{
				if (id == null)
				{
					return NotFound();
				}

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var model = new TeacherRoleUserViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Email = user.Email,
					Gender = user.Gender,
					PhoneNumber = user.PhoneNumber,
					Address = user.Address,
					DateofBirth = user.DateofBirth
				};

				return View(model);
			}
			catch (Exception ex)
			{
				

				return StatusCode(500, "Internal server error: " + ex.Message);
			}
		}



		[HttpPost, ActionName("DeleteTeacher")]
		public async Task<IActionResult> Deleteconfirmed(string id)
		{
			try
			{
				if (id == null)
				{
					return NotFound();
				}

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var result = await _userManager.DeleteAsync(user);
				if (result.Succeeded)
				{
					TempData["error"] = "Teacher Deleted Successfully";
					return RedirectToAction("UsersWithTeacherRole");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error.Description);
				}

				return View("DeleteTeacher", new TeacherRoleUserViewModel
				{
					Id = user.Id,
					UserName = user.UserName,
					FirstName = user.FirstName,
					LastName = user.LastName,
					Email = user.Email,
					Gender = user.Gender,
					PhoneNumber = user.PhoneNumber,
					Address = user.Address,
					DateofBirth = user.DateofBirth
				});
			}
			catch (Exception)
			{
				
				TempData["error"] = "An error occurred while attempting to delete the teacher. Please try again later.";

				return RedirectToAction("UsersWithTeacherRole");
			}
		}



		//[HttpGet]
		//public async Task<IActionResult> StudentDetails(string id)
		//{
		//	try
		//	{
		//		if (string.IsNullOrEmpty(id))
		//		{
		//			return NotFound("Student ID is required.");
		//		}

		//		var student = await _userManager.FindByIdAsync(id);
		//		if (student == null)
		//		{
		//			return NotFound("Student not found.");
		//		}

		//		var userWithSubjects = await _dbContext.Users
		//			.Include(u => u.UserSubject)
		//			.ThenInclude(us => us.Subject)
		//			.FirstOrDefaultAsync(u => u.Id == id);

		//		var model = new AssignSubjectViewModel
		//		{
		//			UserId = student.Id,
		//			UserName = student.UserName,
		//			FirstName = student.FirstName,
		//			LastName = student.LastName,
		//			Email = student.Email,
		//			Address = student.Address,
		//			Gender = student.Gender,
		//			Grade = student.Grade,
		//			PhoneNumber = student.PhoneNumber,
		//			DateofBirth = student.DateofBirth,
		//			IsActive = student.IsActive,
		//			Class = student.Class,
		//			Division = student.Division,
		//			Subjects = userWithSubjects.UserSubject.Select(us => new SubjectViewModel
		//			{
		//				SubjectId = us.Subject.Id,
		//				SubjectName = us.Subject.SubjectName,
		//				Marks = us.Marks
		//			}).ToList()
		//		};

		//		return View(model);
		//	}
		//	catch (Exception ex)
		//	{
		//		// Log the exception (you can use a logging framework here)
		//		// For example: _logger.LogError(ex, "An error occurred while fetching student details.");

		//		return StatusCode(500, "Internal server error: " + ex.Message);
		//	}
		//}






	}

}




