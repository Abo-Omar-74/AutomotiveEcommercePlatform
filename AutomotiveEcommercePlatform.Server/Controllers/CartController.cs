using AutomotiveEcommercePlatform.Server.Data;
using AutomotiveEcommercePlatform.Server.DTOs.CartDTOs;
using AutomotiveEcommercePlatform.Server.Models;
using DataBase_LastTesting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactApp1.Server.Data;
using System.Security.Claims;

namespace AutomotiveEcommercePlatform.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "User")] // User arg3
        [HttpGet("GetCartItems")]
        public async Task<IActionResult> GetCartItems()
        {
            string userId = HttpContext.User.FindFirstValue("Id");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User does not exist!");
            var CarIds = await _context.CartItems.Where(c => c.CartId == userId).Select(c=> c.CarId).ToListAsync();

            var cars = new List<Car>();
            string error = "";
            foreach (var carId in CarIds)
            {
                var car = await _context.Cars.SingleOrDefaultAsync(c => c.Id == carId);
                if (car == null)
                {
                    error += "Some Cars has been removed by their trader\n";
                    continue;
                }else if (car.InStock == false)
                {
                    error += "Some Cars has been Sold by Other Users\n";
                    continue;
                }
                cars.Add(car);
            }

            var responce = new GetCartItemsDto()
            {
                cars = cars,
                Error = error, 
                result = (error=="")
            };

            if (responce.result == false)
                return BadRequest(responce);

            return Ok(responce);
        }
        [Authorize(Roles = "User")] // user
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(int carId)
        {
            string userId = HttpContext.User.FindFirstValue("Id");
            var car = await _context.Cars.SingleOrDefaultAsync(c => c.Id == carId);
            if (car == null)
                return NotFound("Car does not found !");
            if (car.InStock == false)
                return BadRequest("This Car is out of stock !");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User does not exist!");
            // Is the cart already initialized
            var cart = await _context.Carts.SingleOrDefaultAsync(t=>t.CartId == userId);
            // if not : initialize it 
            if (cart == null)
            {
                _context.Carts.AddAsync(new Cart() { CartId = userId });
                _context.SaveChanges();
            }
            // is the car already in the cart ?
            var exist = await _context.CartItems.SingleOrDefaultAsync(c => c.CarId == carId && c.CartId == userId);
            if (exist != null)
                return BadRequest("The Car is already in the Cart");

            var cartItem = new CartItem() { CartId = userId, CarId = carId };
            _context.CartItems.AddAsync(cartItem);
            _context.SaveChanges();

            return Ok(cartItem);
        }

        [Authorize(Roles = "User")] // user  arg9
        [HttpPost("ProceedToPay")]
        public async Task<IActionResult> AddToCart()
        {
            string userId = HttpContext.User.FindFirstValue("Id");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User does not exist!");


            var CartItems = await _context.CartItems.Where(c => c.CartId == userId).ToListAsync();
            
            var carIds =  CartItems.Select(c => c.CarId).ToList();

            if (carIds.Count()==0)
                return BadRequest("The Cart is empty");

            decimal totalPrice = 0;
            List<Car> cars = new List<Car>();
            foreach (var carId in carIds)
            {
                var car = await _context.Cars.SingleOrDefaultAsync(c => c.Id == carId);
                if (car == null || car.InStock == false)
                {
                    return BadRequest(new
                    {
                        Error = $"Transaction Suspended ,{(car==null ? "This Car is no Longer exist" : "This Car is out of stock")} " , 
                        CarId = carId
                    });
                }
                totalPrice += car.Price;
                cars.Add(car);
            }

            var order = await _context.Orders.AddAsync(new Order() { Price = totalPrice, PurchaseDate = DateTime.Now , UserId = userId });
            _context.SaveChanges();

            // adding cars to the order and make them out of stock 
            foreach (var car in cars)
            {
                car.InStock = false ;
                car.OrderId = order.Entity.Id;

            }
            // making this cart empty 
            foreach(var item in CartItems)
                _context.CartItems.Remove(item);
            _context.SaveChanges();

            var responce = new ProceedToPayResponceDto()
            {
                cars = cars,
                orderId = order.Entity.Id
            };

            return Ok(responce);
        }

        [Authorize(Roles = "User")] // user
        [HttpDelete("DeleteFromCart")]
        public async Task<IActionResult> DeleteAsync(int carId)
        {
            string userId = HttpContext.User.FindFirstValue("Id");
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound("User does not exist!");

            // Search for the car in my cart 
            var cartItem = await _context.CartItems.SingleOrDefaultAsync(c => c.CarId == carId && c.CartId == userId);
            if (cartItem == null)
                return NotFound("Car does not exist!");

            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();

            return Ok(cartItem);
        }
    }

}
