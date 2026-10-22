using System;
using Unity.Netcode;

namespace GraceRun.Run
{
    /// <summary>
    /// How many times each one-off party buff has been bought for the next fight. Buffs stack: buy the
    /// same one again, or all three. Synced from the server as one value.
    /// </summary>
    public struct BuffStacks : INetworkSerializable, IEquatable<BuffStacks>
    {
        public byte Empowered;
        public byte IronSkin;
        public byte Swiftstride;

        /// <summary>Stacks of <paramref name="buff"/> (0 for <see cref="PartyBuff.None"/>).</summary>
        public readonly int Of(PartyBuff buff) => buff switch
        {
            PartyBuff.EmpoweredAttacks => Empowered,
            PartyBuff.IronSkin => IronSkin,
            PartyBuff.Swiftstride => Swiftstride,
            _ => 0,
        };

        /// <summary>These stacks with one more of <paramref name="buff"/>.</summary>
        public readonly BuffStacks Plus(PartyBuff buff)
        {
            var result = this;
            switch (buff)
            {
                case PartyBuff.EmpoweredAttacks: result.Empowered++; break;
                case PartyBuff.IronSkin: result.IronSkin++; break;
                case PartyBuff.Swiftstride: result.Swiftstride++; break;
            }
            return result;
        }

        public readonly bool IsEmpty => Empowered == 0 && IronSkin == 0 && Swiftstride == 0;

        public readonly bool Equals(BuffStacks other) =>
            Empowered == other.Empowered && IronSkin == other.IronSkin && Swiftstride == other.Swiftstride;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Empowered);
            serializer.SerializeValue(ref IronSkin);
            serializer.SerializeValue(ref Swiftstride);
        }
    }
}
