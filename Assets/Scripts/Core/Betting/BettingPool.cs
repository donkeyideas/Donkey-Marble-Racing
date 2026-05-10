using System.Collections.Generic;
using System.Linq;

namespace MarbleRace.Core.Betting
{
    public class BettingPool
    {
        private readonly List<Bet> _bets = new List<Bet>();
        private readonly Dictionary<string, int> _betsPerMarble = new Dictionary<string, int>();

        public IReadOnlyList<Bet> AllBets => _bets;
        public int TotalPool { get; private set; }
        public int BetCount => _bets.Count;

        public void AddBet(Bet bet)
        {
            _bets.Add(bet);
            TotalPool += bet.Amount;

            if (_betsPerMarble.ContainsKey(bet.MarbleId))
                _betsPerMarble[bet.MarbleId] += bet.Amount;
            else
                _betsPerMarble[bet.MarbleId] = bet.Amount;
        }

        public Dictionary<string, int> GetBetsPerMarble()
        {
            return new Dictionary<string, int>(_betsPerMarble);
        }

        public int GetTotalBetOnMarble(string marbleId)
        {
            return _betsPerMarble.ContainsKey(marbleId) ? _betsPerMarble[marbleId] : 0;
        }

        public List<Bet> GetBetsForPlayer(string playerId)
        {
            return _bets.Where(b => b.PlayerId == playerId).ToList();
        }

        public bool HasPlayerBet(string playerId)
        {
            return _bets.Any(b => b.PlayerId == playerId);
        }

        public void Clear()
        {
            _bets.Clear();
            _betsPerMarble.Clear();
            TotalPool = 0;
        }
    }
}
