using System;
using GraceRun.Abilities;
using Unity.Collections;
using Unity.Netcode;

namespace GraceRun.Run
{
    /// <summary>
    /// One player's draft hand, synced to every PC so everyone sees everyone's cards and picks:
    /// the dealt cards (catalog ids), which one was picked, and the rerolls left this run.
    /// </summary>
    public struct DraftHand : INetworkSerializable, IEquatable<DraftHand>
    {
        public const sbyte NoPick = -1;

        public ulong ClientId;
        public int PlayerIndex;
        public FixedList32Bytes<short> Cards;
        public sbyte Picked;
        public byte RerollsLeft;

        public bool HasPicked => Picked >= 0 && Picked < Cards.Length;

        /// <summary>Catalog id of the picked ability, or <see cref="AbilityCatalog.None"/>.</summary>
        public short PickedAbility => HasPicked ? Cards[Picked] : AbilityCatalog.None;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerIndex);
            serializer.SerializeValue(ref Picked);
            serializer.SerializeValue(ref RerollsLeft);

            var count = (byte)Cards.Length;
            serializer.SerializeValue(ref count);
            if (serializer.IsReader) Cards.Clear();
            for (var i = 0; i < count; i++)
            {
                var card = serializer.IsReader ? (short)0 : Cards[i];
                serializer.SerializeValue(ref card);
                if (serializer.IsReader) Cards.Add(card);
            }
        }

        public bool Equals(DraftHand other) =>
            ClientId == other.ClientId && PlayerIndex == other.PlayerIndex && Cards.Equals(other.Cards) &&
            Picked == other.Picked && RerollsLeft == other.RerollsLeft;

        public override bool Equals(object obj) => obj is DraftHand other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ClientId, Picked, RerollsLeft, Cards.Length);
    }
}
