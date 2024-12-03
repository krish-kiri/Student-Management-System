using Student_Management.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




namespace Student_Management.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdministrationController(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> ListRoles()
        {
            List<ApplicationRole> roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                // Check if the role already exists
                bool roleExists = await _roleManager.RoleExistsAsync(roleModel?.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("", "Role Already Exists");
                }
                else
                {
                    // Create the role
                    // We just need to specify a unique role name to create a new role
                    ApplicationRole identityRole = new ApplicationRole
                    {
                        Name = roleModel?.RoleName,
                        Description = roleModel?.Description
                    };

                    // Saves the role in the underlying AspNetRoles table
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

            return View(roleModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string roleId)
        {
            //First Get the role information from the database
            ApplicationRole? role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Handle the scenario when the role is not found
                return View("Error");
            }

            //Populate the EditRoleViewModel from the data retrived from the database
            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description,
                Users = new List<string>()
                // You can add other properties here if needed
            };

            // Retrieve all the Users
            foreach (var user in _userManager.Users.ToList())
            {
                // If the user is in this role, add the username to
                // Users property of EditRoleViewModel. 
                // This model object is then passed to the view for display
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }



        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    // Handle the scenario when the role is not found
                    ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found";
                    return View("NotFound");
                }
                else
                {
                    role.Name = model.RoleName;
                    role.Description = model.Description;
                    // Update other properties if needed

                    var result = await _roleManager.UpdateAsync(role);
                    if (result.Succeeded)
                    {
                        TempData["warning"] = "Role Edited Successfully";
                        return RedirectToAction("ListRoles"); // Redirect to the roles list
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }
            }

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                // Role not found, handle accordingly
                ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
                return View("NotFound");
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                TempData["error"] = "Role Deleted Successfully";
                // Role deletion successful
                return RedirectToAction("ListRoles"); // Redirect to the roles list page
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            // If we reach here, something went wrong, return to the view
            return View("ListRoles", await _roleManager.Roles.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
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
                    UserName = user.UserName
                };

                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            //First check whether the Role Exists or not
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
                    //If IsSelected is true and User is not already in this role, then add the user
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    //If IsSelected is false and User is already in this role, then remove the user
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    //Don't do anything simply continue the loop
                    continue;
                }

                //If you add or remove any user, please check the Succeeded of the IdentityResult
                if (result.Succeeded)
                {
                   TempData["warning"] = "User Edited Successfully";

                    if (i < (model.Count - 1))
                        continue;
                    else
                        return RedirectToAction("EditRole", new { roleId = roleId });
                }
            }

            return RedirectToAction("EditRole", new { roleId = roleId });
        }
        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = _userManager.Users;
            return View(users);
        }
        [HttpGet]
        public async Task<IActionResult> EditUser(string Id)
        {
            //First Fetch the User Details by UserId
            var user = await _userManager.FindByIdAsync(Id);

            //Check if User Exists in the Database
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
                return View("NotFound");
            }

            // GetClaimsAsync retunrs the list of user Claims
            var userClaims = await _userManager.GetClaimsAsync(user);

            // GetRolesAsync returns the list of user Roles
            var userRoles = await _userManager.GetRolesAsync(user);

            //Store all the information in the EditUserViewModel instance
            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateofBirth = user.DateofBirth,



                Roles = userRoles
            };

            //Pass the Model to the View
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            //First Fetch the User by Id from the database
            var user = await _userManager.FindByIdAsync(model.Id);

            //Check if the User Exists in the database
            if (user == null)
            {
                //If the User does not exists in the database, then return Not Found Error View
                ViewBag.ErrorMessage = $"User with Id = {model.Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //If the User Exists, then proceed and update the data
                //Populate the user instance with the data from EditUserViewModel
                user.Id = model.Id;
                user.UserName = model.UserName;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.Gender = model.Gender;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.DateofBirth = model.DateofBirth;











                //UpdateAsync Method will update the user data in the AspNetUsersentity table
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)

				{
                    TempData["warning"] = "User Edited Successfully";

                    //Once user data updated redirect to the ListUsers view
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    //In case any error, stay in the same view and show the model validation error
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            //First Fetch the User you want to Delete
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                // Handle the case where the user wasn't found
                ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
                return View("NotFound");
            }
            else
            {
                //Delete the User Using DeleteAsync Method of UserManager Service
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    TempData["error"] = "User Deleted Successfully";

                    // Handle a successful delete
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    // Handle failure
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                return View("ListUsers");
            }
        }
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string Id)
        {
            //First Fetch the User Information from the Identity database by user Id
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                //handle if User Not Found in the Database
                ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
                return View("NotFound");
            }

            //Store the UserId in the ViewBag which is required while updating the Data
            //Store the UserName in the ViewBag which is used for displaying purpose
            ViewBag.UserId = Id;
            ViewBag.UserName = user.UserName;

            //Create a List to Hold all the Roles Information
            var model = new List<UserRolesViewModel>();

            //Loop Through Each role and populate the model 
            foreach (var role in await _roleManager.Roles.ToListAsync())
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    Description = role.Description
                };

                //Check if the Role is already assigned to this user
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }

                //Add the userRolesViewModel to the model
                model.Add(userRolesViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {Id} cannot be found";
                return View("NotFound");
            }

            //fetch the list of roles the specified user belongs to
            var roles = await _userManager.GetRolesAsync(user);

            //Then remove all the assigned roles for this user
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
               TempData["warning"] = "Role Edited Successfully";

                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            List<string> RolesToBeAssigned = model.Where(x => x.IsSelected).Select(y => y.RoleName).ToList();

            //If At least 1 Role is assigned, Any Method will return true
            if (RolesToBeAssigned.Any())
            {
                //add a user to multiple roles simultaneously

                result = await _userManager.AddToRolesAsync(user, RolesToBeAssigned);
                if (!result.Succeeded)
                {
                    TempData["warning"] = "Role Edited Successfully";
                    ModelState.AddModelError("", "Cannot Add Selected Roles to User");
                    return View(model);
                }
            }

            return RedirectToAction("EditUser", new { Id = Id });
        }

        [HttpGet]
        public async Task<IActionResult> UsersWithStudentRole()
        {
            var usersWithStudentRole = await _userManager.GetUsersInRoleAsync("Student");
            var studentRoleUsers = usersWithStudentRole.Select(user => new StudentRoleUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                Gender = user.Gender,
               PhoneNumber = user.PhoneNumber,

            }).ToList();

            return View(studentRoleUsers);
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(string id)
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
                LastName = user.LastName,
                Address = user.Address,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                DateofBirth = user.DateofBirth
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentRoleUserViewModel model)
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
                user.DateofBirth= model.DateofBirth;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Gender = model.Gender;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;


                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["warning"] = "Student Edited Successfully";
                    return RedirectToAction("UsersWithStudentRole");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsStudent(string id)
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
                LastName = user.LastName,
                Address = user.Address,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                DateofBirth = user.DateofBirth,
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteStudent(string id)
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

        [HttpPost, ActionName("DeleteStudent")]
        public async Task<IActionResult> DeleteConfirmed(string id)
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

        [HttpGet]
        public async Task<IActionResult> UsersWithTeacherRole()
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
        [HttpGet]
        public async Task<IActionResult> EditTeacher(string id)
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
        [HttpPost]
        public async Task<IActionResult> EditTeacher(TeacherRoleUserViewModel model)
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

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsTeacher(string id)
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
                Address = user.Address,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                DateofBirth = user.DateofBirth,
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteTeacher(string id)
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

        [HttpPost, ActionName("DeleteTeacher")]
        public async Task<IActionResult> Deleteconfirmed(string id)
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
    }

}




