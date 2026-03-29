using System;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// 音频管理器：控制音乐与音效的播放与开关。
/// </summary>
public class AudioManager : MonoBehaviour
{
    [LabelText("音效剪辑列表")]
    public AudioClip[] audioClips;

    [LabelText("音效与音乐音源")]
    public AudioSource soundAudioSource, musicAudioSource;

    [LabelText("其他需静音控制的音源")]
    public AudioSource[] otherSounds;

    [LabelText("音乐开关状态")]
    bool musicToggle = true;

    [LabelText("音效开关状态")]
    bool soundToggle = true;

    [LabelText("音乐开启图标")]
    public Sprite musicOn, musicOff;

    [LabelText("音效开启图标")]
    public Sprite soundOn, soundOff;

    [LabelText("音乐按钮图片")]
    public Image musicBtnImage;

    [LabelText("音效按钮图片")]
    public Image soundBtnImage;

    [LabelText("全局音频管理器单例")]
    public static AudioManager Instance;

    /// <summary>
    /// 按名字播放音效。
    /// </summary>
    /// <param name="name">音效名称，对应 AudioClip.name。</param>
    public void Play(string name)
    {
        AudioClip clip = Array.Find(audioClips, sound => sound.name == name);

        if(soundAudioSource.clip != clip)
        soundAudioSource.clip = clip;

        soundAudioSource.Play();
    }

    /// <summary>
    /// 叠加播放音效（不替换当前主 clip），适合连续飞币等短音效。
    /// </summary>
    public void PlayOneShot(string name, float volumeScale = 1f)
    {
        if (soundAudioSource == null || soundAudioSource.mute)
            return;

        AudioClip clip = Array.Find(audioClips, sound => sound.name == name);
        if (clip != null)
            soundAudioSource.PlayOneShot(clip, volumeScale);
    }

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey("Music"))
            MusicToggle();

        if (PlayerPrefs.HasKey("Sound"))
            SoundToggle();
    }

    public void MusicToggle()
    {
        musicToggle = !musicToggle;
        if (musicToggle)
        {
            musicBtnImage.sprite = musicOn;
            musicAudioSource.mute = false;
            PlayerPrefs.DeleteKey("Music");
        }
        else
        {
            musicBtnImage.sprite = musicOff;
            musicAudioSource.mute = true;
            PlayerPrefs.SetString("Music", "");
        }
    }

    public void SoundToggle()
    {
        soundToggle = !soundToggle;
        if (soundToggle)
        {
            soundBtnImage.sprite = soundOn;
            soundAudioSource.mute = false;

            foreach(AudioSource audio in otherSounds)
            {
                audio.mute = false;
            }

            PlayerPrefs.DeleteKey("Sound");
        }
        else
        {
            soundBtnImage.sprite = soundOff;
            soundAudioSource.mute = true;

            foreach (AudioSource audio in otherSounds)
            {
                audio.mute = true;
            }

            PlayerPrefs.SetString("Sound", "");
        }
    }
}
