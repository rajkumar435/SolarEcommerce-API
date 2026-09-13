using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Product.Application.DTOs;
using Product.Application.Interface;
using Product.Application.Interfaces;
using Product.Domain.Entities;
using Product.Infrastructure.Data;

namespace Product.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ProductDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IRedisCacheService _cache;
        private readonly ILogger<ProductService> _logger;

        // Cache duration
        private static readonly TimeSpan CacheDuration =
            TimeSpan.FromMinutes(10);

        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public ProductService(
            ProductDbContext context,
            IWebHostEnvironment env,
            IRedisCacheService cache,
            ILogger<ProductService> logger)
        {
            _context = context;
            _env = env;
            _cache = cache;
            _logger = logger;
        }

        // =====================================================
        // CACHE KEYS
        // =====================================================

        private const string AllProductsCacheKey =
            "products:all";

        private static string ProductCacheKey(int id)
        {
            return "product:" + id;
        }

        // =====================================================
        // ADD PRODUCT
        // =====================================================

        public async Task Add(CreateProductDto dto)
        {
            try
            {
                _logger.LogInformation(
                    "Starting product creation for product {ProductName}",
                    dto.Name);

                string imagePath = "";

                // -------------------------------------------------
                // IMAGE UPLOAD
                // -------------------------------------------------

                if (dto.ImageFile != null)
                {
                    _logger.LogInformation(
                        "Uploading image for product {ProductName}",
                        dto.Name);

                    var uploadsFolder =
                        Path.Combine(
                            _env.WebRootPath,
                            "product-images");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(
                            uploadsFolder);

                        _logger.LogInformation(
                            "Created product image directory {ImageDirectory}",
                            uploadsFolder);
                    }

                    var fileName =
                        Guid.NewGuid() +
                        Path.GetExtension(
                            dto.ImageFile.FileName);

                    var filePath =
                        Path.Combine(
                            uploadsFolder,
                            fileName);

                    using (var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create))
                    {
                        await dto.ImageFile
                            .CopyToAsync(stream);
                    }

                    imagePath =
                        "/product-images/" +
                        fileName;

                    _logger.LogInformation(
                        "Product image uploaded successfully for product {ProductName}",
                        dto.Name);
                }

                // -------------------------------------------------
                // CREATE PRODUCT
                // -------------------------------------------------

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

                _logger.LogInformation(
                    "Product created successfully with ProductId {ProductId}",
                    product.Id);

                // -------------------------------------------------
                // CACHE INVALIDATION
                // -------------------------------------------------

                await _cache.RemoveAsync(
                    AllProductsCacheKey);

                _logger.LogInformation(
                    "Product list cache invalidated after creating ProductId {ProductId}",
                    product.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while creating product {ProductName}",
                    dto.Name);

                throw;
            }
        }

        // =====================================================
        // GET ALL PRODUCTS
        // =====================================================

        public async Task<List<ProductResponseDto>> GetAll()
        {
            try
            {
                const string cacheKey =
                    AllProductsCacheKey;

                _logger.LogInformation(
                    "Getting all products");

                // -------------------------------------------------
                // CHECK REDIS
                // -------------------------------------------------

                var cachedProducts =
                    await _cache
                        .GetAsync<List<ProductResponseDto>>(
                            cacheKey);

                // -------------------------------------------------
                // CACHE HIT
                // -------------------------------------------------

                if (cachedProducts != null)
                {
                    _logger.LogInformation(
                        "Redis Cache HIT for key {CacheKey}. Product count: {ProductCount}",
                        cacheKey,
                        cachedProducts.Count);

                    return cachedProducts;
                }

                // -------------------------------------------------
                // CACHE MISS
                // -------------------------------------------------

                _logger.LogInformation(
                    "Redis Cache MISS for key {CacheKey}",
                    cacheKey);

                // -------------------------------------------------
                // GET FROM DATABASE
                // -------------------------------------------------

                _logger.LogInformation(
                    "Fetching products from database");

                var products =
                    await _context.Products
                        .Where(x => x.IsActive)
                        .Select(x =>
                            new ProductResponseDto
                            {
                                Id = x.Id,

                                Name = x.Name,

                                Description =
                                    x.Description,

                                Price = x.Price,

                                Stock = x.Stock,

                                ImagePath =
                                    x.ImagePath
                            })
                        .ToListAsync();

                _logger.LogInformation(
                    "Retrieved {ProductCount} products from database",
                    products.Count);

                // -------------------------------------------------
                // STORE IN REDIS
                // -------------------------------------------------

                await _cache.SetAsync(
                    cacheKey,
                    products,
                    CacheDuration);

                _logger.LogInformation(
                    "Stored {ProductCount} products in Redis with key {CacheKey}. Cache duration: {CacheDurationMinutes} minutes",
                    products.Count,
                    cacheKey,
                    CacheDuration.TotalMinutes);

                // -------------------------------------------------
                // RETURN
                // -------------------------------------------------

                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while getting all products");

                throw;
            }
        }

        // =====================================================
        // GET PRODUCT BY ID
        // =====================================================

        public async Task<ProductResponseDto?> GetById(
            int id)
        {
            try
            {
                string cacheKey =
                    ProductCacheKey(id);

                _logger.LogInformation(
                    "Getting product {ProductId}",
                    id);

                // -------------------------------------------------
                // CHECK REDIS
                // -------------------------------------------------

                var cachedProduct =
                    await _cache
                        .GetAsync<ProductResponseDto>(
                            cacheKey);

                // -------------------------------------------------
                // CACHE HIT
                // -------------------------------------------------

                if (cachedProduct != null)
                {
                    _logger.LogInformation(
                        "Redis Cache HIT for key {CacheKey}",
                        cacheKey);

                    return cachedProduct;
                }

                // -------------------------------------------------
                // CACHE MISS
                // -------------------------------------------------

                _logger.LogInformation(
                    "Redis Cache MISS for key {CacheKey}",
                    cacheKey);

                // -------------------------------------------------
                // GET FROM DATABASE
                // -------------------------------------------------

                _logger.LogInformation(
                    "Fetching ProductId {ProductId} from database",
                    id);

                var product =
                    await _context.Products
                        .Where(x =>
                            x.Id == id &&
                            x.IsActive)
                        .Select(x =>
                            new ProductResponseDto
                            {
                                Id = x.Id,

                                Name = x.Name,

                                Description =
                                    x.Description,

                                Price = x.Price,

                                Stock = x.Stock,

                                ImagePath =
                                    x.ImagePath
                            })
                        .FirstOrDefaultAsync();

                // -------------------------------------------------
                // PRODUCT NOT FOUND
                // -------------------------------------------------

                if (product == null)
                {
                    _logger.LogWarning(
                        "ProductId {ProductId} was not found",
                        id);

                    return null;
                }

                // -------------------------------------------------
                // STORE IN REDIS
                // -------------------------------------------------

                await _cache.SetAsync(
                    cacheKey,
                    product,
                    CacheDuration);

                _logger.LogInformation(
                    "ProductId {ProductId} stored in Redis with key {CacheKey}",
                    id,
                    cacheKey);

                // -------------------------------------------------
                // RETURN
                // -------------------------------------------------

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while getting ProductId {ProductId}",
                    id);

                throw;
            }
        }
    }
}