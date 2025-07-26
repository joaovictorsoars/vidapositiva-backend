using Microsoft.EntityFrameworkCore;
using VidaPositiva.Api.DTOs.Inputs.Category;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.Extensions.String;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.CategoryService;

public class CategoryService(
    IRepository<Category> repository,
    IUnitOfWork unitOfWork) : ICategoryService
{
    public async Task<Either<ValidationError, Category>> GetById(int id, int userId, CancellationToken cancellationToken = default)
    {
        var category = await repository
            .Query()
            .Include(c => c.Pote)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.ParentId == null, cancellationToken);

        if (category == null)
        {
            return Either<ValidationError, Category>.FromLeft(new ValidationError
            {
                Code = "category_not_found",
                HttpCode = 404,
                Message = "Categoria não encontrada."
            });
        }
        
        return Either<ValidationError, Category>.FromRight(category);
    }
    
    public async Task<Either<ValidationError, Category>> GetSubCategoryById(int id, int userId, CancellationToken cancellationToken = default)
    {
        var category = await repository
            .Query()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && p.PoteId == null, cancellationToken);

        if (category == null)
        {
            return Either<ValidationError, Category>.FromLeft(new ValidationError
            {
                Code = "category_not_found",
                HttpCode = 404,
                Message = "Categoria não encontrada."
            });
        }
        
        return Either<ValidationError, Category>.FromRight(category);
    }

    public async Task<IList<Category>> GetAll(CancellationToken cancellationToken = default)
    { 
        return await repository.Query().OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task<IList<Category>> GetByPoteId(int poteId, int userId, CancellationToken cancellationToken = default)
    {
        return await repository.Query()
            .Where(c => c.PoteId == poteId && c.ParentId == null && (c.UserId == null || c.UserId == userId))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IList<Category>> GetByParentCategoryId(int parentCategoryId, int userId, CancellationToken cancellationToken = default)
    {
        return await repository.Query()
            .Where(c => c.ParentId == parentCategoryId && (c.UserId == null || c.UserId == userId))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Either<ValidationError, int>> Edit(CategoryEditInputDto categoryDto, int userId, CancellationToken cancellationToken = default)
    {
        var normalizedDto = categoryDto with { Name = categoryDto.Name.NormalizeWhitespaces(), Description = categoryDto.Description?.NormalizeWhitespaces() };
        
        var category = await repository.Query().FirstOrDefaultAsync(c => c.Id == categoryDto.CategoryId && c.UserId == userId, cancellationToken);
        
        if (category == null)
            return Either<ValidationError, int>.FromLeft(new ValidationError
            {
                Code = "category_not_found",
                HttpCode = 404,
                Message = "A categoria não foi encontrada."
            });
        
        var categoryExists =  await repository
            .Query()
            .AnyAsync(c => c.Name.ToUpper() == normalizedDto.Name.ToUpper() && c.UserId == userId && c.Id != normalizedDto.CategoryId, cancellationToken);
        
        if (categoryExists)
            return Either<ValidationError, int>.FromLeft(new FieldValidationError
            {
                HttpCode = 409,
                FieldErrors = [
                    new FieldError 
                    { 
                        Field = "name", 
                        Message = "Essa categoria já existe. Escolha outro nome."
                    }]
            });
        
        category.Name = normalizedDto.Name;
        category.Description = normalizedDto.Description;
        
        var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        repository.Update(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Either<ValidationError, int>.FromRight(category.Id);
    }

    public async Task<Either<ValidationError, int>> Create(CategoryCreationInputDto categoryDto, int userId, CancellationToken cancellationToken = default)
    {
        var normalizedDto = categoryDto with { Name = categoryDto.Name.NormalizeWhitespaces(), Description = categoryDto.Description?.NormalizeWhitespaces() };

        if (normalizedDto.PoteId == null && normalizedDto.ParentCategoryId == null)
        {
            return Either<ValidationError, int>.FromLeft(new ValidationError
            {
                Code = "category_without_pote_or_parent",
                HttpCode = 400,
                Message = "A categoria deve pertencer a um pote ou a uma categoria pai."
            });
        }
        
        var categoryExists =  await repository
            .Query()
            .AnyAsync(p => p.Name.ToUpper() == normalizedDto.Name.ToUpper() && p.UserId == userId && ((p.PoteId != null && p.PoteId == normalizedDto.PoteId) || (p.ParentId != null && p.ParentId == normalizedDto.ParentCategoryId)), cancellationToken);
        
        if (categoryExists)
            return Either<ValidationError, int>.FromLeft(new FieldValidationError
            {
                HttpCode = 409,
                FieldErrors = [
                    new FieldError 
                    { 
                        Field = "name", 
                        Message = "Essa categoria já existe. Escolha outro nome."
                    }]
            });

        var pote = Category.FromDto(normalizedDto, userId);
        
        var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        await repository.AddAsync(pote, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Either<ValidationError, int>.FromRight(pote.Id);
    }

    public async Task<Category[]> BulkCreateByName(CategoryCreationByNameInputDto[] categoryNames, int userId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var categoriesToAdd = categoryNames.Select(c => new Category
        {
            Name = c.CategoryName,
            UserId = userId,
            Description = null,
            IsFavorite = false,
            PoteId = c.PoteId,
            ParentId = c.ParentCategoryId
        }).ToArray();
        
        await repository.AddRangeAsync(categoriesToAdd, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return categoriesToAdd;
    }

    public async Task<Either<ValidationError, int>> ToggleFavoriteCategory(int categoryId, int userId, CancellationToken cancellationToken = default)
    {
        var category = await repository.Query().FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);

        if (category is null)
        {
            return Either<ValidationError, int>.FromLeft(new ValidationError
            {
                Code = "category_not_found",
                HttpCode = 404,
                Message = "Categoria não encontrada para esse usuário!"
            });
        }
        
        category.IsFavorite = !category.IsFavorite;
        

        var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        repository.Update(category);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        
        return Either<ValidationError, int>.FromRight(category.Id);
    }
}