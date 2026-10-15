using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Combat
{
    /// <summary>
    /// One hit as reported to the server: raw damage, the knockback velocity the target's owner
    /// applies, and who dealt it (for the friendly-fire rule).
    /// </summary>
    public struct DamageInfo : INetworkSerializable
    {
        /// <summary>Attacker id for damage that has no NetworkObject behind it (hazards, debug).</summary>
        public const ulong NoAttacker = ulong.MaxValue;

        public int Amount;
        public Vector2 Knockback;
        public ulong AttackerId;
        public Team AttackerTeam;

        public DamageInfo(int amount, Vector2 knockback, ulong attackerId, Team attackerTeam)
        {
            Amount = amount;
            Knockback = knockback;
            AttackerId = attackerId;
            AttackerTeam = attackerTeam;
        }

        /// <summary>Damage from no one in particular, e.g. a hazard or a debug command.</summary>
        public static DamageInfo FromEnvironment(int amount, Vector2 knockback = default) =>
            new(amount, knockback, NoAttacker, Team.Neutral);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref Knockback);
            serializer.SerializeValue(ref AttackerId);
            serializer.SerializeValue(ref AttackerTeam);
        }
    }
}
