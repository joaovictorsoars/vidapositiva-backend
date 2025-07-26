using VidaPositiva.Api.DTOs.Inputs.Category;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.CategoryService;

public interface ICategoryService
{
    Task<Either<ValidationError, Category>> GetById(int id, int userId, CancellationToken cancellationToken = default);

    Task<Either<ValidationError, Category>> GetSubCategoryById(int id, int userId,
        CancellationToken cancellationToken = default);
    
    Task<IList<Category>> GetAll(CancellationToken cancellationToken = default);
    
    Task<IList<Category>> GetByPoteId(int poteId, int userId, CancellationToken cancellationToken = default);

    Task<IList<Category>> GetByParentCategoryId(int parentCategoryId, int userId,
        CancellationToken cancellationToken = default);

    Task<Either<ValidationError, int>> Edit(CategoryEditInputDto categoryDto, int userId,
        CancellationToken cancellationToken = default);
    
    Task<Either<ValidationError, int>> Create(CategoryCreationInputDto categoryDto, int userId,
        CancellationToken cancellationToken = default);

    Task<Category[]> BulkCreateByName(CategoryCreationByNameInputDto[] categoryNames, int userId,
        CancellationToken cancellationToken = default);
    
    Task<Either<ValidationError, int>> ToggleFavoriteCategory(int categoryId, int userId, CancellationToken cancellationToken = default);
}