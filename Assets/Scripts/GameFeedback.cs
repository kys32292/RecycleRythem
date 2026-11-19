using System;
using UnityEngine;

public enum NoteImpact
{
    Deflect,
    Car,
    Plant
}

// Resources/GameFeedback holds the shared mix and the particle systems used at the hit box.
public sealed class GameFeedback : ScriptableObject
{
    [Serializable]
    private class Sound
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [NonSerialized] private int lastFrame = -1;

        public void Play(AudioSource source)
        {
            // Several title images or remix notes can arrive together.
            if (clip == null || source == null || lastFrame == Time.frameCount)
                return;

            lastFrame = Time.frameCount;
            source.PlayOneShot(clip, volume);
        }
    }

    [Serializable]
    private sealed class Impact : Sound
    {
        public ParticleSystem burst;
        [Min(0.01f)] public float scale = 1f;
        public float heightAboveHitBox;
    }

    [SerializeField] private Sound click;
    [SerializeField] private Sound imageArrival;
    [SerializeField] private Sound resultSuccess;
    [SerializeField] private Sound resultFailure;
    [SerializeField] private Sound missedNote;
    [SerializeField] private Impact car;
    [SerializeField] private Impact plant;
    [SerializeField] private Color soilColor;
    [SerializeField] private Impact deflect;

    public static GameFeedback Current { get; private set; }
    private AudioSource source;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Current = Resources.Load<GameFeedback>("GameFeedback");
        if (Current == null)
        {
            Debug.LogWarning("Resources/GameFeedback is missing.");
            return;
        }

        var player = new GameObject("Game SFX", typeof(AudioSource));
        DontDestroyOnLoad(player);
        Current.source = player.GetComponent<AudioSource>();
        Current.source.playOnAwake = false;
        Current.source.spatialBlend = 0f;
    }

    public void PlayClick() => click.Play(source);
    public void PlayImageArrival() => imageArrival.Play(source);
    public void PlayMiss() => missedNote.Play(source);
    public void PlayResult(bool success) => (success ? resultSuccess : resultFailure).Play(source);

    public void PlayHit(NoteImpact kind, Transform hitBox)
    {
        Impact impact = kind switch
        {
            NoteImpact.Car => car,
            NoteImpact.Plant => plant,
            _ => deflect
        };

        impact.Play(source);
        if (hitBox == null || impact.burst == null)
            return;

        Vector3 position = hitBox.position + Vector3.up * impact.heightAboveHitBox;
        var burst = Instantiate(impact.burst, position, hitBox.rotation);
        burst.transform.localScale *= impact.scale;
        burst.transform.SetParent(hitBox, true);
        if (kind == NoteImpact.Plant)
        {
            var particles = burst.main;
            particles.startColor = soilColor;
        }
        // The projectile pack stores its fire and wind impact roots inactive.
        burst.gameObject.SetActive(true);
        burst.Play(true);
        // These PixPlays bursts emit once. Keep their tails independent of note cleanup.
        Destroy(burst.gameObject, 2f);
    }
}
