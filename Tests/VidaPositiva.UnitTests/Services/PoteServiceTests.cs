using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using VidaPositiva.Api.DTOs.Inputs.Pote;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.Services.PoteService;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.UnitTests.Services;

public class PoteServiceTests
{
    private readonly Pote _initialPote = new()
    {
        Id = 1,
        Name = "Despesas Básicas"
    };
    
    private readonly IPoteService _poteService;
    
    public PoteServiceTests()
    {
        var potes = new List<Pote>
        {
            _initialPote
        }.AsQueryable();
        
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.Dispose());
        
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        
        mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
        
        var mockUserRepository = new Mock<IRepository<Pote>>();
        var mockDbSet = potes.BuildMock().BuildMockDbSet();
        
        mockUserRepository.Setup(r => r.Query()).Returns(mockDbSet.Object);

        _poteService = new PoteService(mockUserRepository.Object, mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetById_WithExistingId_ReturnsPote()
    {
        var poteId = _initialPote.Id;
        
        var pote = await _poteService.GetById(poteId);

        Assert.False(pote.IsNone);
        Assert.True(pote.IsSome);
        Assert.Equal(poteId, pote.Value.Id);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnsNone()
    {
        var poteId = 2;
        
        var pote = await _poteService.GetById(poteId);
        
        Assert.False(pote.IsSome);
        Assert.True(pote.IsNone);
    }

    [Fact]
    public async Task GetAll_ReturnsAllPotes()
    {
        var potes = await _poteService.GetAll();

        Assert.IsType<IList<Pote>>(potes, exactMatch: false);
        Assert.Single(potes);
    }

    [Fact]
    public async Task Create_WithExistingPoteName_ReturnsValidationError()
    {
        var expectedResult = new ValidationError
        {
            Code = "pote_exists",
            HttpCode = 409,
            Message = "O pote já existe."
        };

        var poteDto = new PoteCreationInputDto
        {
            Name = _initialPote.Name
        };

        var result = await _poteService.Create(poteDto);

        Assert.True(result.IsLeft);
        Assert.False(result.IsRight);
        Assert.IsType<ValidationError>(result.Left);
        Assert.Equal(expectedResult, result.Left);
    }

    [Fact]
    public async Task Create_WithNonExistingPoteName_ReturnsPoteId()
    {
        var poteDto = new PoteCreationInputDto
        {
            Name = "Testes Unitários"
        };

        var result = await _poteService.Create(poteDto);

        Assert.False(result.IsLeft);
        Assert.True(result.IsRight);
        Assert.IsType<int>(result.Right);
    }
}