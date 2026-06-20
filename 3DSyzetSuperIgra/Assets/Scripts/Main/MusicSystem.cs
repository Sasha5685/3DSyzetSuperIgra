using UnityEngine;
using UnityEngine.Audio;

public class MusicSystem : MonoBehaviour
{
    private AudioSource AudioSourse;

    public void InitSystem(AudioMixerGroup sfxMixerGroup)
    {
        AudioSourse = gameObject.AddComponent<AudioSource>();
        AudioSourse.playOnAwake = false;
        AudioSourse.loop = false;
        AudioSourse.volume = 1f;
        AudioSourse.outputAudioMixerGroup = sfxMixerGroup;
    }
    public void ShotSound(AudioClip Sound, float Volume)
    {
        AudioSourse.PlayOneShot(Sound, Volume);
    }
    public void ShotSound(AudioClip Sound)
    {
        AudioSourse.PlayOneShot(Sound, 1f);
    }
    public void StopSound()
    {
        AudioSourse.clip = null;    
    }

}
