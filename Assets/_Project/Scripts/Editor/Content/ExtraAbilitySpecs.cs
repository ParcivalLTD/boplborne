using System.Collections.Generic;
using GraceRun.Abilities;
using GraceRun.Abilities.Effects;
using GraceRun.Combat;
using UnityEngine;
using T = GraceRun.Abilities.AbilityTags;

namespace GraceRun.Editor.Content
{
    /// <summary>
    /// The 20 abilities added after the prototype set: the six GDD backlog abilities plus fourteen
    /// new ones. Values here are only the starting numbers the builder writes into each
    /// AbilityData asset; tune the assets afterwards, not this file.
    /// </summary>
    public static class ExtraAbilitySpecs
    {
        public static List<AbilitySpec> All()
        {
            return new List<AbilitySpec>
            {
                Spec("Blink", "blink", T.Movement, AbilityRarity.Rare, 5f,
                    "Teleport a short way toward the aim. Passes through summoned walls, ledges and friends.",
                    "Blink through a teammate's wall", f =>
                    {
                        f["_range"] = 6f; f["_forceSpeed"] = 4f;
                    }, action: "BlinkAction"),

                Spec("CometDash", "comet_dash", T.Movement, AbilityRarity.Common, 4.5f,
                    "Burst along the aim, hitting whatever you pass through.",
                    "Dash onto a Bounce Pad for a huge launch", f =>
                    {
                        f["_forceSpeed"] = 28f; f["_maxFlightTime"] = 0.18f; f["_size"] = new Vector2(0.35f, 1f);
                        f["_effectLifetime"] = 0.3f; f["_effectRadius"] = 1.1f;
                        f["_damage"] = 8; f["_knockback"] = 10f; f["_knockbackLift"] = 4f;
                    }, EffectTemplate.Area, new Color(1f, 0.6f, 0.24f, 0.45f), action: "DashAction",
                    add: new[] { typeof(FollowCaster), typeof(ContactZone) },
                    comp: new() { ["ContactZone._hitEachTargetOnce"] = true }),

                Spec("BouncePad", "bounce_pad", T.Summon | T.Movement, AbilityRarity.Common, 10f,
                    "A trampoline at the aim point. Anyone who lands on it is launched up.",
                    "Reach a boss's high weak points", f =>
                    {
                        f["_spawnMode"] = AbilitySpawnMode.AtAimPoint; f["_range"] = 7f;
                        f["_effectLifetime"] = 8f; f["_effectRadius"] = 1.3f;
                        f["_forceSpeed"] = 24f; f["_tickInterval"] = 0.4f;
                    }, EffectTemplate.Area, new Color(0.24f, 0.8f, 0.5f, 0.7f),
                    add: new[] { typeof(FloorSnap), typeof(LaunchPad) }),

                Spec("Decoy", "decoy", T.Summon, AbilityRarity.Rare, 14f,
                    "A copy of you that every boss goes after while it stands.",
                    "Buys time to revive a friend", f =>
                    {
                        f["_spawnMode"] = AbilitySpawnMode.AtAimPoint; f["_range"] = 5f;
                        f["_effectLifetime"] = 5f; f["_effectRadius"] = 0.75f;
                    }, EffectTemplate.Area, new Color(0.66f, 0.78f, 1f, 0.75f),
                    add: new[] { typeof(FloorSnap), typeof(Decoy) }, remove: new[] { typeof(FadeView) }, player: true),

                Spec("Meteor", "meteor", T.Projectile | T.Area, AbilityRarity.Legendary, 20f,
                    "Calls a slow, huge meteor down on the aim point.",
                    "Hits everyone underneath, friends too", f =>
                    {
                        f["_minFloor"] = 4; f["_spawnMode"] = AbilitySpawnMode.AboveAimPoint; f["_range"] = 12f; f["_dropHeight"] = 12f;
                        f["_launchSpeed"] = 5f; f["_gravityScale"] = 0.4f; f["_maxFlightTime"] = 5f;
                        f["_effectRadius"] = 3.5f; f["_damage"] = 45; f["_knockback"] = 22f; f["_knockbackLift"] = 14f; f["_visualTime"] = 0.5f;
                    }, EffectTemplate.Bomb, new Color(1f, 0.42f, 0.2f), scale: 1.6f),

                Spec("ShrinkRay", "shrink_ray", T.Projectile, AbilityRarity.Rare, 10f,
                    "A beam bolt that shrinks whatever it hits for a few seconds.",
                    "Shrunk teammates are harder to hit, and hit weaker", f =>
                    {
                        f["_launchSpeed"] = 24f; f["_gravityScale"] = 0f; f["_maxFlightTime"] = 1.2f;
                        f["_damage"] = 3; f["_knockback"] = 2f;
                        f["_status"] = StatusKind.Shrink; f["_statusDuration"] = 5f; f["_statusStrength"] = 0.55f;
                    }, EffectTemplate.Arrow, new Color(1f, 0.37f, 0.86f), scale: 0.45f, circle: true,
                    add: new[] { typeof(StatusOnHit) }),

                Spec("SpikeTrap", "spike_trap", T.Summon | T.Area, AbilityRarity.Rare, 12f,
                    "Floor spikes at the aim point that hurt anything standing on them.",
                    "Push enemies in with Gust or Black Hole", f =>
                    {
                        f["_spawnMode"] = AbilitySpawnMode.AtAimPoint; f["_range"] = 8f;
                        f["_effectLifetime"] = 8f; f["_effectRadius"] = 1.6f; f["_tickInterval"] = 0.5f;
                        f["_damage"] = 6; f["_knockback"] = 2f; f["_knockbackLift"] = 9f;
                    }, EffectTemplate.Area, new Color(0.75f, 0.75f, 0.8f, 0.6f),
                    add: new[] { typeof(FloorSnap), typeof(ContactZone) }),

                Spec("FrostBolt", "frost_bolt", T.Projectile, AbilityRarity.Common, 5f,
                    "An icy bolt that slows what it hits.",
                    "Slowed bosses walk into Spike Traps", f =>
                    {
                        f["_launchSpeed"] = 20f; f["_gravityScale"] = 0.2f; f["_maxFlightTime"] = 1.5f;
                        f["_damage"] = 7; f["_knockback"] = 3f;
                        f["_status"] = StatusKind.Slow; f["_statusDuration"] = 3f; f["_statusStrength"] = 0.45f;
                    }, EffectTemplate.Arrow, new Color(0.55f, 0.85f, 1f), scale: 0.45f, circle: true,
                    add: new[] { typeof(StatusOnHit) }),

                Spec("WispSwarm", "wisp_swarm", T.Projectile, AbilityRarity.Rare, 9f,
                    "Four wisps that seek out the nearest enemy.",
                    "Wisps through a Black Hole swirl around it", f =>
                    {
                        f["_count"] = 4; f["_spread"] = 22f; f["_launchSpeed"] = 11f; f["_gravityScale"] = 0f; f["_maxFlightTime"] = 3f;
                        f["_range"] = 14f; f["_steerRate"] = 40f; f["_damage"] = 5; f["_knockback"] = 2f;
                    }, EffectTemplate.Arrow, new Color(0.93f, 0.95f, 0.6f), scale: 0.35f, circle: true,
                    add: new[] { typeof(Homing) }),

                Spec("Boomerang", "boomerang", T.Projectile, AbilityRarity.Common, 5f,
                    "A blade that flies out, cuts through everything and comes back.",
                    "Gust it back early for a double hit", f =>
                    {
                        f["_piercing"] = true; f["_launchSpeed"] = 18f; f["_gravityScale"] = 0f; f["_maxFlightTime"] = 2.2f;
                        f["_turnTime"] = 0.45f; f["_steerRate"] = 70f; f["_effectRadius"] = 1f;
                        f["_damage"] = 7; f["_knockback"] = 5f;
                    }, EffectTemplate.Arrow, new Color(1f, 0.79f, 0.29f), scale: 0.7f,
                    add: new[] { typeof(Boomerang) }),

                Spec("Firebomb", "firebomb", T.Projectile | T.Area, AbilityRarity.Common, 9f,
                    "A lobbed flask that leaves a burning pool.",
                    "The fire burns friends who stand in it too", f =>
                    {
                        f["_launchAngle"] = 40f; f["_launchSpeed"] = 13f; f["_gravityScale"] = 2f; f["_maxFlightTime"] = 2.5f;
                        f["_spawnOffset"] = new Vector2(0.6f, 0.6f); f["_damage"] = 4; f["_knockback"] = 2f; f["_childCount"] = 1;
                    }, EffectTemplate.Arrow, new Color(1f, 0.5f, 0.15f), scale: 0.5f, circle: true,
                    add: new[] { typeof(SpawnOnImpact) },
                    child: Spec("FirePool", "fire_pool", T.Area, AbilityRarity.Common, 0f, "Burning ground.", "", f =>
                    {
                        f["_range"] = 6f; f["_effectLifetime"] = 4f; f["_effectRadius"] = 1.8f; f["_tickInterval"] = 0.4f;
                        f["_damage"] = 3; f["_knockback"] = 1f; f["_knockbackLift"] = 3f;
                    }, EffectTemplate.Area, new Color(1f, 0.4f, 0.1f, 0.5f), add: new[] { typeof(FloorSnap), typeof(ContactZone) })),

                Spec("GravityGrenade", "gravity_grenade", T.Projectile | T.Area, AbilityRarity.Rare, 12f,
                    "A lobbed grenade that opens a small black hole where it lands.",
                    "Gathers projectiles, minions and teammates", f =>
                    {
                        f["_launchAngle"] = 40f; f["_launchSpeed"] = 13f; f["_gravityScale"] = 2f; f["_maxFlightTime"] = 2.5f;
                        f["_spawnOffset"] = new Vector2(0.6f, 0.6f); f["_damage"] = 2; f["_childCount"] = 1;
                    }, EffectTemplate.Arrow, new Color(0.35f, 0.25f, 0.55f), scale: 0.5f, circle: true,
                    add: new[] { typeof(SpawnOnImpact) },
                    child: Spec("MiniHole", "mini_hole", T.Area, AbilityRarity.Common, 0f, "A small black hole.", "", f =>
                    {
                        f["_effectLifetime"] = 1.8f; f["_effectRadius"] = 4f; f["_forceSpeed"] = 10f; f["_forceRate"] = 60f;
                        f["_size"] = new Vector2(0.8f, 1f);
                    }, EffectTemplate.Area, new Color(0.07f, 0.06f, 0.1f, 0.7f), add: new[] { typeof(PullField) })),

                Spec("ClusterBomb", "cluster_bomb", T.Projectile | T.Area, AbilityRarity.Rare, 11f,
                    "A bomb that bursts into three bomblets.",
                    "Bomblets launch teammates in every direction", f =>
                    {
                        f["_launchAngle"] = 50f; f["_launchSpeed"] = 14f; f["_gravityScale"] = 2f; f["_maxFlightTime"] = 2.5f;
                        f["_spawnOffset"] = new Vector2(0.6f, 0.6f); f["_effectRadius"] = 1.6f;
                        f["_damage"] = 8; f["_knockback"] = 8f; f["_knockbackLift"] = 8f; f["_visualTime"] = 0.3f;
                        f["_childCount"] = 3; f["_childSpread"] = 35f;
                    }, EffectTemplate.Bomb, new Color(0.95f, 0.85f, 0.4f),
                    add: new[] { typeof(SpawnOnImpact) },
                    child: Spec("Bomblet", "bomblet", T.Projectile | T.Area, AbilityRarity.Common, 0f, "A small bomb.", "", f =>
                    {
                        f["_launchSpeed"] = 9f; f["_gravityScale"] = 2f; f["_maxFlightTime"] = 1.2f; f["_effectRadius"] = 1.4f;
                        f["_damage"] = 7; f["_knockback"] = 10f; f["_knockbackLift"] = 10f; f["_visualTime"] = 0.25f;
                    }, EffectTemplate.Bomb, new Color(0.95f, 0.75f, 0.3f), scale: 0.6f)),

                Spec("GraceTotem", "grace_totem", T.Summon | T.Area, AbilityRarity.Rare, 18f,
                    "A totem that heals every player standing near it.",
                    "Plant it next to a gravestone while reviving", f =>
                    {
                        f["_spawnMode"] = AbilitySpawnMode.AtAimPoint; f["_range"] = 4f;
                        f["_effectLifetime"] = 8f; f["_effectRadius"] = 3.5f; f["_tickInterval"] = 1f; f["_damage"] = 4;
                    }, EffectTemplate.Area, new Color(0.4f, 1f, 0.55f, 0.35f),
                    add: new[] { typeof(FloorSnap), typeof(HealZone) }),

                Spec("Sentry", "sentry", T.Summon, AbilityRarity.Rare, 15f,
                    "A little turret that shoots the nearest enemy.",
                    "Its bolts pick up Area effects they fly through", f =>
                    {
                        f["_minFloor"] = 2; f["_spawnMode"] = AbilitySpawnMode.AtAimPoint; f["_range"] = 15f;
                        f["_spawnOffset"] = new Vector2(0f, 0.8f); f["_effectLifetime"] = 8f; f["_effectRadius"] = 0.6f;
                        f["_tickInterval"] = 0.8f; f["_childCount"] = 1;
                    }, EffectTemplate.Area, new Color(0.6f, 0.65f, 0.75f),
                    add: new[] { typeof(FloorSnap), typeof(Turret) },
                    child: Spec("SentryBolt", "sentry_bolt", T.Projectile, AbilityRarity.Common, 0f, "A turret bolt.", "", f =>
                    {
                        f["_launchSpeed"] = 22f; f["_gravityScale"] = 0f; f["_maxFlightTime"] = 1.2f; f["_damage"] = 4; f["_knockback"] = 2f;
                    }, EffectTemplate.Arrow, new Color(0.8f, 0.85f, 0.95f), scale: 0.5f)),

                Spec("BladeRing", "blade_ring", T.Area, AbilityRarity.Rare, 12f,
                    "Spinning blades circle you for a few seconds, cutting anything close.",
                    "Hug a friend and the blades get them too", f =>
                    {
                        f["_effectLifetime"] = 4f; f["_effectRadius"] = 2.2f; f["_tickInterval"] = 0.3f;
                        f["_damage"] = 4; f["_knockback"] = 6f; f["_knockbackLift"] = 3f;
                    }, EffectTemplate.Area, new Color(0.85f, 0.9f, 1f, 0.35f),
                    add: new[] { typeof(FollowCaster), typeof(ContactZone) }),

                Spec("Thunderclap", "thunderclap", T.Area, AbilityRarity.Common, 7f,
                    "A clap that blasts everything around you away.",
                    "Clap teammates out of a boss's slam", f =>
                    {
                        f["_spawnOffset"] = Vector2.zero; f["_launchSpeed"] = 0f; f["_gravityScale"] = 0f; f["_maxFlightTime"] = 0.02f;
                        f["_effectRadius"] = 3f; f["_damage"] = 6; f["_knockback"] = 18f; f["_knockbackLift"] = 8f; f["_visualTime"] = 0.3f;
                    }, EffectTemplate.Bomb, new Color(1f, 1f, 0.7f), scale: 0.01f),

                Spec("ProximityMine", "proximity_mine", T.Summon | T.Area, AbilityRarity.Common, 8f,
                    "A mine at the aim point that blows up when anything steps near.",
                    "Teammates set it off too. Plan accordingly", f =>
                    {
                        f["_spawnMode"] = AbilitySpawnMode.AtAimPoint; f["_range"] = 6f;
                        f["_effectLifetime"] = 20f; f["_effectRadius"] = 2.2f; f["_tickInterval"] = 0.6f;
                        f["_damage"] = 22; f["_knockback"] = 16f; f["_knockbackLift"] = 12f; f["_visualTime"] = 0.35f;
                    }, EffectTemplate.Area, new Color(0.88f, 0.29f, 0.29f), scale: 0.6f,
                    add: new[] { typeof(FloorSnap), typeof(ContactZone) }, remove: new[] { typeof(FadeView) },
                    comp: new() { ["ContactZone._detonateOnce"] = true, ["ContactZone._fxPrefab"] = "BombBurst" }),

                Spec("Tornado", "tornado", T.Area, AbilityRarity.Rare, 12f,
                    "A slow tornado that rolls forward, sucking things in.",
                    "Drags projectiles and teammates along with it", f =>
                    {
                        f["_spawnOffset"] = new Vector2(1.5f, 0.5f); f["_launchSpeed"] = 5f;
                        f["_effectLifetime"] = 3.5f; f["_effectRadius"] = 3f; f["_forceSpeed"] = 9f; f["_forceRate"] = 50f;
                        f["_size"] = new Vector2(0.8f, 1f);
                    }, EffectTemplate.Area, new Color(0.7f, 0.75f, 0.8f, 0.45f),
                    add: new[] { typeof(Drift), typeof(PullField) }),

                Spec("SpearThrow", "spear_throw", T.Projectile, AbilityRarity.Common, 6f,
                    "A fast spear that pierces through every target in a line.",
                    "Line up the boss and its minions", f =>
                    {
                        f["_piercing"] = true; f["_launchSpeed"] = 32f; f["_gravityScale"] = 0.15f; f["_maxFlightTime"] = 1.5f;
                        f["_damage"] = 14; f["_knockback"] = 9f; f["_knockbackLift"] = 2f;
                    }, EffectTemplate.Arrow, new Color(0.85f, 0.8f, 0.7f), scale: 1.3f),
            };
        }

