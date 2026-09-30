using NUnit.Framework;

namespace RpsKnights.Tests
{
    public class RpsRulesTests
    {
        [TestCase(Sign.Scissors, Sign.Paper)]
        [TestCase(Sign.Rock, Sign.Scissors)]
        [TestCase(Sign.Paper, Sign.Rock)]
        public void WinningPairsReturnWin(Sign player, Sign opponent)
        {
            Assert.AreEqual(RoundOutcome.Win, RpsRules.Resolve(player, opponent));
        }

        [TestCase(Sign.Scissors)]
        [TestCase(Sign.Rock)]
        [TestCase(Sign.Paper)]
        public void SameSignsDraw(Sign sign)
        {
            Assert.AreEqual(RoundOutcome.Draw, RpsRules.Resolve(sign, sign));
        }
    }
}
