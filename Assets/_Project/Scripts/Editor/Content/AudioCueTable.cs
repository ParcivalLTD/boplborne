using System.Collections.Generic;

namespace GraceRun.Editor.Content
{
    /// <summary>What <see cref="AudioBuilder"/> wires: which generated clip backs which cue, how loud, and what each asset gets.</summary>
    public static class AudioCueTable
    {
        /// <summary>One cue asset: name (file under Data/Audio), clip names (SFX/Cartoon), loudness and pitch spread.</summary>
        public readonly struct CueSpec
        {
            public readonly string Name;
            public readonly string[] Clips;
            public readonly float Volume;
            public readonly float Pitch;
            public readonly float MinInterval;

            public CueSpec(string name, string clip, double volume, double pitch = 0.04, double minInterval = 0.03)
            {
                Name = name;
                Clips = new[] { clip };
                Volume = (float)volume;
                Pitch = (float)pitch;
                MinInterval = (float)minInterval;
            }
        }

        /// <summary>Attack asset (under Data/Bosses/..) to its windup clip and strike clip.</summary>
        public static readonly (string folder, string attack, string windup, string strike)[] Attacks =
        {
            ("Gatekeeper", "OverheadSlam", "gk_overhead_slam", "st_gk_slam"),
            ("Gatekeeper", "OverheadSlamDouble", "gk_overhead_double", "st_gk_slam"),
            ("Gatekeeper", "OverheadSlamPhase2", "gk_overhead_phase2", "st_gk_slam"),
            ("Gatekeeper", "DelayedSecondSlam", "gk_delayed_slam", "st_gk_slam"),
            ("Gatekeeper", "ShieldBash", "gk_shield_bash", "st_gk_bash"),
            ("Gatekeeper", "SweepingCut", "gk_sweeping_cut", "st_gk_sweep"),
            ("BroodMother", "FangBite", "bm_fang_bite", "st_bm_bite"),
            ("BroodMother", "FangBiteDouble", "bm_fang_double", "st_bm_bite"),
            ("BroodMother", "FangBiteSecond", "bm_fang_second", "st_bm_bite"),
            ("BroodMother", "WebSpit", "bm_web_spit", "st_bm_web"),
            ("BroodMother", "SpawnCrawlers", "bm_spawn_crawlers", "st_bm_spawn"),
            ("BroodMother", "CeilingDrop", "bm_ceiling_drop", "st_bm_drop"),
            ("BellTwins", "ClapperSwing", "bt_clapper_swing", "st_bt_clapper"),
            ("BellTwins", "ClapperSwingDouble", "bt_clapper_double", "st_bt_clapper"),
            ("BellTwins", "ClapperSwingSecond", "bt_clapper_second", "st_bt_clapper"),
            ("BellTwins", "RingingToll", "bt_ringing_toll", "st_bt_toll"),
            ("BellTwins", "EnragedRingingToll", "bt_toll_enraged", "st_bt_toll"),
            ("BellTwins", "EnragedRingingEcho", "bt_echo_enraged", "st_bt_toll"),
            ("BellTwins", "VaultingDash", "bt_vault_dash", "st_bt_dash"),
            ("StormWyrm", "TailSweep", "sw_tail_sweep", "st_sw_tail"),
            ("StormWyrm", "TailSweepDouble", "sw_tail_double", "st_sw_tail"),
            ("StormWyrm", "TailSweepSecond", "sw_tail_second", "st_sw_tail"),
            ("StormWyrm", "ThunderLunge", "sw_thunder_lunge", "st_sw_lunge"),
            ("StormWyrm", "StormSlam", "sw_storm_slam", "st_sw_slam"),
            ("StormWyrm", "LightningColumns", "sw_lightning_columns", "st_sw_bolt"),
            ("StormWyrm", "LightningFlanks", "sw_lightning_flanks", "st_sw_bolt"),
            ("MirrorDuelist", "RapierLunge", "md_rapier_lunge", "st_md_lunge"),
            ("MirrorDuelist", "MirrorCut", "md_mirror_cut", "st_md_rapier"),
            ("MirrorDuelist", "MirrorCutDouble", "md_mirror_double", "st_md_rapier"),
            ("MirrorDuelist", "MirrorCutSecond", "md_mirror_second", "st_md_rapier"),
            ("MirrorDuelist", "MirrorEcho", "md_mirror_echo", "st_md_echo"),
            ("TestBoss", "TestSlam", "test_slam", "st_test"),
            ("TestBoss", "TestSlamFast", "test_slam_fast", "st_test"),
        };

        /// <summary>
        /// Extra boss cues (under Data/Audio/Boss) that live in a data asset instead of an attack: asset path, serialized
        /// cue field, the cue's name, clip, volume, pitch spread and minimum interval. A missing asset is skipped.
        /// </summary>
        public static readonly (string asset, string field, string cue, string clip, double volume, double pitch, double minInterval)[] BossExtras =
        {
            ("StormWyrm/LightningColumnData", "_strikeCue", "WyrmColumnBolt", "st_sw_column", 0.7, 0.05, 0.02),
            ("MirrorDuelist/MirrorDuelistData", "_reflectionAppearCue", "MirrorReflectionAppear", "md_reflection_appear", 0.65, 0.0, 0.2),
            ("MirrorDuelist/MirrorDuelistData", "_reflectionStrikeCue", "MirrorReflectionStrike", "md_reflection_strike", 0.7, 0.03, 0.1),
            ("MirrorDuelist/MirrorDuelistData", "_memoryChangeCue", "MirrorMemoryChange", "md_memory_change", 0.6, 0.02, 0.15),
            ("MirrorDuelist/MirrorShardData", "_spawnCue", "MirrorShardFly", "md_shard", 0.7, 0.05, 0.1),
            ("MirrorDuelist/MirrorWaveData", "_spawnCue", "MirrorWaveFly", "md_wave", 0.75, 0.03, 0.1),
        };

