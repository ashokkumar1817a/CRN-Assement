using AutoMapper;
using Microsoft.Extensions.Logging;
using ProductApi.Application.DTOs;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Domain.Exceptions;

namespace ProductApi.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(
            query.PageNumber, query.PageSize, query.Search, cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(items),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDto> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdWithItemsAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, string createdBy, CancellationToken cancellationToken = default)
    {
        var product = _mapper.Map<Product>(dto);
        product.CreatedBy = createdBy;
        product.CreatedOn = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} created by {CreatedBy}", product.Id, createdBy);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto dto, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.ProductName = dto.ProductName;
        product.ModifiedBy = modifiedBy;
        product.ModifiedOn = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} updated by {ModifiedBy}", product.Id, modifiedBy);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} deleted", id);
    }

    public async Task<IReadOnlyList<ItemDto>> GetItemsForProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), productId);

        var items = await _unitOfWork.Items.GetByProductIdAsync(productId, cancellationToken);
        return _mapper.Map<List<ItemDto>>(items);
    }

    public async Task<ItemDto> AddItemToProductAsync(int productId, CreateItemDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), productId);

        var item = _mapper.Map<Item>(dto);
        item.ProductId = product.Id;

        await _unitOfWork.Items.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ItemDto>(item);
    }
}
