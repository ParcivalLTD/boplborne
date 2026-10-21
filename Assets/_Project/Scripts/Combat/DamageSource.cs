namespace GraceRun.Combat
{
    /// <summary>Stable small ids for what dealt a hit, carried in <see cref="DamageInfo.Source"/>.</summary>
    public static class DamageSource
    {
        public const ushort Unknown = 0;

        /// <summary>The player's own weapon swing.</summary>
        public const ushort Melee = 1;

        /// <summary>A stable id for an ability (FNV-1a of its id string, never 0 or 1).</summary>
        public static ushort OfAbility(string abilityId)
        {
            if (string.IsNullOrEmpty(abilityId)) return Unknown;
            var hash = 2166136261u;
            for (var i = 0; i < abilityId.Length; i++) hash = (hash ^ abilityId[i]) * 16777619u;
            var id = (ushort)(hash ^ (hash >> 16));
            return id <= Melee ? (ushort)(id + 2) : id;
        }
    }
}
