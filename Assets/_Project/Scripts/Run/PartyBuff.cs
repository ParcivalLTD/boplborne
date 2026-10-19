namespace GraceRun.Run
{
    /// <summary>
    /// One-off party buffs purchasable at rest stops for the next boss fight (docs/GDD.md).
    /// </summary>
    public enum PartyBuff : byte
    {
        None = 0,
        EmpoweredAttacks = 1, // +20% damage to boss
        IronSkin = 2,         // +25 Max HP for the next fight
        Swiftstride = 3,      // +15% move speed & +25% stamina recovery
    }
}
