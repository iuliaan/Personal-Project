using ProjectLogistiXX.Domain.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Application.Abstraction.Messaging
{
    public interface ICommandHandler<in TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
    }
}
