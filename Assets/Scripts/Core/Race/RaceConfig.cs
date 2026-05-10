namespace MarbleRace.Core.Race
{
    public class RaceConfig
    {
        public int MarbleCount { get; set; } = 8;
        public float CountdownDuration { get; set; } = 3f;
        public float RaceTimeout { get; set; } = 60f;
        public float MinNudgeInterval { get; set; } = 0.5f;
        public float MaxNudgeInterval { get; set; } = 1.5f;
        public float MinNudgeForce { get; set; } = 0.5f;
        public float MaxNudgeForce { get; set; } = 2f;
        public float BettingDuration { get; set; } = 15f;
    }
}
