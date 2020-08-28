using System.Threading.Tasks;
using NUnit.Framework;
using SimpleEventStore;
using SimpleEventStore.InMemory;

namespace EventSourcing.Test
{
    public class Tests
    {
        private DomainRepository repository;
        private EventStore eventStore;

        [SetUp]
        public void Setup()
        {
            eventStore = new EventStore(new InMemoryStorageEngine());
            repository = new DomainRepository(eventStore);
        }

        [Test]
        public async Task Test1()
        {
            var id = "123";
            var account = new BankAccount(id);
            await repository.LoadAsync(account);
            account.Deposit(10);
            await repository.SaveAsync(account);

            Assert.AreEqual(10, account.Amount, "The amount is incorrect");
        }
    }
}