//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Product.Application.DTOs;
//using Product.Application.Interfaces;

//[ApiController]
//[Route("api/products")]
//public class ProductController : ControllerBase
//{
//    private readonly IProductService _service;

//    public ProductController(IProductService service)
//    {
//        _service = service;
//    }

//    [Authorize]
//    [HttpPost]
//    public async Task<IActionResult> Add(CreateProductDto dto)
//    {
//        await _service.Add(dto);
//        return Ok("Product Added");
//    }

//    [Authorize]
//    [HttpGet]
//    public async Task<IActionResult> GetAll()
//    {
//        return Ok(await _service.GetAll());
//    }
//}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.Interfaces;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    //[Authorize]
    //[HttpPost]
    //public async Task<IActionResult> Add(CreateProductDto dto)
    //{
    //    await _service.Add(dto);
    //    return Ok("Added");
    //}

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(
    [FromForm] CreateProductDto dto)
    {
        await _service.Add(dto);

        return Ok("Product Added");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _service.GetById(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }
}