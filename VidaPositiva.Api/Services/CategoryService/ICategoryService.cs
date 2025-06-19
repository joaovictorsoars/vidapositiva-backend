using VidaPositiva.Api.DTOs.Inputs.Category;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.CategoryService;

public interface ICategoryService
{
    Task<Option<Category>> GetById(int id, CancellationToken cancellationToken = default);
    
    Task<IList<Category>> GetAll(CancellationToken cancellationToken = default);
    
    Task<IList<Category>> GetByPoteId(int poteId, int userId, CancellationToken cancellationToken = default);

    Task<Either<ValidationError, int>> Create(CategoryCreationInputDto categoryDto, int userId,
        CancellationToken cancellationToken = default);
}