using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Services;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(
            IOrderService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            CreateOrderDto dto)
        {
            var result =
                await _service.CreateOrder(dto);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result =
                await _service.GetOrders();

            return Ok(result);
        }
    }
}