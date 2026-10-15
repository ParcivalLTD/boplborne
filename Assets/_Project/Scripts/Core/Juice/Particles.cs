using System.Collections.Generic;
using UnityEngine;

namespace GraceRun.Core.Juice
{
    /// <summary>
    /// Plays one-shot particle prefabs (hit sparks, dust) from a small pool per prefab.
    /// Local cosmetic only; does nothing in batch mode (server-only builds).
    /// The prefab's stop action must be Disable so a finished instance can be reused.
    /// </summary>
    public static class Particles
    {
        private static readonly Dictionary<ParticleSystem, List<ParticleSystem>> Pools = new();

        /// <summary>Plays <paramref name="prefab"/> at a position, mirrored when <paramref name="facing"/> is negative.</summary>
        public static void Play(ParticleSystem prefab, Vector3 position, float facing = 1f)
        {
            if (prefab == null || Application.isBatchMode) return;

            var instance = Get(prefab);
            var rotation = facing < 0f ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            instance.Play(true);
        }

        private static ParticleSystem Get(ParticleSystem prefab)
        {
            if (!Pools.TryGetValue(prefab, out var pool))
            {
                pool = new List<ParticleSystem>();
                Pools.Add(prefab, pool);
            }

            pool.RemoveAll(p => p == null); // instances die with their scene
            foreach (var p in pool)
                if (!p.gameObject.activeSelf) return p;

            var created = Object.Instantiate(prefab);
            created.gameObject.SetActive(false);
            pool.Add(created);
            return created;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Pools.Clear();
    }
}
