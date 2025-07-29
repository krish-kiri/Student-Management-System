using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Management.Models;
using System.Diagnostics;

namespace Student_Management.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


		public IActionResult Index()
		{
			try
			{
				return View();
			}
			catch (Exception ex)
			{
				
				Console.WriteLine($"Error: {ex.Message}");

				
				return View("Error"); 
			}
		}



		[Authorize(Roles = "Admin")]
		public IActionResult Privacy()
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



	}
}
