using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Application.Abstraction.Messaging
{
    public interface IQueryHandler<in TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
    }
}
