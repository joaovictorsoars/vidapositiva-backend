using Microsoft.EntityFrameworkCore;
using VidaPositiva.Api.DTOs.Inputs.User;
using VidaPositiva.Api.DTOs.Outputs.User;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.UserService;

public class UserService(
    IRepository<User> userRepository,
    IUnitOfWork unitOfWork) : IUserService
{
    public async Task<User> SignInWithGoogle(UserCreationInputDto userDto, CancellationToken cancellationToken = default)
    {
        var user = await userRepository
            .Query()
            .FirstOrDefaultAsync(user => user.GoogleId == userDto.UserId, cancellationToken);

        if (user is null)
            return await CreateUserByGoogleUserId(userDto, cancellationToken);

        return await UpdateLastLoginByGoogleUserId(user, cancellationToken);
    }

    public async Task<Either<ValidationError, UserInfoOutputDto>> GetByGoogleUserId(string? userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository
            .Query()
            .FirstOrDefaultAsync(user => user.GoogleId == userId, cancellationToken);
        
        if (user is null)
            return Either<ValidationError, UserInfoOutputDto>.FromLeft(new ValidationError
            {
                Code = "user_not_found",
                HttpCode = 404,
                Message = "User not found!"
            });
        
        return Either<ValidationError, UserInfoOutputDto>.FromRight(new UserInfoOutputDto
        {
            Name = user.Name,
            Email = user.Email,
            PictureUrl = user.PictureUrl,
            PublicId = user.PublicId
        });
    }

    public async Task<Option<User>> GetUserByEmail(string? email, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.Query().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return Option<User>.FromNullable(user);
    }

    private async Task<User> CreateUserByGoogleUserId(UserCreationInputDto userDto, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        var user = new User
        {
            Name = userDto.Name,
            Email = userDto.Email,
            LastLogin = now,
            GoogleId = userDto.UserId,
            PublicId = Guid.NewGuid().ToString(),
            PictureUrl = userDto.PictureUrl is { Length: > 0 } ?  userDto.PictureUrl : null,
            CreatedAt = now,
            UpdatedAt = now,
        };
        
        var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        userRepository.Add(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return user;
    }

    private async Task<User> UpdateLastLoginByGoogleUserId(User user,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        user.LastLogin = now;
        
        var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        userRepository.Update(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return user;
    }
}