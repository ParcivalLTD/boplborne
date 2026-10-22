using System.Collections.Generic;
using GraceRun.Abilities;
using GraceRun.Bosses;
using UnityEngine;

namespace GraceRun.Run
{
    /// <summary>
    /// Run tuning (docs/ARCHITECTURE.md): the hub arena, the boss pool per floor, which floors are
    /// followed by a rest stop, shared currency awards, and the draft rules. Lives under Data/Config.
    /// </summary>
    [CreateAssetMenu(menuName = "Grace Run/Run Config", fileName = "RunConfig")]
    public class RunConfig : ScriptableObject
    {
        [Header("Run")]
        [SerializeField, Tooltip("Arena prefab spawned in the hub and at rest stops.")]
        private GameObject _hubArenaPrefab;
        [SerializeField, Tooltip("One entry per floor, in order; the last entry is the final boss floor.")]
        private FloorPool[] _floors;
        [SerializeField, Tooltip("1-based floor numbers followed by a rest stop after their draft (GDD: 2, 4, 6).")]
        private int[] _restAfterFloors = { 2, 4, 6 };

        [Header("Modes")]
        [SerializeField, Tooltip("Bosses the leader can pick for Boss practice, in hub order.")]
        private BossData[] _practiceBosses;
        [SerializeField, Tooltip("Abilities dealt into empty slots at the start of Boss practice and the Arena, one per slot.")]
        private AbilityData[] _modeKit;

        [SerializeField, Min(1), Tooltip("Arena: round wins a team needs to take the match.")]
        private int _arenaWinsNeeded = 3;
        [SerializeField, Min(0f), Tooltip("Arena: seconds between a round ending and the next countdown.")]
        private float _arenaIntermission = 3.5f;
        [SerializeField, Min(1f), Tooltip("Arena: distance from the middle of the arena at which each team starts.")]
        private float _arenaSpawnEdge = 7f;

        [Header("Currency")]
        [SerializeField, Min(0)] private int _currencyPerBoss = 100;
        [SerializeField, Min(0)] private int _bonusNoDeaths = 50;
        [SerializeField, Min(0)] private int _bonusFastKill = 50;
        [SerializeField, Min(1f), Tooltip("Boss kill under this many seconds grants the fast kill bonus.")]
        private float _fastKillSeconds = 60f;

        [Header("Rest Stop")]
        [SerializeField, Min(0), Tooltip("Shared currency cost to buy an additional team reroll.")]
        private int _rerollCost = 50;
        [SerializeField, Min(0), Tooltip("Shared currency cost to upgrade one ability to Level 2.")]
        private int _upgradeCost = 100;
        [SerializeField, Min(0), Tooltip("Shared currency cost to buy a party buff for the next boss fight.")]
        private int _buffCost = 75;
        [SerializeField] private PartyBuffTuning _partyBuffs = new();

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

        public GameObject HubArenaPrefab => _hubArenaPrefab;
        public IReadOnlyList<FloorPool> Floors => _floors;
        public IReadOnlyList<int> RestAfterFloors => _restAfterFloors;
        public int CurrencyPerBoss => _currencyPerBoss;
        public int BonusNoDeaths => _bonusNoDeaths;
        public int BonusFastKill => _bonusFastKill;
        public float FastKillSeconds => _fastKillSeconds;
        public int RerollCost => _rerollCost;
        public int UpgradeCost => _upgradeCost;
        public int BuffCost => _buffCost;
        public PartyBuffTuning PartyBuffs => _partyBuffs;
        public int CardsOffered => _cardsOffered;
        public int RerollsPerRun => _rerollsPerRun;
        public int LegendaryMinFloor => _legendaryMinFloor;
        public RarityWeights RarityWeights => _rarityWeights;
        public int ArenaWinsNeeded => _arenaWinsNeeded;
        public float ArenaIntermission => _arenaIntermission;
        public float ArenaSpawnEdge => _arenaSpawnEdge;
        public BossData[] PracticeBosses => _practiceBosses ?? System.Array.Empty<BossData>();
        public AbilityData[] ModeKit => _modeKit ?? System.Array.Empty<AbilityData>();
        public AbilityData[] DraftPool => _draftPool;
        public float DraftOpenDelay => _draftOpenDelay;
        public float DraftCloseDelay => _draftCloseDelay;
    }
}
