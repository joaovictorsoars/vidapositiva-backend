using VidaPositiva.Api.DTOs.Inputs.User;
using VidaPositiva.Api.DTOs.Outputs.User;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.UserService;

public interface IUserService
{
    Task<User> SignInWithGoogle(UserCreationInputDto userDto, CancellationToken cancellationToken = default);
    Task<Option<User>> GetUserByEmail(string? email, CancellationToken cancellationToken = default);

    Task<Either<ValidationError, UserInfoOutputDto>> GetByGoogleUserId(string? userId, CancellationToken cancellationToken = default);
}