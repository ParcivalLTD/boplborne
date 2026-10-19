using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Bosses.Behaviours
{
    /// <summary>
    /// Pure math and rules for The Bell Twins (Floor 3): splitting scaled encounter HP across two
    /// linked boss bodies, shared boss-bar normalization, 50%-threshold twin death transfer so the
    /// survivor enrages without a bar jump, and target splitting across living players.
    /// </summary>
    public static class BellTwinMath
    {
        /// <summary>Splits <paramref name="totalScaledHP"/> across the two twin bodies (at least 1 HP each).</summary>
        public static void SplitEncounterHP(int totalScaledHP, out int primaryHP, out int partnerHP)
        {
            var clamped = Mathf.Max(2, totalScaledHP);
            primaryHP = (clamped + 1) / 2;
            partnerHP = clamped - primaryHP;
        }

        /// <summary>Normalized 0..1 fill of the shared boss bar across both twin bodies.</summary>
        public static float NormalizedEncounterHP(int primaryCurrent, int partnerCurrent, int encounterMax)
        {
            if (encounterMax <= 0) return 0f;
            var sum = Mathf.Max(0, primaryCurrent) + Mathf.Max(0, partnerCurrent);
            return Mathf.Clamp01((float)sum / encounterMax);
        }

        /// <summary>
        /// True when one twin should die and the survivor should enrage: either one twin reached 0 HP
        /// while the other is alive, or split damage across both twins brought combined HP to or below
        /// <paramref name="threshold"/> of <paramref name="encounterMax"/>.
        /// </summary>
        public static bool ShouldTriggerEnrage(int primaryCurrent, int partnerCurrent, int encounterMax, float threshold)
        {
            var total = Mathf.Max(0, primaryCurrent) + Mathf.Max(0, partnerCurrent);
            if (total <= 0) return false;
            if (primaryCurrent <= 0 || partnerCurrent <= 0) return true;
            return encounterMax > 0 && total <= Mathf.RoundToInt(encounterMax * threshold);
        }

        /// <summary>
        /// Resolves the moment one twin dies: the weaker twin (or the partner on a tie) drops to 0 HP,
        /// and any remaining HP in the link transfers to the survivor (capped at <paramref name="survivorMax"/>)
        /// so the shared boss bar stays continuous at the phase 2 transition.
        /// </summary>
        public static void ResolveTwinDeath(
            int primaryCurrent,
            int partnerCurrent,
            int survivorMax,
            out int newPrimary,
            out int newPartner,
            out bool primarySurvives)
        {
            var p1 = Mathf.Max(0, primaryCurrent);
            var p2 = Mathf.Max(0, partnerCurrent);
            var combined = Mathf.Clamp(p1 + p2, 1, Mathf.Max(1, survivorMax));

            primarySurvives = p1 >= p2;
            if (primarySurvives)
            {
                newPrimary = combined;
                newPartner = 0;
            }
            else
            {
                newPrimary = 0;
                newPartner = combined;
            }
        }

        /// <summary>
        /// Splits a debug "Set boss HP" value across the two twin bodies. Above the phase 2 threshold
        /// both twins stay alive; at or below the threshold one twin is set to 0 HP and the survivor
        /// receives the requested HP so Phase 2 triggers cleanly.
        /// </summary>
        public static void SplitDebugHP(
            int requestedTotalHP,
            int encounterMax,
            int singleBodyMax,
            float threshold,
            bool bothAlive,
            bool primaryIsSurvivor,
            out int primaryHP,
            out int partnerHP)
        {
            var clamped = Mathf.Clamp(requestedTotalHP, 0, Mathf.Max(1, encounterMax));
            if (clamped <= 0)
            {
                primaryHP = 0;
                partnerHP = 0;
                return;
            }

            var phase2Cutoff = Mathf.RoundToInt(encounterMax * threshold);
            if (bothAlive && clamped > phase2Cutoff)
            {
                primaryHP = Mathf.Min(singleBodyMax, (clamped + 1) / 2);
                partnerHP = Mathf.Min(singleBodyMax, clamped - primaryHP);
                return;
            }

            var survivorHP = Mathf.Min(singleBodyMax, clamped);
            primaryHP = primaryIsSurvivor ? survivorHP : 0;
            partnerHP = primaryIsSurvivor ? 0 : survivorHP;
        }

        /// <summary>
        /// Picks a target for one twin while avoiding <paramref name="partnerTarget"/> whenever at
        /// least one other living player exists, so the twins naturally split the team.
        /// </summary>
        public static ulong ChooseSplitTarget(
            ulong currentTarget,
            ulong partnerTarget,
            IReadOnlyList<ulong> candidates,
            int roll)
        {
            if (candidates == null || candidates.Count == 0) return BossTargeting.NoTarget;
            if (candidates.Count == 1) return candidates[0];

            var nonPartnerCount = 0;
            for (var i = 0; i < candidates.Count; i++)
                if (candidates[i] != partnerTarget) nonPartnerCount++;
            if (nonPartnerCount == 0) return BossTargeting.Choose(currentTarget, candidates, roll);

            if (nonPartnerCount == 1)
            {
                for (var i = 0; i < candidates.Count; i++)
                    if (candidates[i] != partnerTarget) return candidates[i];
            }

            var preferredCount = 0;
            for (var i = 0; i < candidates.Count; i++)
                if (candidates[i] != partnerTarget && candidates[i] != currentTarget) preferredCount++;

            var poolCount = preferredCount > 0 ? preferredCount : nonPartnerCount;
            var pick = Mathf.Abs(roll) % poolCount;
            for (var i = 0; i < candidates.Count; i++)
            {
                var id = candidates[i];
                if (id == partnerTarget) continue;
                if (preferredCount > 0 && id == currentTarget) continue;
                if (pick-- == 0) return id;
            }
            return candidates[0];
        }
    }
}
