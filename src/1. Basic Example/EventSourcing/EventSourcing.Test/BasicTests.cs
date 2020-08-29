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
        public async Task GivenAnEmptyAccount_WhenIApplyDeposit_ThenAmountIsIncreased()
        {
            var id = "123";
            var account = new BankAccount(id);
            await repository.LoadAsync(account);
            account.Deposit(10);
            await repository.SaveAsync(account);

            Assert.AreEqual(10, account.Amount, "The amount is incorrect");
        }

        [Test]
        public async Task GivenTwoUncommittedAggregates_WhenSaveTheStream_ThenAConcurrencyExceptionIsThrown()
        {
            const string id = "123";
            var account1 = new BankAccount(id);
            var account2 = new BankAccount(id);
            await repository.LoadAsync(account1);
            await repository.LoadAsync(account2);
            account1.Deposit(10);
            account2.Deposit(20);
            await repository.SaveAsync(account1);

            Assert.ThrowsAsync<StreamConcurrencyException>(async () =>
            {
                await repository.SaveAsync(account2);
            }, "Should throw a ConcurrencyException");
        }
    }
}