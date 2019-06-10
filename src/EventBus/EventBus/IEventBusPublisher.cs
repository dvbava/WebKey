namespace EventBus
{
    public interface IEventBusPublisher
    {
        void PublishEvent<T>(T toPublish);
    }
}
