using UnityEngine;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    const float DEFAULT_BGM_VOL = 0.05f;
    const float DEFAULT_SFX_VOL = 1.0f;

    private DateTime walkingStartTime;

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
    // private AudioSource sfxSource;
    private AudioSource walkingSource; // 걷는 소리 전용 AudioSource


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
        bgmSource.loop = true;

        walkingSource = gameObject.AddComponent<AudioSource>();
        walkingSource.loop = true;

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
    public void PlayBGM(string bgmName, float volume = DEFAULT_BGM_VOL)
    {
        AudioClip clip = LoadAudioClip($"Audio/BGM/{bgmName}");
        if (clip != null && bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.volume = volume;
            bgmSource.Play(); // BGM 또는 오디오 루프 재생 (현재 오디오 중단되고 새 오디오가 재생됨)
        }
    }

    // 걷는 소리 재생
    public void StartWalkingSound(string clipName)
    {
        AudioClip clip = LoadAudioClip($"Audio/SFX/{clipName}");
        if (clip != null && walkingSource.clip != clip)
        {
            // 이미 재생 중인 소리가 같은 클립일 경우 다시 재생하지 않음
            if (walkingSource.isPlaying && walkingSource.clip == clip)
            {
                return;
            }

            // 클립 설정 및 재생
            walkingSource.clip = clip;
            walkingSource.loop = true; // 루프 활성화
            walkingSource.Play();
            walkingStartTime = DateTime.Now;
        }
    }

    // 걷는 소리 중지
    public void StopWalkingSound()
    {
        if (walkingSource.isPlaying)
        {

            DateTime endTime = DateTime.Now;

            TimeSpan timeDiff = endTime - walkingStartTime;

            if (timeDiff.Milliseconds <= 200)
            {
                DelayedExecutor.ExecuteAfterDelay(200 - timeDiff.Milliseconds, () =>
                {
                    walkingSource.Stop();
                    walkingSource.clip = null; // 클립 초기화
                });
            }
            else
            {
                walkingSource.Stop();
                walkingSource.clip = null; // 클립을 해제하여 상태 초기화
            }

        }
    }

    // public void StartShootSound(string clipName)
    // {
    //     AudioClip clip = LoadAudioClip($"Audio/SFX/{clipName}");

    // }
}