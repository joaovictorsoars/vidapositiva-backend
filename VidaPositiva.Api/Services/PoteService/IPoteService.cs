using VidaPositiva.Api.DTOs.Inputs.Pote;
using VidaPositiva.Api.Entities;
using VidaPositiva.Api.ValueObjects.Common;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Services.PoteService;

public interface IPoteService
{
    Task<Option<Pote>> GetById(int id, CancellationToken cancellationToken = default);

    Task<IList<Pote>> GetAll(CancellationToken cancellationToken = default);

    Task<Either<ValidationError, int>> Create(PoteCreationInputDto poteDto,
        CancellationToken cancellationToken = default);
}