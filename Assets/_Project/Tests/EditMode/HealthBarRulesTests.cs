using GraceRun.UI;
using NUnit.Framework;
using UnityEngine;

namespace GraceRun.Tests
{
    public class HealthBarRulesTests
    {
        [Test]
        public void Chip_jumps_up_with_healing()
        {
            Assert.AreEqual(0.9f, HealthBarRules.Chip(0.5f, 0.9f, 0f, 0.6f, 0.1f), 1e-5f);
        }

        [Test]
        public void Chip_waits_while_held()
        {
            Assert.AreEqual(0.8f, HealthBarRules.Chip(0.8f, 0.4f, 0.2f, 0.6f, 0.1f), 1e-5f);
        }

        [Test]
        public void Chip_drains_but_stops_at_hp()
        {
            Assert.AreEqual(0.74f, HealthBarRules.Chip(0.8f, 0.4f, 0f, 0.6f, 0.1f), 1e-5f);
            Assert.AreEqual(0.4f, HealthBarRules.Chip(0.41f, 0.4f, 0f, 0.6f, 0.1f), 1e-5f);
        }

        [Test]
        public void Hold_restarts_on_damage_and_counts_down_otherwise()
        {
            Assert.AreEqual(0.35f, HealthBarRules.Hold(0f, 1f, 0.8f, 0.35f, 0.1f), 1e-5f);
            Assert.AreEqual(0.15f, HealthBarRules.Hold(0.25f, 0.8f, 0.8f, 0.35f, 0.1f), 1e-5f);
            Assert.AreEqual(0f, HealthBarRules.Hold(0.05f, 0.8f, 0.9f, 0.35f, 0.1f), 1e-5f);
        }

        [Test]
        public void CooldownLabel_rounds_up_and_shows_tenths_under_one_second()
        {
            Assert.AreEqual("7", AbilitySlotView.CooldownLabel(6.2f));
            Assert.AreEqual(0.8f.ToString("0.0"), AbilitySlotView.CooldownLabel(0.8f));
            Assert.AreEqual(0.1f.ToString("0.0"), AbilitySlotView.CooldownLabel(0.01f));
        }

        [Test]
        public void FillColor_blends_between_warn_and_critical()
        {
            var full = Color.green;
            var low = Color.red;
            Assert.AreEqual(full, HealthBarRules.FillColor(0.8f, 0.5f, 0.25f, full, low));
            Assert.AreEqual(low, HealthBarRules.FillColor(0.1f, 0.5f, 0.25f, full, low));
            Assert.AreEqual(Color.Lerp(full, low, 0.5f), HealthBarRules.FillColor(0.375f, 0.5f, 0.25f, full, low));
        }
    }
}
