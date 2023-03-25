namespace Shop.Services.Common.EventBus.Messages.Events
{
    public class IntegrationBaseEvent
    {
        public Guid Id { get; private set; }
        public DateTime CreationDate { get; private set; }

        public IntegrationBaseEvent()
        {
            CreationDate = DateTime.Now;
            Id = Guid.NewGuid();
        }

        public IntegrationBaseEvent(Guid id, DateTime creationDate)
        {
            CreationDate = creationDate;
            Id = id;
        }
    }
}