        private static AbilitySpec Spec(string name, string id, T tags, AbilityRarity rarity, float cooldown,
            string description, string comboHook, System.Action<Dictionary<string, object>> fill,
            EffectTemplate template = EffectTemplate.None, Color? color = null, float scale = 0f,
            string action = null, System.Type[] add = null, System.Type[] remove = null,
            Dictionary<string, object> comp = null, AbilitySpec child = null, bool circle = false, bool player = false)
        {
            var fields = new Dictionary<string, object>
            {
                ["_id"] = id, ["_displayName"] = Nice(name), ["_description"] = description, ["_comboHook"] = comboHook,
                ["_tags"] = tags, ["_rarity"] = rarity, ["_cooldown"] = cooldown,
            };
            if (action != null) fields["_casterAction"] = action;
            fill(fields);
            return new AbilitySpec
            {
                Name = name, Id = id, Template = template, Fields = fields, Color = color ?? Color.white,
                VisualScale = scale, Add = add ?? System.Array.Empty<System.Type>(),
                Remove = remove ?? System.Array.Empty<System.Type>(), ComponentFields = comp ?? new(),
                Child = child, CircleVisual = circle, PlayerVisual = player, InDraft = cooldown > 0f,
            };
        }

        private static string Nice(string name) => System.Text.RegularExpressions.Regex.Replace(name, "(?<=[a-z])(?=[A-Z])", " ");
    }
}
