using Unity.Netcode;

namespace GraceRun.Run
{
    /// <summary>The Arena match as every PC sees it: round wins, the round number and who took the last round.</summary>
    public struct ArenaState : INetworkSerializable
    {
        public byte ScoreA;
        public byte ScoreB;
        public byte Round;
        public ArenaRound LastRound;
        public bool MatchOver;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ScoreA);
            serializer.SerializeValue(ref ScoreB);
            serializer.SerializeValue(ref Round);
            serializer.SerializeValue(ref LastRound);
            serializer.SerializeValue(ref MatchOver);
        }
    }
}
