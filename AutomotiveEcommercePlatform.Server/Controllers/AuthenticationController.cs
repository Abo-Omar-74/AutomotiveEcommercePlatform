using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using AutomotiveEcommercePlatform.Server.Configurations;
using AutomotiveEcommercePlatform.Server.Data;
using AutomotiveEcommercePlatform.Server.DTOs.AuthenticationDTOs;
using AutomotiveEcommercePlatform.Server.Models;
using AutomotiveEcommercePlatform.Server.Services;
using DataBase_LastTesting.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReactApp1.Server.Data;


namespace AutomotiveEcommercePlatform.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController] 
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthenticationController(UserManager<ApplicationUser> userManager , IConfiguration configuration, IAuthService authService , ApplicationDbContext context , RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _authService = authService;
            _context = context;
            _roleManager = roleManager;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
        {
            // validate the incoming request
            if (ModelState.IsValid)
            {

                // Validate First Name and Last Name 
                if (requestDto.FirstName == null || requestDto.LastName == null)
                {
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string> {
                            "This Field is required !"
                        }
                    });
                }

                // Validate the email 
                if (!Regex.IsMatch(requestDto.Email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string> {
                            "Invalid Email!"
                        }
                    });

                // Checking if the email is already exist 
                var userExist = await _userManager.FindByEmailAsync(requestDto.Email);
                if (userExist != null)
                {
                    return BadRequest(new AuthResult ()
                    {
                        Result = false ,
                        Errors = new List<string> {
                        "Email is already exit"
                        }
                    });
                }

                // Password Match 
                if (requestDto.Password != requestDto.ConfirmPassword) 
                    return BadRequest(new AuthResult ()
                   {
                       Result = false ,
                       Errors = new List<string> {
                       "Un Matched Passwords!"
                       }
                   });

                // Validate The Phone Number
                if (!Regex.IsMatch(requestDto.PhoneNumber , @"^\d{11}$")) 
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string> {
                            "Invalid Phone Number!"
                        }
                    });

                // create a user 
                var newUser = new ApplicationUser()
                {
                    Email = requestDto.Email,
                    UserName = requestDto.Email ,
                    PhoneNumber = requestDto.PhoneNumber,
                    FirstName = requestDto.FirstName,
                    LastName = requestDto.LastName,
                };

                // Validate the role + Adding User to a role 
                if (requestDto.Role.ToUpper() == "ADMIN" ||(requestDto.Role.ToUpper() != "USER" && requestDto.Role.ToUpper() != "TRADER"))
                    return BadRequest (new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string> {
                            "This Action is forbidden !"
                        }
                    });

                var isCreated = await _userManager.CreateAsync(newUser, requestDto.Password);
                

                if (isCreated.Succeeded)
                {
                    // Generate the token
                    var token = GenerateJwtToken(newUser , requestDto.Role);

                    var UserToRole = new AddRoleModel
                    {
                        role = requestDto.Role,
                        email = requestDto.Email
                    };
                    var AddRoleResult = await _authService.AddRoleAsync(UserToRole);

                    if (AddRoleResult.Result == false)
                    {
                        return BadRequest(AddRoleResult);
                    }

                    var createdUser = await _userManager.FindByEmailAsync(requestDto.Email);
                        if (requestDto.Role.ToUpper() == "USER")
                        {
                            await _context.Users.AddAsync(new User(){ UserId = newUser.Id});
                            await _context.Carts.AddAsync(new Cart() { CartId = newUser.Id });
                            _context.SaveChanges();
                        }

                        if (requestDto.Role.ToUpper() == "TRADER")
                        {
                            await _context.Traders.AddAsync(new Trader() { TraderId = newUser.Id });
                            _context.SaveChanges();
                        }
                        return Ok(new AuthResult()
                    {
                        Role = requestDto.Role,
                        Result = true,
                        Token = token
                    });
                }
                return  BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = isCreated.Errors.Select(des => des.Description).ToList()
                });
            }
            
            return BadRequest();
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginRequestDto loginRequest)
        {
            if (ModelState.IsValid)
            {
                // check if the user exist 
                var existingUser = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (existingUser == null)
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Invalid Email or Password !"
                        }
                    });
                var isCorrect = await _userManager.CheckPasswordAsync(existingUser,loginRequest.Password);

                if (!isCorrect)
                    return BadRequest(new AuthResult()
                    {
                        Result = false,
                        Errors = new List<string>()
                        {
                            "Invalid Email or Password !"
                        }
                    });

                var roles = await _userManager.GetRolesAsync(existingUser);
                var onlyRole = roles.FirstOrDefault();
                var jwtToken = GenerateJwtToken(existingUser , onlyRole);
                return Ok(new AuthResult()
                {
                    Role = onlyRole,
                    Result = true,
                    Token = jwtToken
                });

            }
            return BadRequest(new AuthResult()
            {
                Result = false ,
                Errors = new List<string>()
                {
                    "Invalid Email or Password !"
                }
            });
        }
        private string GenerateJwtToken(ApplicationUser user , string role)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            // Token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, value:user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())
                }),
                Expires = DateTime.Now.AddDays(30), // will be changed 
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key) , SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
    }
}
