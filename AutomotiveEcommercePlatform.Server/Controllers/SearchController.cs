using AutomotiveEcommercePlatform.Server.Data;
using AutomotiveEcommercePlatform.Server.DTOs.CarInfoPageDTOs;
using AutomotiveEcommercePlatform.Server.DTOs.SearchDTOs;
using DataBase_LastTesting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;

namespace AutomotiveEcommercePlatform.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllAsync()
        //{
        //    var cars = _context.Cars.Where(c=>c.InStock == true);

        //    var carsInfo = new List<CarInfoResponseDto>();

        //    foreach (var car in cars)
        //    {
        //        var Trader = await _userManager.FindByIdAsync(car.TraderId);
        //        if (Trader == null) continue;

        //        var averageTraderRating = _context.TraderRatings
        //            .Where(tr => tr.TraderId == car.TraderId) 
        //            .Select(tr => tr.Rating)
        //            .ToList()
        //            .DefaultIfEmpty(0) 
        //            .Average();

        //        var responce = new CarInfoResponseDto()
        //        {
        //            Id = car.Id,
        //            ModelName = car.ModelName,
        //            BrandName = car.BrandName,
        //            CarCategory = car.CarCategory,
        //            CarImage =car.CarImage,
        //            ModelYear = car.ModelYear,
        //            Price = car.Price,
        //            carReviews = car.CarReviews,
        //            FirstName = Trader.FirstName,
        //            LastName = Trader.LastName,
        //            PhoneNumber = Trader.PhoneNumber,
        //            InStock = car.InStock,
        //            TraderRating = averageTraderRating
        //        };
        //        carsInfo.Add(responce);
        //    }

        //    return Ok(carsInfo);
        //}

        [Authorize] // user (trader?)
        [HttpGet]
        public async Task<IActionResult> GetFilteredCars([FromQuery]SearchDto searchDto , [FromQuery] int page=1)
        {
            if (searchDto == null)
                return NotFound("Not Found the Page !");

            IQueryable<Car> query = _context.Cars.Where(c=>c.InStock==true);

            if (!string.IsNullOrEmpty(searchDto.BrandName))
                query = query.Where(q => q.BrandName.ToUpper().Contains(searchDto.BrandName.ToUpper()));

            if (!string.IsNullOrEmpty(searchDto.CarCategory))
                query = query.Where(q => q.CarCategory.ToUpper().Contains(searchDto.CarCategory.ToUpper()));

            if (!string.IsNullOrEmpty(searchDto.ModelName))
                query = query.Where(q => q.ModelName.ToUpper().Contains( searchDto.ModelName.ToUpper()));

            if (searchDto.ModelYear!=null)
                query = query.Where(q => q.ModelYear == searchDto.ModelYear);

            if (searchDto.minPrice != null)
                query = query.Where(q => q.Price >= searchDto.minPrice);

            if (searchDto.maxPrice != null)
                query = query.Where(q => q.Price <= searchDto.maxPrice);


            var pageSize = 3;

            var totalCount = query.Count();

            if (!(0 <= page && page <= totalCount))
                return NotFound("Page Not Found!");

            var totalPages =totalCount / pageSize;

            var cars = query.Skip((page-1)*pageSize).Take(pageSize).ToList();

            return Ok(cars);

        }   
    }
}
