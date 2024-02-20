using System.Security.Claims;
using AutomotiveEcommercePlatform.Server.Data;
using AutomotiveEcommercePlatform.Server.DTOs.TraderDashboardDTOs;
using AutomotiveEcommercePlatform.Server.DTOs.UserInfoDTO;
using DataBase_LastTesting.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace AutomotiveEcommercePlatform.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize] // user + trader 
        [HttpPut]
        public async Task<IActionResult> EditUserData(UpdateUserDto dto)
        {
            string userId = HttpContext.User.FindFirstValue("Id");
            var User = await _userManager.FindByIdAsync(userId);
            if (User == null)
                return NotFound("The User does not exist!");

            /*Validate the date*/

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                if (!Regex.IsMatch(dto.PhoneNumber, @"^\d{11}$"))
                    return BadRequest("Invalid PhoneNumber!");

            if (!string.IsNullOrEmpty(dto.Email))
            {
                if (!Regex.IsMatch(dto.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                    return BadRequest("Invalid Email!");

                var emailExist = await _userManager.FindByEmailAsync(dto.Email);
                if (emailExist != null)
                    return BadRequest("Email is already Exist!");
            }

            /* Update the data */

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                User.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Email))
                User.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.FirstName))
                User.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                User.LastName = dto.LastName;

            if (!string.IsNullOrEmpty(dto.LastName) || !string.IsNullOrEmpty(dto.FirstName))
                User.DisplayName = $"{User.FirstName} {User.LastName}";

            _context.SaveChanges();

            return Ok(new
            {
                User.Id,
                User.FirstName,
                User.LastName,
                User.Email,
                User.PhoneNumber
            });
        }

        [Authorize]
        [HttpGet]
        
        public async Task<IActionResult> GetUserInfoAsync()
        {
            string userId = HttpContext.User.FindFirstValue("Id");
            var User = await _userManager.FindByIdAsync(userId);
            if (User == null)
                return NotFound("User is not founded !");
            
            var roles = await  _userManager.GetRolesAsync(User);
            if (roles == null)
                return BadRequest("Something went Wrong!");

            string role = roles.FirstOrDefault();

            return Ok(new
            {
                User.Id,
                User.FirstName,
                User.LastName,
                User.Email,
                User.PhoneNumber,
                Role = role
            });
        }
    }
}
