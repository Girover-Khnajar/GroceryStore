using CQRS.Abstractions.Messaging;

namespace CQRS.Infrastructure.Queries;

public sealed record GetAllEntitiesQuery<TReturnType> : IQuery<TReturnType>
{
}