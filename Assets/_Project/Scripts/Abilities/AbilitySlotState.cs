using System;
using Unity.Netcode;

namespace GraceRun.Abilities
{
    /// <summary>One synced loadout slot: which ability (catalog id) and the server time it is ready again.</summary>
    public struct AbilitySlotState : INetworkSerializable, IEquatable<AbilitySlotState>
    {
        public short AbilityId;
        public double ReadyAt;

        public AbilitySlotState(short abilityId, double readyAt)
        {
            AbilityId = abilityId;
            ReadyAt = readyAt;
        }

        public static AbilitySlotState Empty => new(AbilityCatalog.None, 0d);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref AbilityId);
            serializer.SerializeValue(ref ReadyAt);
        }

        public bool Equals(AbilitySlotState other) => AbilityId == other.AbilityId && ReadyAt.Equals(other.ReadyAt);
        public override bool Equals(object obj) => obj is AbilitySlotState other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(AbilityId, ReadyAt);
    }
}
