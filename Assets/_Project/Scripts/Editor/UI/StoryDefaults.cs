using GraceRun.UI.Story;

namespace GraceRun.Editor.UI
{
    /// <summary>The words of docs/LORE.md, used to fill a brand-new <c>StoryData</c> asset.</summary>
    internal static class StoryDefaults
    {
        public static void Fill(StoryData data)
        {
            var opening = new[]
            {
                Card("BOPLBORNE", "The bell has been quiet a long time."),
                Card("THE GATE", "Nobody remembers who took the clapper."),
                Card("THE KEY-RING", "Locked. Naturally."),
                Card("A SMALLER WAY", "There is always a smaller way in."),
                Card("THE HUB", "No way back but up."),
            };
            var ascent = new[]
            {
                "Floor 1. The key-holder is awake.",
                "Floor 2. Something up here is knitting. A lost hat lies on the stairs.",
                "Floor 3. Do you hear that? Neither do the bells.",
                "Floor 4. The weather is indoors.",
                "Floor 5. Everyone looks like somebody up here.",
                "Floor 6. Tick. Tock. Tick. Tick. Tock.",
                "Floor 7. Someone has been up here before you. Someone with a lot of hats.",
            };
            var ending = new[]
            {
                Card("THE CROWN COMES OFF", "Under all those hats: one small pilgrim, who hands over the clapper."),
                Card("THE BELL RINGS", "Every hat that climbed, in a row."),
            };
            data.Configure(opening, ascent, "The top. The crown is waiting.",
                "It wears every hat it ever found. Some of them look familiar.",
                ending, Card("TO BE CONTINUED...", "The lantern goes out. The tower closes over."),
                null, null, null, null, null, new UnityEngine.Sprite[0], new UnityEngine.Vector2[0]);
        }

        private static StoryCard Card(string title, string caption) => new() { Title = title, Caption = caption };
    }
}
