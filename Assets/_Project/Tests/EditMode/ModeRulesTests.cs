using System.Collections.Generic;
using GraceRun.Run;
using NUnit.Framework;

namespace GraceRun.Tests
{
    public class ModeRulesTests
    {
        [Test]
        public void SelectionListIsRunThenBossesThenArena()
        {
            Assert.AreEqual(7, ModeRules.OptionCount(5));
            Assert.AreEqual(GameMode.Run, ModeRules.ModeOf(0, 5));
            Assert.AreEqual(GameMode.Practice, ModeRules.ModeOf(1, 5));
            Assert.AreEqual(GameMode.Practice, ModeRules.ModeOf(5, 5));
            Assert.AreEqual(GameMode.Arena, ModeRules.ModeOf(6, 5));
            Assert.AreEqual(-1, ModeRules.BossIndexOf(0, 5));
            Assert.AreEqual(0, ModeRules.BossIndexOf(1, 5));
            Assert.AreEqual(4, ModeRules.BossIndexOf(5, 5));
            Assert.AreEqual(-1, ModeRules.BossIndexOf(6, 5));
        }

        [Test]
        public void StepWrapsBothWays()
        {
            Assert.AreEqual(1, ModeRules.Step(0, 1, 5));
            Assert.AreEqual(0, ModeRules.Step(6, 1, 5));
            Assert.AreEqual(6, ModeRules.Step(0, -1, 5));
        }

        [Test]
        public void NoBossesLeavesRunAndArena()
        {
            Assert.AreEqual(GameMode.Run, ModeRules.ModeOf(0, 0));
            Assert.AreEqual(GameMode.Arena, ModeRules.ModeOf(1, 0));
            Assert.AreEqual(0, ModeRules.Step(1, 1, 0));
        }
    }

    public class ArenaRulesTests
    {
        [Test]
        public void TeamsAreSlotParity()
        {
            Assert.AreEqual(0, ArenaRules.TeamOf(0));
            Assert.AreEqual(1, ArenaRules.TeamOf(1));
            Assert.AreEqual(0, ArenaRules.TeamOf(2));
            Assert.AreEqual(1, ArenaRules.TeamOf(3));
            Assert.IsTrue(ArenaRules.IsOpposing(0, 1));
            Assert.IsFalse(ArenaRules.IsOpposing(0, 2));
        }

        [Test]
        public void MatchNeedsPlayersOnBothTeams()
        {
            Assert.IsTrue(ArenaRules.HasBothTeams(new List<int> { 0, 1 }));
            Assert.IsFalse(ArenaRules.HasBothTeams(new List<int> { 0, 2 }));
            Assert.IsFalse(ArenaRules.HasBothTeams(new List<int>()));
        }

        [Test]
        public void RoundEndsWhenATeamHasNobodyUp()
        {
            Assert.AreEqual(ArenaRound.Undecided, ArenaRules.RoundResult(2, 1));
            Assert.AreEqual(ArenaRound.TeamA, ArenaRules.RoundResult(1, 0));
            Assert.AreEqual(ArenaRound.TeamB, ArenaRules.RoundResult(0, 2));
            Assert.AreEqual(ArenaRound.Draw, ArenaRules.RoundResult(0, 0));
        }

        [Test]
        public void MatchGoesToTheFirstTeamWithEnoughWins()
        {
            Assert.AreEqual(-1, ArenaRules.MatchWinner(2, 2, 3));
            Assert.AreEqual(0, ArenaRules.MatchWinner(3, 1, 3));
            Assert.AreEqual(1, ArenaRules.MatchWinner(0, 3, 3));
        }

        [Test]
        public void TeamsStartOnOppositeSides()
        {
            Assert.AreEqual(-7f, ArenaRules.SpawnX(0, 7f, 2.5f));
            Assert.AreEqual(7f, ArenaRules.SpawnX(1, 7f, 2.5f));
            Assert.AreEqual(-9.5f, ArenaRules.SpawnX(2, 7f, 2.5f));
            Assert.AreEqual(9.5f, ArenaRules.SpawnX(3, 7f, 2.5f));
        }
    }
}
