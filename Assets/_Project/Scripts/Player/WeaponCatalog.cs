using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>Every weapon in the game. The list index is the id that travels over the network; index 0 (Fists) is the default.</summary>
    [CreateAssetMenu(menuName = "Grace Run/Weapon Catalog", fileName = "WeaponCatalog")]
    public class WeaponCatalog : ScriptableObject
    {
        [SerializeField] private WeaponData[] _weapons;
        [SerializeField, Range(0.1f, 1f), Tooltip("How far the move stick must point up or down for the light button to swing up or down.")]
        private float _directionThreshold = 0.5f;

        public IReadOnlyList<WeaponData> Weapons => _weapons;
        public float DirectionThreshold => _directionThreshold;

        public bool IsValid(int id) => id >= 0 && id < _weapons.Length && _weapons[id] != null;

        /// <summary>The weapon with this id; Fists for an unknown id.</summary>
        public WeaponData Get(int id) => IsValid(id) ? _weapons[id] : _weapons[0];
    }
}
