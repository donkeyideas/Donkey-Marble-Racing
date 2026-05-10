using System.Collections.Generic;

namespace MarbleRace.Core.Race
{
    public class RaceResult
    {
        public string WinnerMarbleId { get; }
        public List<string> FinishOrder { get; }
        public float RaceDuration { get; }
        public bool WasTimeout { get; }

        public RaceResult(string winnerMarbleId, List<string> finishOrder, float raceDuration, bool wasTimeout)
        {
            WinnerMarbleId = winnerMarbleId;
            FinishOrder = finishOrder;
            RaceDuration = raceDuration;
            WasTimeout = wasTimeout;
        }

        public int GetPosition(string marbleId)
        {
            int index = FinishOrder.IndexOf(marbleId);
            return index >= 0 ? index + 1 : -1;
        }
    }
}
