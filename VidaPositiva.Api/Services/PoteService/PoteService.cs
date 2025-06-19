using Microsoft.EntityFrameworkCore;
using VidaPositiva.Api.DTOs.Inputs.Pote;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.Extensions.String;
using VidaPositiva.Api.Persistence.Repository;
using VidaPositiva.Api.Persistence.UnitOfWork;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.PoteService;

public class PoteService(
    IRepository<Pote> poteRepository,
    IUnitOfWork unitOfWork) : IPoteService
{
    public async Task<Option<Pote>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var pote = await poteRepository.Query().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        
        return Option<Pote>.FromNullable(pote);
    }

    public async Task<IList<Pote>> GetAll(CancellationToken cancellationToken = default)
    { 
        return await poteRepository.Query().OrderBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public async Task<Either<ValidationError, int>> Create(PoteCreationInputDto poteDto, CancellationToken cancellationToken = default)
    {
        var normalizedDto = new PoteCreationInputDto
        {
            Name = poteDto.Name.NormalizeWhitespaces(),
            PictureUrl = poteDto.PictureUrl?.NormalizeWhitespaces(),
        };
        
        var poteExists =  await poteRepository.Query().AnyAsync(p => p.Name.ToUpper() == normalizedDto.Name.ToUpper(), cancellationToken);
        
        if (poteExists)
            return Either<ValidationError, int>.FromLeft(new ValidationError
            {
                Code = "pote_exists",
                HttpCode = 409,
                Message = "O pote já existe."
            });

        var pote = Pote.FromDto(normalizedDto);
        
        var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        poteRepository.Add(pote);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Either<ValidationError, int>.FromRight(pote.Id);
    }
}