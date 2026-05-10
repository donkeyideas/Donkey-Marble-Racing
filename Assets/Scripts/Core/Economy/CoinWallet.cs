namespace MarbleRace.Core.Economy
{
    public class CoinWallet
    {
        public int Balance { get; private set; }
        public int TotalEarned { get; private set; }
        public int TotalSpent { get; private set; }

        public CoinWallet(int startingBalance)
        {
            Balance = startingBalance;
            TotalEarned = startingBalance;
            TotalSpent = 0;
        }

        public bool CanAfford(int amount)
        {
            return amount > 0 && Balance >= amount;
        }

        public bool Deduct(int amount)
        {
            if (amount <= 0 || Balance < amount)
                return false;

            Balance -= amount;
            TotalSpent += amount;
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Balance += amount;
            TotalEarned += amount;
        }

        public void SetBalance(int amount)
        {
            if (amount < 0) return;
            Balance = amount;
        }
    }
}
