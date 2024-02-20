using AutomotiveEcommercePlatform.Server.Data;
using DataBase_LastTesting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;

namespace AutomotiveEcommercePlatform.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomePageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomePageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [Authorize] // arg3
        [HttpGet]

        public async Task<IActionResult> GetTheMostExpensiveCarsAsync()
        {
            int min = 0;
            var cars = _context.Cars.OrderByDescending(c => c.Price);
            int count = cars.Count();
            if (count <= 10)
                min = count;

            var Responce = _context.Cars
                .OrderByDescending(p => p.Price)
                .Take(min)
                .ToList();

            return Ok(Responce);
        }
    }
}
