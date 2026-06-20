using UnityEngine;
using UnityEngine.Audio;

public class MusicSystem : MonoBehaviour
{
    private AudioSource audioSource;
    private bool isLoop;

    public void InitSystem(AudioMixerGroup sfxMixerGroup, bool loop = false)
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.volume = 1f;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        isLoop = loop;
    }
    
    public void SetLoop(bool loop)
    {
        isLoop = loop;
        audioSource.loop = loop;
    }
    
    public void SetClip(AudioClip clip)
    {
        audioSource.clip = clip;
    }
    
    public void PlaySound()
    {
        audioSource.Play();
    }
    
    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
    
    public void ShotSound(AudioClip sound, float volume = 1f)
    {
        audioSource.PlayOneShot(sound, volume);
    }
    
    public void StopSound() 
    { 
        audioSource.Pause(); 
    }
    
    public void ResumeSound() 
    { 
        audioSource.UnPause(); 
    }
    
    public void ClearSound()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }
    
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
    
    public void SetPitch(float pitch)
    {
        audioSource.pitch = pitch;
    }
    
    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }
}