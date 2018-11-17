﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/*
 * 사운드 분류 추가 할 때 해야 할 3가지
 * 1. SOUNDTYPE Enum 추가
 * 2. AudioClip[] 만들기
 * 3. Play 함수에서 switch안에 내용 추가 하기
 */

public enum SOUNDTYPE
{
    WEAPON,
    UI
}

[RequireComponent(typeof(AudioSource))]
public class SoundController : MonoBehaviourSingleton<SoundController>
{
    //[Header("[PlayerPrefs Key]")]
    //[SerializeField] private string saveKey = "Option_Sound";
    [SerializeField]
    private AudioClip[] uiSoundList;
    [SerializeField]
    private AudioClip[] weaponSoundList;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // 사운드 On/Off 여부 확인, 임시로 true
    public bool IsEnableSound()
    {
        return true;
        //return PlayerPrefs.GetInt(saveKey, 1) == 1 ? true : false;
    }

    /*
    // 사운드 설정 저장
    public void EnableSound(bool enable)
    {
        PlayerPrefs.SetInt(saveKey, enable ? 1 : 0);
        PlayerPrefs.Save();
    }

    // 사운드 On/Off 토글
    public void ToggleSound()
    {
        EnableSound(!IsEnableSound());
    }*/


    /// <summary>
    /// 사운드 재생
    /// </summary>
    public void Play(int soundIndex, SOUNDTYPE soundType)
    {
        if (soundIndex < 0) return;

        AudioClip _clip = null;

        switch(soundType)
        {
            case SOUNDTYPE.WEAPON:
                _clip = weaponSoundList[soundIndex];
                break;
            case SOUNDTYPE.UI:
                _clip = uiSoundList[soundIndex];
                break;
            default:
                break;
        }

        if (_clip == null)
            return;

        audioSource.PlayOneShot(_clip);
    }
}
