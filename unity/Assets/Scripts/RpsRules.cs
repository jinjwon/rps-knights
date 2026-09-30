namespace RpsKnights
{
    public enum Sign { Scissors, Rock, Paper }
    public enum RoundOutcome { Draw, Win, Loss }
    public enum DuelMode { Solo, LocalVersus }

    public static class RpsRules
    {
        public static RoundOutcome Resolve(Sign player, Sign opponent)
        {
            if (player == opponent) return RoundOutcome.Draw;

            bool wins =
                (player == Sign.Scissors && opponent == Sign.Paper) ||
                (player == Sign.Rock && opponent == Sign.Scissors) ||
                (player == Sign.Paper && opponent == Sign.Rock);
            return wins ? RoundOutcome.Win : RoundOutcome.Loss;
        }

        public static string Hand(Sign sign) => sign switch
        {
            Sign.Scissors => "✌",
            Sign.Rock => "✊",
            _ => "✋"
        };

        public static string KoreanName(Sign sign) => sign switch
        {
            Sign.Scissors => "가위",
            Sign.Rock => "바위",
            _ => "보"
        };

        public static string Job(Sign sign) => sign switch
        {
            Sign.Scissors => "검 기사",
            Sign.Rock => "해머 기사",
            _ => "방패 기사"
        };
    }
}
