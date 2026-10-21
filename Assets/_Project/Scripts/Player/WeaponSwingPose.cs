using UnityEngine;

namespace GraceRun.Player
{
    /// <summary>Where a weapon's grip is and which way it points, relative to the player, plus the smear's strength.</summary>
    public readonly struct WeaponPose
    {
        public readonly Vector2 Position;

        /// <summary>Degrees, 0 = pointing right.</summary>
        public readonly float Angle;

        /// <summary>0 = no smear, 1 = full.</summary>
        public readonly float Smear;

        public WeaponPose(Vector2 position, float angle, float smear)
        {
            Position = position;
            Angle = angle;
            Smear = smear;
        }
    }

    /// <summary>The look of one weapon's swing, from its <see cref="WeaponData"/>.</summary>
    public readonly struct WeaponSwingLook
    {
        public readonly WeaponSwingStyle Style;

        /// <summary>Arc: how far above the aim the weapon is raised in the wind-up, in degrees.</summary>
        public readonly float WindAngle;

        /// <summary>Arc: how far it sweeps in total, in degrees.</summary>
        public readonly float SweepAngle;

        /// <summary>Arc: how far the grip sits from the player's centre. Thrust: how far the grip is pushed out.</summary>
        public readonly float Reach;

        public WeaponSwingLook(WeaponSwingStyle style, float windAngle, float sweepAngle, float reach)
        {
            Style = style;
            WindAngle = windAngle;
            SweepAngle = sweepAngle;
            Reach = reach;
        }
    }

    /// <summary>
    /// Pure animation of a held weapon from the swing phase: rest on the back, wind-up, strike,
    /// recovery. <paramref name="side"/> is +1 aiming right and -1 aiming left; every angle mirrors
    /// with it. No Unity objects, so it is tested in EditMode.
    /// </summary>
    public static class WeaponSwingPose
    {
        private const float PullBack = -0.25f;
        private const float RecoverySmearFade = 0.3f;

        public static WeaponPose Evaluate(WeaponSwingLook look, SwingPhase phase, float progress, float aimAngle, float side, WeaponPose rest)
        {
            if (phase == SwingPhase.Idle) return rest;
            progress = Mathf.Clamp01(progress);
            return look.Style == WeaponSwingStyle.Thrust
                ? Thrust(look, phase, progress, aimAngle, rest)
                : Arc(look, phase, progress, aimAngle, side, rest);
        }

        private static WeaponPose Arc(WeaponSwingLook look, SwingPhase phase, float t, float aim, float side, WeaponPose rest)
        {
            var windAngle = aim + side * look.WindAngle;
            var endAngle = windAngle - side * look.SweepAngle;
            var wind = new WeaponPose(Along(windAngle, look.Reach), windAngle, 0f);
            var end = new WeaponPose(Along(endAngle, look.Reach), endAngle, 0f);
            switch (phase)
            {
                case SwingPhase.Startup:
                    return Lerp(rest, wind, EaseOut(t));
                case SwingPhase.Active:
                {
                    var angle = Mathf.Lerp(windAngle, endAngle, EaseIn(t));
                    return new WeaponPose(Along(angle, look.Reach), angle, 1f - 0.5f * t);
                }
                default:
                    return Lerp(new WeaponPose(end.Position, end.Angle, 0.5f * (1f - Mathf.Clamp01(t / RecoverySmearFade))), rest, EaseInOut(t));
            }
        }

        private static WeaponPose Thrust(WeaponSwingLook look, SwingPhase phase, float t, float aim, WeaponPose rest)
        {
            var back = new WeaponPose(Along(aim, PullBack), aim, 0f);
            var front = new WeaponPose(Along(aim, look.Reach), aim, 1f);
            switch (phase)
            {
                case SwingPhase.Startup:
                    return Lerp(rest, back, EaseOut(t));
                case SwingPhase.Active:
                    return Lerp(back, front, EaseOut(t));
                default:
                    return Lerp(new WeaponPose(front.Position, front.Angle, 0.5f * (1f - Mathf.Clamp01(t / RecoverySmearFade))), rest, EaseInOut(t));
            }
        }

        private static Vector2 Along(float angle, float distance) =>
            new(Mathf.Cos(angle * Mathf.Deg2Rad) * distance, Mathf.Sin(angle * Mathf.Deg2Rad) * distance);

        private static WeaponPose Lerp(WeaponPose a, WeaponPose b, float t) =>
            new(Vector2.Lerp(a.Position, b.Position, t), Mathf.LerpAngle(a.Angle, b.Angle, t), Mathf.Lerp(a.Smear, b.Smear, t));

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);

        private static float EaseIn(float t) => t * t;

        private static float EaseInOut(float t) => t * t * (3f - 2f * t);
    }
}
