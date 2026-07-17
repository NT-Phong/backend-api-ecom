using Ecom.Domain.Common.Interfaces;

namespace Ecom.Domain.Common
{
    public abstract class BaseEntity : IEntity, IAuditableEntity ,IHasConcurrencyStamp, IHasDomainEvents, ISoftDelete
    {
        private readonly List<IDomainEvent> _domainEvents = [];

        public Guid Id { get; protected set; }
        
        public int No { get; set; }

        public Guid ConcurrencyStamp { get; set; } = Guid.NewGuid();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected BaseEntity()
        {
            Id = Guid.NewGuid();
        }

        protected BaseEntity(Guid id)
        {
            Id = id;
        }

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}

