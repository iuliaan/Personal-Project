using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectLogistiXX.Domain.Abstraction
{
    public class AggregateRoot : Entity
    {
        private readonly List<IDomainEvent> _domainEvents = [];
        protected AggregateRoot(Guid id) : base(id) { }
        protected AggregateRoot() { }
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            _domainEvents.Add(domainEvent);
        }
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
