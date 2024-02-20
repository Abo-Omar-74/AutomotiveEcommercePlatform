using AutomotiveEcommercePlatform.Server.Data;
using AutomotiveEcommercePlatform.Server.DTOs.AddressDTO;
using AutomotiveEcommercePlatform.Server.Models;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using System.Security.Claims;

namespace AutomotiveEcommercePlatform.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class AddressControllers : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressControllers(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "User")] // user
        [HttpPost]

        public async Task<IActionResult> AddingAddressAsync(AddAddressDTO dto)
        {
            string userId = HttpContext.User.FindFirstValue("Id");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("Not Found");
            if (dto.City == null)
                return BadRequest("City is Required");
            if (dto.Country == null)
                return BadRequest("Country is Required");
            if (dto.State == null)
                return BadRequest("State is Required");
            if (dto.StreetAddress == null)
                return BadRequest("StreetAddress is Required");
            if (dto.ZipCode == null)
                return BadRequest("ZipCode is Required");

            var address = new Address()
            {
                UserId = userId,
                Country = dto.Country,
                State = dto.State,
                City = dto.City,    
                ZipCode = dto.ZipCode,
                StreetAddress = dto.StreetAddress
            };

            await _context.AddAsync(address);
            _context.SaveChanges();
            return Ok(address);
        }

        [Authorize(Roles = "User")] // user 
        [HttpGet]

        public async Task<IActionResult> GetAddressAsync()
        {
            string userId = HttpContext.User.FindFirstValue("Id");

            var user = await _userManager.FindByIdAsync(userId);


            //sbaka
            //var address =await _context.Addresses.SingleOrDefaultAsync(u=>u.UserId==userId);

            //var response = new AddressInfoDto()
            //{
            //    Country = address.Country,
            //    City = address.City,
            //    State = address.State,
            //    StreetAddress = address.StreetAddress,
            //    ZipCode = address.ZipCode,
            //};
            //return Ok(response);

            //m4 sbaka
            if (user == null || userId == null)
                return NotFound("No User Was Found");
            var address = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
            return Ok(address);

        }
    }
}
