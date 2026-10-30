using UnityEngine;

namespace MedievalJourney
{
    [DisallowMultipleComponent]
    public sealed class GameAudio : MonoBehaviour
    {
        [Header("Fontes")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource effectsSource;

        [Header("Musicas")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;

        [Header("Efeitos")]
        [SerializeField] private AudioClip swordAttack;
        [SerializeField] private AudioClip shieldDefense;
        [SerializeField] private AudioClip deathBell;

        public AudioSource MusicSource => musicSource;
        public AudioSource EffectsSource => effectsSource;
        public AudioClip MenuMusic => menuMusic;
        public AudioClip GameplayMusic => gameplayMusic;
        public AudioClip SwordAttack => swordAttack;
        public AudioClip ShieldDefense => shieldDefense;
        public AudioClip DeathBell => deathBell;
        public AudioClip CurrentMusic => musicSource != null ? musicSource.clip : null;
        public int SwordAttackPlayCount { get; private set; }
        public int ShieldDefensePlayCount { get; private set; }
        public int DeathBellPlayCount { get; private set; }

        public void Configure(AudioSource music, AudioSource effects, AudioClip menu,
            AudioClip gameplay, AudioClip sword, AudioClip shield, AudioClip death)
        {
            musicSource = music;
            effectsSource = effects;
            menuMusic = menu;
            gameplayMusic = gameplay;
            swordAttack = sword;
            shieldDefense = shield;
            deathBell = death;
        }

        public void HandlePhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.MainMenu:
                    PlayMusic(menuMusic);
                    break;
                case GamePhase.Introduction:
                case GamePhase.Won:
                case GamePhase.Lost:
                    StopMusic();
                    break;
                case GamePhase.Playing:
                    PlayMusic(gameplayMusic);
                    break;
                case GamePhase.Paused:
                    break;
            }
        }

        public void PlaySwordAttack()
        {
            if (PlayEffect(swordAttack))
            {
                SwordAttackPlayCount++;
            }
        }

        public void PlayShieldDefense()
        {
            if (PlayEffect(shieldDefense))
            {
                ShieldDefensePlayCount++;
            }
        }

        public void PlayDeathBell()
        {
            if (PlayEffect(deathBell))
            {
                DeathBellPlayCount++;
            }
        }

        private void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null)
            {
                return;
            }

            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        private void StopMusic()
        {
            if (musicSource == null)
            {
                return;
            }

            musicSource.Stop();
            musicSource.clip = null;
        }

        private bool PlayEffect(AudioClip clip)
        {
            if (effectsSource == null || clip == null)
            {
                return false;
            }

            effectsSource.PlayOneShot(clip);
            return true;
        }
    }
}
