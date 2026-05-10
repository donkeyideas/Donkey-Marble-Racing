using UnityEngine;

namespace MarbleRace.Runtime.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource crowdSource;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip countdownBeep;
        [SerializeField] private AudioClip countdownGo;
        [SerializeField] private AudioClip marbleCollision;
        [SerializeField] private AudioClip winFanfare;
        [SerializeField] private AudioClip loseBuzz;
        [SerializeField] private AudioClip coinReward;
        [SerializeField] private AudioClip betPlaced;
        [SerializeField] private AudioClip buttonTap;

        [Header("Crowd")]
        [SerializeField] private AudioClip crowdCheer;
        [SerializeField] private AudioClip crowdGasp;
        [SerializeField] private AudioClip crowdAmbience;

        [Header("Settings")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void PlayCountdownBeep() => PlaySFX(countdownBeep);
        public void PlayCountdownGo() => PlaySFX(countdownGo);
        public void PlayMarbleCollision() => PlaySFX(marbleCollision, 0.5f);
        public void PlayWinFanfare() => PlaySFX(winFanfare);
        public void PlayLoseBuzz() => PlaySFX(loseBuzz);
        public void PlayCoinReward() => PlaySFX(coinReward);
        public void PlayBetPlaced() => PlaySFX(betPlaced);
        public void PlayButtonTap() => PlaySFX(buttonTap);

        public void PlayCrowdCheer() => PlayCrowd(crowdCheer);
        public void PlayCrowdGasp() => PlayCrowd(crowdGasp);

        public void StartCrowdAmbience()
        {
            if (crowdSource == null || crowdAmbience == null) return;
            crowdSource.clip = crowdAmbience;
            crowdSource.loop = true;
            crowdSource.Play();
        }

        public void StopCrowdAmbience()
        {
            if (crowdSource != null)
                crowdSource.Stop();
        }

        private void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        }

        private void PlayCrowd(AudioClip clip)
        {
            if (crowdSource == null || clip == null) return;
            crowdSource.PlayOneShot(clip);
        }
    }
}
