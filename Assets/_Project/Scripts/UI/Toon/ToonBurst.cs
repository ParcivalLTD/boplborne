using UnityEngine;

namespace GraceRun.UI.Toon
{
    /// <summary>A pop of stars and sparkles (a card picked, a button confirmed, a run started). Cosmetic.</summary>
    public static class ToonBurst
    {
        public static void Play(Transform parent, Vector2 anchoredPosition, int count = 10, float speed = 520f)
        {
            var theme = ToonTheme.Current;
            if (theme == null) return;
            for (var i = 0; i < count; i++)
            {
                var angle = Random.Range(0f, Mathf.PI * 2f);
                var velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(speed * 0.4f, speed) + Vector2.up * 220f;
                var sprite = i % 3 == 0 ? theme.Sparkle : i % 2 == 0 ? theme.StarOrange : theme.Star;
                ToonSpark.Spawn(parent, anchoredPosition, sprite, velocity, Random.Range(0.55f, 0.9f), Random.Range(30f, 52f));
            }
        }
    }
}
