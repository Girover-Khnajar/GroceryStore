using CQRS.Abstractions.Messaging;

namespace CQRS.Infrastructure.Queries;

public sealed record GetEntityByIdQuery<TId, TReturnType>(TId Id) : IQuery<TReturnType>
{
}