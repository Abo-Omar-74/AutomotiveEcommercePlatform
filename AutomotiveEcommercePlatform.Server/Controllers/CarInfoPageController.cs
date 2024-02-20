using AutomotiveEcommercePlatform.Server.Data;
using DataBase_LastTesting.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using System.Diagnostics;
using System.Linq;
using AutomotiveEcommercePlatform.Server.DTOs.CarInfoPageDTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.IO.Pipelines;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;


namespace AutomotiveEcommercePlatform.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarInfoPageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CarInfoPageController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize] 
        [HttpGet]
        public async Task<IActionResult> GetCarInfo(int carId)
        {
            var car = await _context.Cars.SingleOrDefaultAsync(t => t.Id == carId);
            if (car == null)
                return NotFound("Car is not Found!");
            var trader = await _userManager.FindByIdAsync(car.TraderId);
            if (trader == null)
                return BadRequest("SomeThing Went Wrong !");
            // Car info + Car Review + Trader display + Trader Rating 

            var averageTraderRating = _context.TraderRatings
                .Where(tr => tr.TraderId == car.TraderId)
                .Select(tr => tr.Rating)
                .ToList()
                .DefaultIfEmpty(0)
                .Average();
            var responce = new CarInfoResponseDto()
            {
                Id = carId,
                BrandName = car.BrandName,
                ModelName = car.ModelName,
                ModelYear = car.ModelYear,
                Price = car.Price,
                CarCategory = car.CarCategory,
                CarImage = car.CarImage,
                InStock = car.InStock,
                carReviews = car.CarReviews,
                TraderRating = averageTraderRating,
                FirstName = trader.FirstName,
                LastName = trader.LastName,
                PhoneNumber = trader.PhoneNumber
            };

            return Ok(responce);
        }


    }
}