        /// <summary>Boss data folder to its music loop name (<c>music_&lt;name&gt;.wav</c>); the folder's one BossData asset is found by type.</summary>
        public static readonly (string folder, string track)[] BossMusic =
        {
            ("Gatekeeper", "gatekeeper"), ("BroodMother", "brood"), ("BellTwins", "bell"),
            ("StormWyrm", "wyrm"), ("MirrorDuelist", "duelist"),
        };

        /// <summary>Ability asset name to its special sound clip (everything else uses the cue for its tag).</summary>
        public static readonly (string ability, string clip)[] AbilitySpecials =
        {
            ("Thunderclap", "ability_zap"), ("FrostBolt", "ability_ice"), ("GraceTotem", "ability_heal"),
            ("Firebomb", "ability_explosion"), ("Bomb", "ability_explosion"), ("ClusterBomb", "ability_explosion"),
            ("Meteor", "ability_explosion"), ("BlackHole", "ability_pull"), ("GravityGrenade", "ability_pull"),
            ("StoneWall", "ability_wall"), ("SpikeTrap", "ability_trap"), ("ProximityMine", "ability_trap"),
            ("BouncePad", "ability_trap"), ("GrappleHook", "ability_grapple"), ("ShrinkRay", "ability_buff"),
            ("Decoy", "ability_buff"),
        };

        /// <summary>Every non-boss cue, in the order they are created.</summary>
        public static IEnumerable<CueSpec> General()
        {
            yield return new CueSpec("Jump", "jump", 0.55);
            yield return new CueSpec("Land", "land", 0.5);
            yield return new CueSpec("Dodge", "dodge", 0.6);
            yield return new CueSpec("PlayerHurt", "hurt", 0.8);
            yield return new CueSpec("Revive", "revive", 0.7);
            yield return new CueSpec("FlaskDrink", "flask_drink", 0.7);
            yield return new CueSpec("DraftPick", "draft_pick", 0.7);
            yield return new CueSpec("UiClick", "ui_click", 0.6, 0.02, 0.05);
            yield return new CueSpec("UiHover", "ui_hover", 0.45, 0.03, 0.05);
            yield return new CueSpec("UiConfirm", "ui_confirm", 0.65, 0.02, 0.05);
            yield return new CueSpec("UiBack", "ui_back", 0.55, 0.02, 0.05);
            yield return new CueSpec("UiDeny", "ui_deny", 0.6, 0.02, 0.1);
            yield return new CueSpec("DoorStart", "door_start", 0.8, 0.0, 0.5);
            yield return new CueSpec("VictorySting", "victory_sting", 0.85, 0.0, 1.0);
            yield return new CueSpec("DefeatSting", "defeat_sting", 0.85, 0.0, 1.0);
            yield return new CueSpec("Coin", "pickup_coin", 0.6, 0.05, 0.05);
            yield return new CueSpec("ShopBuy", "shop_buy", 0.7, 0.03, 0.1);
            yield return new CueSpec("BossPhaseRoar", "boss_phase_roar", 0.9, 0.0, 1.0);
            yield return new CueSpec("BossDeath", "boss_death", 0.95, 0.0, 1.0);
            yield return new CueSpec("BossStagger", "boss_stagger", 0.85, 0.0, 0.5);
            yield return new CueSpec("BossEnrage", "boss_enrage", 0.9, 0.0, 1.0);
            yield return new CueSpec("StoryCard", "story_card", 0.5, 0.03, 0.1);
            yield return new CueSpec("AscentChime", "ascent_chime", 0.55, 0.0, 1.0);
            yield return new CueSpec("AbilityProjectile", "ability_projectile", 0.55, 0.06, 0.06);
            yield return new CueSpec("AbilityArea", "ability_area", 0.7, 0.04, 0.06);
            yield return new CueSpec("AbilityMovement", "ability_movement", 0.6, 0.05, 0.06);
            yield return new CueSpec("AbilitySummon", "ability_summon", 0.65, 0.03, 0.06);
            foreach (var (_, clip) in AbilitySpecials) yield return new CueSpec("Ability_" + clip, clip, 0.7, 0.04, 0.06);
            foreach (var weapon in new[] { "fists", "sword", "spear", "hammer" })
            {
                yield return new CueSpec(Cap(weapon) + "Swing", weapon + "_swing", 0.6);
                yield return new CueSpec(Cap(weapon) + "Finisher", weapon + "_heavy", 0.75);
                yield return new CueSpec(Cap(weapon) + "Hit", weapon + "_hit", 0.8, 0.05);
                yield return new CueSpec(Cap(weapon) + "HeavyHit", weapon + "_hit_heavy", 0.9, 0.04);
            }
        }

        private static string Cap(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
