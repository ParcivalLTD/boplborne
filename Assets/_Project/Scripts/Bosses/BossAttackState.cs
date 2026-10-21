using System;
using Unity.Netcode;

namespace GraceRun.Bosses
{
    /// <summary>
    /// The boss attack every PC should be playing: which attack, the server tick it starts on,
    /// who it targets and which way the boss faces. The server writes it once per attack
    /// (<see cref="BossBrain"/>); every <see cref="BossAttackRunner"/> plays it from the tick.
    /// </summary>
    public struct BossAttackState : INetworkSerializable, IEquatable<BossAttackState>
    {
        /// <summary>Counts up with every attack, so the same attack twice in a row is still a change.</summary>
        public ushort Sequence;

        /// <summary>Attack id from <see cref="BossData.AttackById"/>; negative = no attack.</summary>
        public short AttackId;

        public int StartTick;
        public ulong TargetId;

        /// <summary>+1 facing right, -1 facing left, fixed for the whole attack.</summary>
        public sbyte Facing;

        /// <summary>True for a feint: the wind-up plays, then no strike and no chain, only a short recovery.</summary>
        public bool Feint;

        public static BossAttackState None => new() { AttackId = -1, TargetId = BossTargeting.NoTarget, Facing = 1 };

        public bool IsNone => AttackId < 0;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Sequence);
            serializer.SerializeValue(ref AttackId);
            serializer.SerializeValue(ref StartTick);
            serializer.SerializeValue(ref TargetId);
            serializer.SerializeValue(ref Facing);
            serializer.SerializeValue(ref Feint);
        }

        public bool Equals(BossAttackState other) =>
            Sequence == other.Sequence && AttackId == other.AttackId && StartTick == other.StartTick &&
            TargetId == other.TargetId && Facing == other.Facing && Feint == other.Feint;

        public override bool Equals(object obj) => obj is BossAttackState other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Sequence, AttackId, StartTick, TargetId, Facing, Feint);
    }
}
