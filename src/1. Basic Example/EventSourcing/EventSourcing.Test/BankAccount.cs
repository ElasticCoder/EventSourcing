namespace EventSourcing.Test
{
    using EventSourcing;

    public class BankAccount : Aggregate
    {
        public decimal Amount { get; private set; }

        public BankAccount(string id) : base (id)
        {
        }

        #region These methods execute commands against the aggregate

        public void Deposit(decimal amount)
        {
            this.Apply(new MoneyDeposited(amount));
        }

        #endregion

        #region These methods update the aggregate state from events

        protected void UpdateState(MoneyDeposited deposited)
        {
            this.Amount = deposited.Amount;
        }

        #endregion
    }
}
