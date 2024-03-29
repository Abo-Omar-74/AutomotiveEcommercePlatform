﻿using AutomotiveEcommercePlatform.Server.Data;
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

            /* IQueryable<Car> query = _context.Cars.Where(c=>c.InStock==true)
                 .Include(c => c.CarReviews); ;*/

            var query = from car in _context.Cars
                join review in _context.CarReviews
                    on car.Id equals review.CarId into carReviews
                where car.InStock == true
                select new
                {
                    Car = car,
                    Reviews = carReviews.ToList(),
                    AverageRating = carReviews.Any() ? carReviews.Average(r => r.Rating) : 0 

                };

            if (!string.IsNullOrEmpty(searchDto.BrandName))
                query = query.Where(q => q.Car.BrandName.ToUpper().Contains(searchDto.BrandName.ToUpper()));

            if (!string.IsNullOrEmpty(searchDto.CarCategory))
                query = query.Where(q => q.Car.CarCategory.ToUpper().Contains(searchDto.CarCategory.ToUpper()));

            if (!string.IsNullOrEmpty(searchDto.ModelName))
                query = query.Where(q => q.Car.ModelName.ToUpper().Contains( searchDto.ModelName.ToUpper()));

            if (searchDto.ModelYear!=null)
                query = query.Where(q => q.Car.ModelYear == searchDto.ModelYear);

            if (searchDto.minPrice != null)
                query = query.Where(q => q.Car.Price >= searchDto.minPrice);

            if (searchDto.maxPrice != null)
                query = query.Where(q => q.Car.Price <= searchDto.maxPrice);


            var pageSize = 9;

            var totalCount = query.Count();

            if (!(0 <= page && page <= totalCount))
                return NotFound("Page Not Found!");

            var totalPages =totalCount / pageSize;

            var cars = query.Skip((page-1)*pageSize).Take(pageSize).ToList();


            var result = new
            {
                TotalCount = totalCount,
                Page = page,
                Cars = cars.Select(q => new { Car = q.Car, Reviews = q.Reviews, AverageRating = q.AverageRating })

            };

            return Ok(result);

        }   
    }
}
