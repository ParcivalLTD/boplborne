using System;
using Unity.Netcode;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Synced summary of a completed run (victory on final floor or defeat on any floor),
    /// sent from server to all clients to display the run results screen.
    /// </summary>
    public struct RunResultData : INetworkSerializable, IEquatable<RunResultData>
    {
        public bool IsValid;
        public bool IsVictory;
        public int FloorReached;
        public int TotalFloors;
        public float ElapsedSeconds;
        public int DamageP1;
        public int DamageP2;
        public int DamageP3;
        public int DamageP4;
        public int KnockbacksP1;
        public int KnockbacksP2;
        public int KnockbacksP3;
        public int KnockbacksP4;

        /// <summary>Gets damage dealt by player slot 0..3.</summary>
        public int GetDamage(int slot) => slot switch
        {
            0 => DamageP1,
            1 => DamageP2,
            2 => DamageP3,
            3 => DamageP4,
            _ => 0
        };

        /// <summary>Gets friendly-fire knockbacks caused by player slot 0..3.</summary>
        public int GetKnockbacks(int slot) => slot switch
        {
            0 => KnockbacksP1,
            1 => KnockbacksP2,
            2 => KnockbacksP3,
            3 => KnockbacksP4,
            _ => 0
        };

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref IsValid);
            serializer.SerializeValue(ref IsVictory);
            serializer.SerializeValue(ref FloorReached);
            serializer.SerializeValue(ref TotalFloors);
            serializer.SerializeValue(ref ElapsedSeconds);
            serializer.SerializeValue(ref DamageP1);
            serializer.SerializeValue(ref DamageP2);
            serializer.SerializeValue(ref DamageP3);
            serializer.SerializeValue(ref DamageP4);
            serializer.SerializeValue(ref KnockbacksP1);
            serializer.SerializeValue(ref KnockbacksP2);
            serializer.SerializeValue(ref KnockbacksP3);
            serializer.SerializeValue(ref KnockbacksP4);
        }

        public bool Equals(RunResultData other) =>
            IsValid == other.IsValid &&
            IsVictory == other.IsVictory &&
            FloorReached == other.FloorReached &&
            TotalFloors == other.TotalFloors &&
            Mathf.Approximately(ElapsedSeconds, other.ElapsedSeconds) &&
            DamageP1 == other.DamageP1 && DamageP2 == other.DamageP2 &&
            DamageP3 == other.DamageP3 && DamageP4 == other.DamageP4 &&
            KnockbacksP1 == other.KnockbacksP1 && KnockbacksP2 == other.KnockbacksP2 &&
            KnockbacksP3 == other.KnockbacksP3 && KnockbacksP4 == other.KnockbacksP4;

        public override bool Equals(object obj) => obj is RunResultData other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(
            IsValid, IsVictory, FloorReached, TotalFloors, ElapsedSeconds, DamageP1, KnockbacksP1);
    }
}
