using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class AudioManager : Singleton<AudioManager>
{
    // const float DEFAULT_BGM_VOL = 0.3f;
    // const float DEFAULT_SFX_VOL = 1.0f;

    private DateTime walkingStartTime;

    [Header("#BGM")]
    public AudioClip bgmClip;
    public float bgmVolume;
    AudioSource bgmPlayer;

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels; // 오디오 동시에 몇 개 플레이 할지
    AudioSource[] sfxPlayers;
    int channelIndex;

    public enum Sfx
    {
        Dash,
        Dead,
        GameCountDown,
        GameOver,
        GameStart,
        GetItem,
        Heal,
        ShootFire,
        ShootNormal,
        ShootStone,
        Walk,
        Win
    }

    // 싱글톤 인스턴스

    // BGM과 효과음을 재생할 AudioSource
    private AudioSource bgmSource;
    // private AudioSource sfxSource;
    private AudioSource walkingSource; // 걷는 소리 전용 AudioSource


    // 오디오 클립 캐싱을 위한 딕셔너리
    private Dictionary<string, AudioClip> audioClips;

    void Start()
    {
        // 기본값 설정
        if (channels <= 0)
        {
            channels = 10; // 기본 채널 개수
            Debug.Log($"채널 개수가 0이어서 기본값 {channels}으로 설정됨");
        }

        if (sfxVolume <= 0 || sfxVolume > 1)
        {
            sfxVolume = 1.0f; // 기본 볼륨
            Debug.Log($"SFX 볼륨이 잘못되어 기본값 {sfxVolume}으로 설정됨");
        }

        // AudioSource 초기화
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        walkingSource = gameObject.AddComponent<AudioSource>();
        walkingSource.loop = true;

        // 오디오 클립 딕셔너리 초기화
        audioClips = new Dictionary<string, AudioClip>();

        InitAudioManager();
    }

    private void InitAudioManager()
    {
        // 배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        // 효과음 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    public void PlaySfx(Sfx sfx, float? volume = null)
    {
        float setVolume = volume ?? sfxVolume; // 매개변수가 null이면 sfxVolume 사용

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
            {
                continue;
            }

            channelIndex = loopIndex;

            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx];
            if (sfxPlayers[loopIndex].clip == null)
            {
                Debug.LogError($"{(Sfx)sfx} 클립 없음");
                return;
            }

            sfxPlayers[loopIndex].volume = Mathf.Clamp(setVolume, 0f, 1f); // 볼륨 설정 (0 ~ 1 사이로 제한)
            sfxPlayers[loopIndex].Play();
            break;
        }
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
    public void PlayBGM(string bgmName, float? volume = null)
    {
        AudioClip newClip = LoadAudioClip($"Audio/BGM/{bgmName}");

        if (newClip != null)
        {
            if (bgmSource.isPlaying && bgmSource.clip == newClip)
            {
                Debug.Log($"BGM '{bgmName}' is already playing.");
                return; // 같은 BGM이 재생 중이면 아무 작업도 하지 않음
            }

            if (bgmSource.isPlaying)
            {
                Debug.Log($"Stopping current BGM: {bgmSource.clip.name}");
                bgmSource.Stop(); // 기존 BGM 멈춤
            }

            float setVolume = volume ?? bgmVolume; // 매개변수가 null이면 bgmVolume 사용
            bgmSource.clip = newClip; // 새로운 BGM 설정
            bgmSource.volume = Mathf.Clamp(setVolume, 0f, 1f); // 볼륨 설정
            bgmSource.Play(); // 새 BGM 재생
            Debug.Log($"Playing new BGM: {bgmName}");
        }
        else
        {
            Debug.LogWarning($"Failed to load BGM: {bgmName}");
        }
    }

    // 걷는 소리 재생
    public void StartWalkingSound()
    {
        AudioClip clip = LoadAudioClip($"Audio/SFX/WalkSFX");
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
}