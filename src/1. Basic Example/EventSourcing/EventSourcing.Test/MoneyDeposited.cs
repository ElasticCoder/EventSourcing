namespace EventSourcing.Test
{
    public class MoneyDeposited
    {
        public MoneyDeposited(decimal amount)
        {
            this.Amount = amount;
        }

        public decimal Amount { get; private set; }
    }
}
