using System;
using Unity.Netcode;

namespace GraceRun.Abilities
{
    /// <summary>One synced loadout slot: which ability (catalog id), server time ready, and upgrade level.</summary>
    public struct AbilitySlotState : INetworkSerializable, IEquatable<AbilitySlotState>
    {
        public short AbilityId;
        public double ReadyAt;
        public byte Level;

        public AbilitySlotState(short abilityId, double readyAt, byte level = 1)
        {
            AbilityId = abilityId;
            ReadyAt = readyAt;
            Level = level > 0 ? level : (byte)1;
        }

        public static AbilitySlotState Empty => new(AbilityCatalog.None, 0d, 1);

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref AbilityId);
            serializer.SerializeValue(ref ReadyAt);
            serializer.SerializeValue(ref Level);
        }

        public bool Equals(AbilitySlotState other) => AbilityId == other.AbilityId && ReadyAt.Equals(other.ReadyAt) && Level == other.Level;
        public override bool Equals(object obj) => obj is AbilitySlotState other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(AbilityId, ReadyAt, Level);
    }
}
