using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using VidaPositiva.Api.DTOs.Inputs.Category;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.Services.CategoryService;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly ICategoryService _categoryService;
    
    public CategoryServiceTests()
    {
        var categories = new List<Category>
        {
            new()
            {
                Id = 1,
                Name = "Energia",
                Description = "Uma categoria básica",
                PoteId = 1,
            },
            new()
            {
                Id = 2,
                Name = "Roupas",
                Description = "Uma categoria profissional",
                PoteId = 2,
            }
        }.AsQueryable();
        
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.Dispose());
        
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        
        mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
        
        var mockUserRepository = new Mock<IRepository<Category>>();
        var mockDbSet = categories.BuildMock().BuildMockDbSet();
        
        mockUserRepository.Setup(r => r.Query()).Returns(mockDbSet.Object);

        _categoryService = new CategoryService(mockUserRepository.Object, mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsCategory()
    {
        var expectedResult = new Category
        {
            Id = 1,
            Name = "Energia",
            Description = "Uma categoria básica",
            PoteId = 1,
        };
        
        var result = await _categoryService.GetById(expectedResult.Id);

        Assert.False(result.IsLeft);
        Assert.True(result.IsRight);
        Assert.Equivalent(expectedResult, result.Right!);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsValidationError()
    {
        var poteId = 3;
        
        var result = await _categoryService.GetById(poteId);
        
        Assert.False(result.IsRight);
        Assert.True(result.IsLeft);
        Assert.IsType<ValidationError>(result.Left);
    }

    [Fact]
    public async Task GetAll_ReturnsAllCategories()
    {
        var expectedResult = new List<Category>
        {
            new()
            {
                Id = 1,
                Name = "Energia",
                Description = "Uma categoria básica",
                PoteId = 1,
            },
            new()
            {
                Id = 2,
                Name = "Roupas",
                Description = "Uma categoria profissional",
                PoteId = 2,
            }
        };
        
        var categories = await _categoryService.GetAll();

        Assert.IsType<IList<Category>>(categories, exactMatch: false);
        Assert.Equivalent(expectedResult, categories);
    }

    [Fact]
    public async Task Create_WithExistingPoteId_ReturnsCategory()
    {
        var userId = 1;
        var categoryDto = new CategoryCreationInputDto
        {
            Name = "Nova categoria",
            Description = "Descrição",
            PoteId = 1,
        };

        var result = await _categoryService.Create(categoryDto, userId);

        Assert.False(result.IsLeft);
        Assert.True(result.IsRight);
        Assert.IsType<int>(result.Right);
    }
}