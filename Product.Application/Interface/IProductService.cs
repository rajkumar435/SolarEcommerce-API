using Product.Application.DTOs;

namespace Product.Application.Interfaces
{
    public interface IProductService
    {
        Task Add(CreateProductDto dto);
        Task<List<ProductResponseDto>> GetAll();
        Task<ProductResponseDto?> GetById(int id);
    }
}