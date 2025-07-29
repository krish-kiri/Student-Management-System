using Microsoft.AspNetCore.Mvc;
using Student_Management.Data;
using Student_Management.Models;

namespace Student_Management.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _dbcontext;
        public SubjectController(ApplicationDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }


		[HttpGet]
		public IActionResult Index()
		{
			try
			{
				List<Subject> subject = _dbcontext.Subjects.ToList();
				return View(subject);
			}
			catch (Exception ex)
			{
				

				
				return View("Error"); 
			}
		}



		[HttpGet]
		public IActionResult Create()
		{
			try
			{
				
				return View();
			}
			catch (Exception ex)
			{
				
				Console.WriteLine(ex.Message);

				
				return View("Error"); 
			}
		}



		[HttpPost]
		public IActionResult Create(Subject subject)
		{
			try
			{
				_dbcontext.Subjects.Add(subject);
				_dbcontext.SaveChanges();
				TempData["success"] = "Subject Created Successfully ";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				
				TempData["error"] = $"Error: {ex.Message}";
				return RedirectToAction(nameof(Index));
			}
		}


		[HttpGet]
		public IActionResult Details(Guid Id)
		{
			try
			{
				Subject subject = _dbcontext.Subjects.FirstOrDefault(x => x.Id == Id);
				if (subject == null)
				{
					
					return NotFound();
				}
				return View(subject);
			}
			catch (Exception ex)
			{
				
				return StatusCode(500, "Internal server error");
			}
		}


		[HttpGet]
		public IActionResult Edit(Guid Id)
		{
			try
			{
				Subject subject = _dbcontext.Subjects.FirstOrDefault(x => x.Id == Id);
				if (subject == null)
				{
					return NotFound(); 
				}
				return View(subject);
			}
			catch (Exception ex)
			{
				
				return StatusCode(500, "Internal server error. Please try again later.");
			}
		}




		[HttpPost]
		public IActionResult Edit(Subject subject)
		{
			try
			{
				_dbcontext.Subjects.Update(subject);
				_dbcontext.SaveChanges();
				TempData["warning"] = "Subject Updated Successfully ";
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				

				TempData["error"] = "An error occurred while updating the subject. Please try again later.";
				return RedirectToAction(nameof(Index));
			}
		}


		[HttpGet]
		public IActionResult Delete(Guid Id)
		{
			try
			{
				Subject subject = _dbcontext.Subjects.FirstOrDefault(x => x.Id == Id);
				if (subject == null)
				{
					
					return NotFound(); 
				}

				return View(subject);
			}
			catch (Exception ex)
			{
				
				return StatusCode(500, "An error occurred while processing your request.");
			}
		}




		[HttpPost]
		public IActionResult Delete(Subject subject)
		{
			try
			{
				
				_dbcontext.Subjects.Remove(subject);
				_dbcontext.SaveChanges();

				
				TempData["error"] = "Subject Deleted Successfully";
			}
			catch (Exception ex)
			{
				
				TempData["error"] = "An error occurred while deleting the subject. Please try again.";
			}

			
			return RedirectToAction(nameof(Index));
		}


	}
}
