namespace MarbleRace.Core.Betting
{
    public class Bet
    {
        public string PlayerId { get; }
        public string MarbleId { get; }
        public int Amount { get; }
        public long Timestamp { get; }

        public Bet(string playerId, string marbleId, int amount, long timestamp)
        {
            PlayerId = playerId;
            MarbleId = marbleId;
            Amount = amount;
            Timestamp = timestamp;
        }
    }
}
