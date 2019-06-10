using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EventBus.Test
{
    public class EventBusTest
    {
        [Fact]
        public async Task ConnectOneTest()
        {
            EventBus eventBus = new EventBus();
            IEventBus subscriber = eventBus;
            IEventBusPublisher publisher = eventBus;

            DummyEventDataOne dummyEventDataOne = null;
            subscriber.Connect<DummyEventDataOne>((ev) => { dummyEventDataOne = ev; });

            DummyEventDataTwo dummyEventDataTwo = null;
            subscriber.Connect<DummyEventDataTwo>((ev) => { dummyEventDataTwo = ev; });

            await Task.Run(async () =>
            {
                publisher.PublishEvent<DummyEventDataOne>(new DummyEventDataOne { Id = 10 });
                await Task.Delay(50);
            });           

            Assert.NotNull(dummyEventDataOne);
            Assert.Null(dummyEventDataTwo);
        }

        [Fact]
        public async Task ConnectTwoTest()
        {
            EventBus eventBus = new EventBus();
            IEventBus subscriber = eventBus;
            IEventBusPublisher publisher = eventBus;

            DummyEventDataOne dummyEventDataOne = null;
            subscriber.Connect<DummyEventDataOne>((ev) => { dummyEventDataOne = ev; });

            DummyEventDataTwo dummyEventDataTwo = null;
            subscriber.Connect<DummyEventDataTwo>((ev) => { dummyEventDataTwo = ev; });           

            await Task.Run(async () =>
            {
                publisher.PublishEvent<DummyEventDataTwo>(new DummyEventDataTwo { Id = 10 });
                await Task.Delay(50);
            });

            Assert.Null(dummyEventDataOne);
            Assert.NotNull(dummyEventDataTwo);
        }

        [Fact]
        public async Task ConnectBothWithDataTest()
        {
            EventBus eventBus = new EventBus();
            IEventBus subscriber = eventBus;
            IEventBusPublisher publisher = eventBus;

            DummyEventDataOne dummyEventDataOne = null;
            subscriber.Connect<DummyEventDataOne>((ev) => { dummyEventDataOne = ev; });

            DummyEventDataTwo dummyEventDataTwo = null;
            subscriber.Connect<DummyEventDataTwo>((ev) => { dummyEventDataTwo = ev; });           

            await Task.Run(async () =>
            {
                publisher.PublishEvent<DummyEventDataOne>(new DummyEventDataOne { Id = 10 });
                publisher.PublishEvent<DummyEventDataTwo>(new DummyEventDataTwo { Id = 11 });
                await Task.Delay(50);
            });

            Assert.NotNull(dummyEventDataOne);
            Assert.NotNull(dummyEventDataTwo);

            Assert.Equal(10, dummyEventDataOne.Id);
            Assert.Equal(11, dummyEventDataTwo.Id);
        }

        class DummyEventDataOne
        {
            public int Id { get; set; }
        }

        class DummyEventDataTwo
        {
            public int Id { get; set; }
        }
    }
}
