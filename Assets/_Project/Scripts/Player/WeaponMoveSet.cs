using System.Collections.Generic;

namespace GraceRun.Player
{
    /// <summary>
    /// One weapon's attacks flattened into the single hit list that <see cref="LightAttackCombo"/>
    /// plays: the chained light hits first, then Heavy, Aerial, Up and Down for the ones the weapon
    /// has. Hit indices are what travels over the network. No Unity objects, so it is tested in EditMode.
    /// </summary>
    public sealed class WeaponMoveSet
    {
        private const int Missing = -1;

        private readonly int[] _index = { 0, Missing, Missing, Missing, Missing };
        private readonly WeaponMove[] _moveOf;

        public WeaponMoveSet(IReadOnlyList<LightAttackHit> combo, LightAttackHit? heavy, LightAttackHit? aerial,
            LightAttackHit? up, LightAttackHit? down)
        {
            var hits = new List<LightAttackHit>(combo);
            var moves = new List<WeaponMove>();
            for (var i = 0; i < combo.Count; i++) moves.Add(WeaponMove.Light);
            ComboLength = combo.Count;
            Add(hits, moves, WeaponMove.Heavy, heavy);
            Add(hits, moves, WeaponMove.Aerial, aerial);
            Add(hits, moves, WeaponMove.Up, up);
            Add(hits, moves, WeaponMove.Down, down);
            Hits = hits;
            _moveOf = moves.ToArray();
        }

        public IReadOnlyList<LightAttackHit> Hits { get; }

        /// <summary>How many hits at the front chain into the light combo.</summary>
        public int ComboLength { get; }

        /// <summary>The hit index that starts <paramref name="move"/>; the first light hit when the weapon lacks it.</summary>
        public int IndexFor(WeaponMove move) => _index[(int)move] >= 0 ? _index[(int)move] : 0;

        public WeaponMove MoveOf(int hitIndex) => hitIndex >= 0 && hitIndex < _moveOf.Length ? _moveOf[hitIndex] : WeaponMove.Light;

        /// <summary>True for the last light hit and for the Heavy move (the big swings that get the heavy sound).</summary>
        public bool IsFinisher(int hitIndex) => hitIndex == ComboLength - 1 || MoveOf(hitIndex) == WeaponMove.Heavy;

        private void Add(List<LightAttackHit> hits, List<WeaponMove> moves, WeaponMove move, LightAttackHit? hit)
        {
            if (hit == null) return;
            _index[(int)move] = hits.Count;
            hits.Add(hit.Value);
            moves.Add(move);
        }
    }
}
