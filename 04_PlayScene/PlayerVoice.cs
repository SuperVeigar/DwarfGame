using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVoice : MonoBehaviour
{
    public AudioClip m_jumpVoice;
    public AudioClip m_fallbackVoice;
    public AudioClip m_attackStaicboltVice;
    public AudioClip[] m_attackVoice;
    public AudioClip[] m_softdamageVoice;
    public AudioClip[] m_harddamageVoice;
    public AudioClip[] m_dieVoice;
    public AudioClip[] m_furyVoice;
    public AudioClip[] m_walkFootStepSound;
    public AudioClip[] m_runFootStepSound;
    public AudioClip[] m_jumpFootStepSound;

    private enum AudiosourceNumber { Voice = 0, Anim, Something }
    private int m_audiosourceCount;
    private AudioSource[] m_audiosources;   // 0:Voice    1:Anim    2:

    // Start is called before the first frame update
    void Start()
    {
        m_audiosources = GetComponents<AudioSource>();
        m_audiosourceCount = m_audiosources.Length;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool IsAvailableVoiceAudioSource()
    {
        if (m_audiosourceCount > (int)AudiosourceNumber.Voice &&
           !CommonSoundManager_DontDest.instance.isMute) return true;

        return false;
    }

    bool IsAvailableAnimAudioSource()
    {
        if (m_audiosourceCount > (int)AudiosourceNumber.Anim &&
           !CommonSoundManager_DontDest.instance.isMute) return true;

        return false;
    }

    public void PlayJumpVoice()
    {
        if(IsAvailableVoiceAudioSource()) m_audiosources[(int)AudiosourceNumber.Voice].PlayOneShot(m_jumpVoice);
    }

    public void PlaySoftDamageVoice()
    {
        int num = Random.Range(0, 4);

        if (IsAvailableVoiceAudioSource()) m_audiosources[(int)AudiosourceNumber.Voice].PlayOneShot(m_softdamageVoice[num]);
    }

    public void PlayHardDamageVoice()
    {
        int num = Random.Range(0, 2);

        if (IsAvailableVoiceAudioSource()) m_audiosources[(int)AudiosourceNumber.Voice].PlayOneShot(m_harddamageVoice[num]);
    }

    public void PlayDieVoice()
    {
        int num = Random.Range(0, 2);

        if (IsAvailableVoiceAudioSource()) m_audiosources[(int)AudiosourceNumber.Voice].PlayOneShot(m_dieVoice[num]);
    }

    public void PlayAttackVoice()
    {
        int num = Random.Range(0, 6);

        if (IsAvailableVoiceAudioSource()) m_audiosources[(int)AudiosourceNumber.Voice].PlayOneShot(m_attackVoice[num]);
    }

    public void PlayFallbackVoice()
    {
        if (IsAvailableVoiceAudioSource()) m_audiosources[(int)AudiosourceNumber.Voice].PlayOneShot(m_fallbackVoice);
    }

    public void PlayFuryVoice()
    {
        int num = Random.Range(0, 2);

        if (IsAvailableVoiceAudioSource()) m_audiosources[(int)AudiosourceNumber.Voice].PlayOneShot(m_furyVoice[num]);
    }

    public void PlayAttackStaticboltVoice()
    {
        if (IsAvailableVoiceAudioSource()) m_audiosources[(int)AudiosourceNumber.Voice].PlayOneShot(m_attackStaicboltVice);
    }

    public void PlayWalkLeftFootStepSound()
    {
        if (IsAvailableAnimAudioSource()) m_audiosources[(int)AudiosourceNumber.Anim].PlayOneShot(m_walkFootStepSound[0]);
    }

    public void PlayWalkRightFootStepSound()
    {
        if (IsAvailableAnimAudioSource()) m_audiosources[(int)AudiosourceNumber.Anim].PlayOneShot(m_walkFootStepSound[1]);
    }

    public void PlayRunLeftFootStepSound()
    {
        int num = Random.Range(0, 2);

        if (IsAvailableAnimAudioSource()) m_audiosources[(int)AudiosourceNumber.Anim].PlayOneShot(m_runFootStepSound[num * 2]);
    }

    public void PlayRunRightFootStepSound()
    {
        int num = Random.Range(0, 2);

        if (IsAvailableAnimAudioSource()) m_audiosources[(int)AudiosourceNumber.Anim].PlayOneShot(m_runFootStepSound[num * 2 + 1]);
    }

    public void PlayJumpStartFootStepSound()
    {
        if (IsAvailableAnimAudioSource()) m_audiosources[(int)AudiosourceNumber.Anim].PlayOneShot(m_jumpFootStepSound[0]);
    }

    public void PlayJumpEndFootStepSound()
    {
        if (IsAvailableAnimAudioSource()) m_audiosources[(int)AudiosourceNumber.Anim].PlayOneShot(m_jumpFootStepSound[1]);
    }
}
