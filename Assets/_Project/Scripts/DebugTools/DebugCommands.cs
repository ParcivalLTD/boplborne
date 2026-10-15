using GraceRun.Bosses;
using GraceRun.Combat;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.DebugTools
{
    /// <summary>
    /// Debug actions behind the F1 menu. Anything that changes game state goes to the server
    /// through an RPC, so the commands work the same on a host and on a client.
    /// </summary>
    public static class DebugCommands
    {
        private static NetworkObject _cachedPlayer;
        private static Health _cachedHealth;

        /// <summary>This PC's own player Health, or null (menus, server-only build).</summary>
        public static Health LocalPlayerHealth()
        {
            var net = NetworkManager.Singleton;
            var player = net != null && net.IsClient ? net.LocalClient?.PlayerObject : null;
            if (player != _cachedPlayer)
            {
                _cachedPlayer = player;
                _cachedHealth = player != null ? player.GetComponent<Health>() : null;
            }
            return _cachedHealth;
        }

        /// <summary>Damages your own player. Goes through the same report RPC as a real hit.</summary>
        public static void DamageMe(Health self, int amount) =>
            self.ReportHit(DamageInfo.FromEnvironment(amount));

        public static void HealMe(Health self) => self.RestoreFullRpc();

        public static void ToggleGodMode(Health self) => self.SetGodModeRpc(!self.GodMode);

        /// <summary>Hits the dummy as if your own attack had landed: attacker-side report with knockback away from you.</summary>
        public static void HitDummy(Health self, Health dummy, int amount, Vector2 knockback)
        {
            var away = Mathf.Sign(dummy.transform.position.x - self.transform.position.x);
            var push = new Vector2(knockback.x * away, knockback.y);
            dummy.ReportHit(new DamageInfo(amount, push, self.NetworkObjectId, Team.Player));
        }
    
        /// <summary>(Re)spawns boss <paramref name="index"/> of the scene's BossSpawner, through the server.</summary>
        public static void SkipToBoss(int index)
        {
            if (BossSpawner.Instance != null) BossSpawner.Instance.SkipToBossRpc(index);
        }

        /// <summary>Sets the current boss's HP, through the server.</summary>
        public static void SetBossHP(int value)
        {
            var boss = BossBrain.Active;
            if (boss != null) boss.Health.DebugSetCurrentRpc(value);
        }
    }
}
