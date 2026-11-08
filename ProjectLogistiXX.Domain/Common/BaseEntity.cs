using ProjectLogistiXX.Domain.Abstraction;

namespace ProjectLogistiXX.Domain.Common
{
    public abstract class BaseEntity
    {
        private readonly List<IDomainEvent> _domainEvents = [];
        public DateTime CreatedAt { get; private set; }

        public DateTime? UpdatedAt { get; private set; }

        public bool IsDeleted { get; protected set; }

        protected BaseEntity()
        {

        }

        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        protected void MarkAsDeleted()
        {
            IsDeleted = true;
            MarkAsUpdated();
        }

        protected virtual void MarkAsUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }


    }
}
