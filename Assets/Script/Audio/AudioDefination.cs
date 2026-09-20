using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDefination : MonoBehaviour
{
    public PlayAudioEventSO palyAudioEvent;//创建事件
    public AudioClip audioClip;//音频片段
    public bool playOnable;//判断音频是否在一开始播放

    private void OnEnable()
    {
        if (playOnable)
            PlayAudioClip();
    }
    public void PlayAudioClip()
    {
        palyAudioEvent.RaiseEvent(audioClip);
    }
}
