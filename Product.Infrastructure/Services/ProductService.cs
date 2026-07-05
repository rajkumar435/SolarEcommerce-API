//using Microsoft.EntityFrameworkCore;
//using Product.Application.DTOs;
//using Product.Application.Interfaces;
//using Product.Domain.Entities;
//using Product.Infrastructure.Data;

//namespace Product.Infrastructure.Services
//{
//    public class ProductService : IProductService
//    {
//        private readonly ProductDbContext _context;

//        public ProductService(ProductDbContext context)
//        {
//            _context = context;
//        }

//        // =========================
//        // ADD PRODUCT
//        // =========================
//        public async Task Add(CreateProductDto dto)
//        {
//            var product = new Products
//            {
//                Name = dto.Name,
//                Description = dto.Description,
//                Price = dto.Price,
//                Stock = dto.Stock,

//                ImageBase64 = dto.ImageBase64,
//                ImagePath = dto.ImageName,

//                CreatedBy = "Admin",
//                CreatedAt = DateTime.UtcNow,
//                IsActive = true
//            };

//            _context.Productss.Add(product);
//            await _context.SaveChangesAsync();
//        }

//        // =========================
//        // GET ALL PRODUCTS
//        // =========================
//        public async Task<List<ProductResponseDto>> GetAll()
//        {
//            return await _context.Productss
//                .Where(x => x.IsActive)
//                .Select(x => new ProductResponseDto
//                {
//                    Id = x.Id,
//                    Name = x.Name,
//                    Description = x.Description,
//                    Price = x.Price,
//                    Stock = x.Stock,
//                    ImagePath = x.ImagePath
//                })
//                .ToListAsync();
//        }

//        // =========================
//        // GET PRODUCT BY ID
//        // =========================
//        public async Task<ProductResponseDto?> GetById(int id)
//        {
//            var product = await _context.Productss
//                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

//            if (product == null)
//                return null;

//            return new ProductResponseDto
//            {
//                Id = product.Id,
//                Name = product.Name,
//                Description = product.Description,
//                Price = product.Price,
//                Stock = product.Stock,
//                ImagePath = product.ImagePath
//            };
//        }
//    }
//}















//using Microsoft.EntityFrameworkCore;
//using Product.Application.DTOs;
//using Product.Application.Interfaces;
//using Product.Domain.Entities;
//using Product.Infrastructure.Data;

//using Product.Application.DTOs;
//using Product.Application.Interfaces;
//using Product.Infrastructure.Data;

//public class ProductService : IProductService
//{
//    private readonly ProductDbContext _context;

//    public ProductService(ProductDbContext context)
//    {
//        _context = context;
//    }

//    public async Task Add(CreateProductDto dto)
//    {
//        var product = new Products
//        {
//            Name = dto.Name,
//            Description = dto.Description,
//            Price = dto.Price,
//            Stock = dto.Stock,
//            ImageBase64 = dto.ImageBase64,
//            CreatedAt = DateTime.UtcNow,
//            IsActive = true
//        };

//        _context.Productss.Add(product);
//        await _context.SaveChangesAsync();
//    }

//    public async Task<List<ProductResponseDto>> GetAll()
//    {
//        return await _context.Productss
//            .Where(x => x.IsActive)
//            .Select(x => new ProductResponseDto
//            {
//                Id = x.Id,
//                Name = x.Name,
//                Description = x.Description,
//                Price = x.Price,
//                Stock = x.Stock
//            })
//            .ToListAsync();
//    }

//    public async Task<ProductResponseDto?> GetById(int id)
//    {
//        return await _context.Productss
//            .Where(x => x.Id == id)
//            .Select(x => new ProductResponseDto
//            {
//                Id = x.Id,
//                Name = x.Name,
//                Description = x.Description,
//                Price = x.Price,
//                Stock = x.Stock
//            })
//            .FirstOrDefaultAsync();
//    }
//}



using Microsoft.EntityFrameworkCore;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Domain.Entities;
using Product.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;

namespace Product.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductDbContext _context;
        private readonly IWebHostEnvironment _env;


        public ProductService(
            ProductDbContext context,
            IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================
        // ADD PRODUCT
        // =========================
        public async Task Add(CreateProductDto dto)
        {
            string imagePath = "";

            if (dto.ImageFile != null)
            {
                var uploadsFolder =
                    Path.Combine(
                        _env.WebRootPath,
                        "product-images");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(dto.ImageFile.FileName);

                var filePath =
                    Path.Combine(
                        uploadsFolder,
                        fileName);

                using (var stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }

                imagePath =
                    "/product-images/" + fileName;
            }

            var product = new Products
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,

                ImagePath = imagePath,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                IsActive = true
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();
        }

        // =========================
        // GET ALL PRODUCTS
        // =========================
        public async Task<List<ProductResponseDto>> GetAll()
        {
            return await _context.Products
                .Where(x => x.IsActive)
                .Select(x => new ProductResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Stock = x.Stock,
                    ImagePath =
    "https://localhost:7259" +
    x.ImagePath
                })
                .ToListAsync();
        }

        // =========================
        // GET PRODUCT BY ID
        // =========================
        public async Task<ProductResponseDto?> GetById(int id)
        {
            var product = await _context.Products
                .Where(x => x.Id == id && x.IsActive)
                .Select(x => new ProductResponseDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    Stock = x.Stock,
                    ImagePath =
    "https://localhost:7259" +
    x.ImagePath
                })
                .FirstOrDefaultAsync();

            return product;
        }
    }
}



