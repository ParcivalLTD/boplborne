using GraceRun.Abilities;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Run tuning (docs/ARCHITECTURE.md). For now only the draft part; floors, rest stops and
    /// currency join it with RunManager (M6). Lives under Data/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Run Config", fileName = "RunConfig")]
    public class RunConfig : ScriptableObject
    {
        [Header("Draft")]
        [SerializeField, Range(1, 4), Tooltip("Cards each player is offered per draft.")]
        private int _cardsOffered = 3;
        [SerializeField, Min(0)] private int _rerollsPerRun = 1;
        [SerializeField, Min(1), Tooltip("Legendary cards are only dealt from this floor on.")]
        private int _legendaryMinFloor = 4;
        [SerializeField] private RarityWeights _rarityWeights = new(70f, 25f, 5f);
        [SerializeField, Tooltip("Abilities the draft can deal. Every one must also be in the AbilityCatalog.")]
        private AbilityData[] _draftPool;
        [SerializeField, Min(0f), Tooltip("Seconds the victory screen stays before the draft opens.")]
        private float _draftOpenDelay = 2f;
        [SerializeField, Min(0f), Tooltip("Seconds everyone's picks stay on screen after the last pick.")]
        private float _draftCloseDelay = 2f;

        public int CardsOffered => _cardsOffered;
        public int RerollsPerRun => _rerollsPerRun;
        public int LegendaryMinFloor => _legendaryMinFloor;
        public RarityWeights RarityWeights => _rarityWeights;
        public AbilityData[] DraftPool => _draftPool;
        public float DraftOpenDelay => _draftOpenDelay;
        public float DraftCloseDelay => _draftCloseDelay;
    }
}
