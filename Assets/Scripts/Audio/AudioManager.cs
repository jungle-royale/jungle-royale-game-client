using UnityEngine;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            // _instance가 이미 생성되었다면 기존 객체를 반환하고, 생성되지 않았을 경우 새 객체를 생성
            if (_instance == null)
            {
                var audioManagerObject = new GameObject("AudioManager");
                _instance = audioManagerObject.AddComponent<AudioManager>();
                DontDestroyOnLoad(audioManagerObject); // 씬 전환 시에도 유지
            }
            return _instance;
        }
    }

    // BGM과 효과음을 재생할 AudioSource
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    // 오디오 클립 캐싱을 위한 딕셔너리
    private Dictionary<string, AudioClip> audioClips;

    void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 초기화
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true; // BGM은 반복 재생

        sfxSource = gameObject.AddComponent<AudioSource>();

        // 오디오 클립 딕셔너리 초기화
        audioClips = new Dictionary<string, AudioClip>();
    }

    // 오디오 클립을 Resources 폴더에서 로드하여 딕셔너리에 저장
    private AudioClip LoadAudioClip(string path)
    {
        if (audioClips.ContainsKey(path))
        {
            return audioClips[path];
        }
        else
        {
            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                audioClips[path] = clip;
            }
            else
            {
                Debug.LogWarning($"오디오 클립을 찾을 수 없습니다: {path}");
            }
            return clip;
        }
    }

    // BGM 재생
    public void PlayBGM(string bgmName)
    {
        AudioClip clip = LoadAudioClip($"Audio/BGM/{bgmName}");
        if (clip != null && bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    // 효과음 재생
    public void PlaySFX(string sfxName)
    {
        AudioClip clip = LoadAudioClip($"Audio/SFX/{sfxName}");
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}