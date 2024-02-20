using AutomotiveEcommercePlatform.Server.Data;
using AutomotiveEcommercePlatform.Server.DTOs.OrdersDTO;
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
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "User")] // (Trader?)
        [HttpGet ]
        public async Task<IActionResult> GetOrderDetailsAsync([FromQuery] int orderId)
        {
            var order = await _context.Orders.SingleOrDefaultAsync(c => c.Id == orderId);
            if (order == null)
                return NotFound("The Order does not exist!");

            var orderCars = await _context.Cars.Where(c => c.OrderId == orderId).ToListAsync();

            var customer = await _userManager.FindByIdAsync(order.UserId);

            var carFullInfo = new List<OrderCarInfoDto>();
            /*
                public int OrderId { get; set; }
               public List<OrderCarInfoDto> CarsInfo { get; set; }
               public decimal TotalPrice { get; set; }
               public string PaymentStatus { get; set; }
               public DateTime PurchaseDate { get; set; }
             */
            foreach (var car in orderCars)
            {
                var trader = await _userManager.FindByIdAsync(car.TraderId);
                var orderCarInfo = new OrderCarInfoDto()
                {
                    CarId = car.Id , 
                    BrandName = car.BrandName,
                    ModelName = car.ModelName,
                    ModelYear = car.ModelYear,
                    Price = car.Price,
                    CarCategory = car.CarCategory,
                    TraderName = trader!=null ? trader.DisplayName : String.Empty
                };
                carFullInfo.Add(orderCarInfo);
            }
            var total = orderCars
                .Select(c => c.Price)
                .ToList()
                .DefaultIfEmpty(0)
                .Sum();
            var orderDetails = new OrdersDTO()
            {
                OrderId = orderId,
                CarsInfo = carFullInfo,
                TotalPrice = total,
                PaymentStatus = "Completed",
                CustomerName = customer.DisplayName,
                PurchaseDate = DateTime.Now
            };
            return Ok(orderDetails);
        }
    }
}
