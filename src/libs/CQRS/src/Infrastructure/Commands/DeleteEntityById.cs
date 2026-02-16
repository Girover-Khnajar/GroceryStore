using CQRS.Abstractions.Messaging;

namespace CQRS.Infrastructure.Commands;

public record DeleteEntityByIdCommand<TId, TReturnType>(TId Id) : ICommand<TReturnType>
{
}