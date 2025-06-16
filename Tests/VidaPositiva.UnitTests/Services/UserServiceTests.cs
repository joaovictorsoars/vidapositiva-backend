using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using VidaPositiva.Api.DTOs.Inputs.User;
using VidaPositiva.Api.DTOs.Outputs.User;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.Services.UserService;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.UnitTests.Services;

public class UserServiceTests
{
    private readonly IUserService _userService;
    
    private static readonly UserCreationInputDto ValidGoogleUserDto = new()
    {
        UserId = "valid-google-user-id",
        Email = "johndoe@example.com",
        Name = "John Doe",
        PictureUrl = "https://example.com/picture",
    };

    private readonly User _existingUser = new()
    {
        Id = 1,
        Name = ValidGoogleUserDto.Name,
        Email = ValidGoogleUserDto.Email,
        GoogleId = ValidGoogleUserDto.UserId,
        PublicId = Guid.NewGuid().ToString(),
        PictureUrl = null,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now,
        LastLogin = DateTime.Now
    };
    
    public UserServiceTests()
    {
        var users = new List<User>
        {
            _existingUser
        }.AsQueryable();
        
        var mockTransaction = new Mock<IDbContextTransaction>();
        mockTransaction.Setup(t => t.Dispose());
        
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        
        mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        mockUnitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
        
        var mockUserRepository = new Mock<IRepository<User>>();
        var mockDbSet = users.BuildMock().BuildMockDbSet();
        
        mockUserRepository.Setup(r => r.Query()).Returns(mockDbSet.Object);

        _userService = new UserService(mockUserRepository.Object, mockUnitOfWork.Object);
    }
    
    [Fact]
    public async Task GetOrCreateByGoogleUserId_WithExistingUser_ReturnsUser()
    {
        var userDto = ValidGoogleUserDto;
        
        var user = await _userService.SignInWithGoogle(userDto);
        
        Assert.IsType<User>(user);
        Assert.NotNull(user);
        Assert.Equal(_existingUser, user);
    }
    
    [Fact]
    public async Task GetOrCreateByGoogleUserId_WithNonExistingUser_ReturnsUser()
    {
        var newUserDto = new UserCreationInputDto
        {
            UserId = "invalid-google-user-id",
            Email = "janedoe@example.com",
            Name = "Jane Doe",
            PictureUrl = "https://example.com/picture",
        };
        
        var user = await _userService.SignInWithGoogle(newUserDto);
        
        Assert.IsType<User>(user);
        Assert.NotNull(user);
        Assert.NotEqual(_existingUser, user);
        Assert.Equal(newUserDto.UserId, user.GoogleId);
        Assert.Equal(newUserDto.Email, user.Email);
        Assert.Equal(newUserDto.Name, user.Name);
        Assert.Equal(newUserDto.PictureUrl, user.PictureUrl);
    }

    [Fact]
    public async Task GetByGoogleUserId_WithExistingUser_ReturnsUser()
    {
        var userId = _existingUser.GoogleId;
        var expectedResult = new UserInfoOutputDto
        {
            Name = _existingUser.Name,
            Email = _existingUser.Email,
            PictureUrl = _existingUser.PictureUrl,
            PublicId = _existingUser.PublicId
        };

        var user = await _userService.GetByGoogleUserId(userId!);
        
        Assert.False(user.IsLeft);
        Assert.True(user.IsRight);
        Assert.IsType<UserInfoOutputDto>(user.Right);
        Assert.NotNull(user.Right);
        Assert.Equal(expectedResult, user.Right);
    }

    [Fact]
    public async Task GetByGoogleUserId_WithNonExistingUser_ReturnsValidationError()
    {
        var userId = "invalid-google-user-id";
        var expectedResult = new ValidationError
        {
            Code = "user_not_found",
            HttpCode = 404,
            Message = "User not found!"
        };

        var user = await _userService.GetByGoogleUserId(userId);
        
        Assert.True(user.IsLeft);
        Assert.False(user.IsRight);
        Assert.IsType<ValidationError>(user.Left);
        Assert.NotNull(user.Left);
        Assert.Equal(expectedResult, user.Left);
    }
}