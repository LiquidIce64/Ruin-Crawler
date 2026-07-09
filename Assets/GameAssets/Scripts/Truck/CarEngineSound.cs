using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarEngineSound : MonoBehaviour
{
    [SerializeField] private VehicleController vehicleController;
    private float minPitch = 0.5f;
    private float maxPitch = 1.8f;
    private float pitchSmoothTime = 1f;
    private float maxVolume = 0.5f;
    private float fadeOutDuration = 3f;
    private float volumeRecoverySpeed = 8f;

    private AudioSource audioSource;
    private float targetPitch;
    private float pitchVelocity;

    private float airTime;
    private float volumeAtLiftoff;
    private bool wasGrounded;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = maxVolume;
        audioSource.Play();
        wasGrounded = true;
    }

    private void Update()
    {
        if (vehicleController == null) return;

        bool grounded = vehicleController.IsGrounded;

        if (!grounded)
        {
            if (wasGrounded)
            {
                airTime = 0f;
                volumeAtLiftoff = audioSource.volume;
            }
            airTime += Time.deltaTime;
            float t = Mathf.Clamp01(airTime / fadeOutDuration);
            audioSource.volume = Mathf.Lerp(volumeAtLiftoff, 0f, t);
        }
        else
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, maxVolume, volumeRecoverySpeed * Time.deltaTime);
            airTime = 0f;
        }

        targetPitch = grounded
            ? Mathf.Lerp(minPitch, maxPitch, vehicleController.CurrentSpeedNormalized)
            : minPitch;
        audioSource.pitch = Mathf.SmoothDamp(audioSource.pitch, targetPitch, ref pitchVelocity, pitchSmoothTime);

        wasGrounded = grounded;
    }
}