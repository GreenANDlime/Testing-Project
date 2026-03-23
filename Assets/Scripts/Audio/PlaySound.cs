using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlaySound : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource soundONE;
    [SerializeField] private AudioSource soundTWO;
    private bool playAudio;
    private float volumeLevel;
    private float audioDuration = 1.5f;

    [Header("Finding Volume")]
    private int sampleSize = 256; // how many samples to read
    public float multiplier = 20f; // adjust sensitivity
    public Image volumeBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundONE.Play();
    }

    // Update is called once per frame
    void Update()
    {
        PlayAudio();
        PlayStepAudio();
    }

    private void PlayAudio()
    {
        if (!soundONE.isPlaying)
        {
            soundONE.Play();
        }
        PlayStepAudio();
        volumeLevel = GetRMS(soundONE) + GetRMS(soundTWO);
        volumeBar.fillAmount = volumeLevel;

    }

    private void PlayStepAudio()
    {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)){
            if (!soundTWO.isPlaying){
                if(Input.GetKey(KeyCode.C)){ 
                    soundTWO.volume = 0.3f;
                    soundTWO.pitch = 0.3f;
                }
                else soundTWO.pitch = 1f;

                soundTWO.time = 0.2f;
                soundTWO.Play();
            }
            if(audioDuration > 0) audioDuration -= Time.deltaTime;
            else{
                soundTWO.Stop();
                soundTWO.volume = 1f;
                audioDuration = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 2f;
            }
        }
        else{
            if(soundTWO.isPlaying) soundTWO.volume = Mathf.MoveTowards(soundTWO.volume, 0, Time.deltaTime / 2);
            if(soundTWO.volume == 0){
                soundTWO.Stop();
                soundTWO.volume = 1f;
            }
            audioDuration = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 2f;
        }
    }

    private float GetRMS(AudioSource source)
    {
        float[] samples = new float[sampleSize];
        if (!source.isPlaying) return 0;

        source.GetOutputData(samples, 0);

        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        float rmsValue = Mathf.Sqrt(sum / samples.Length);

        // scaled version for use in gameplay
        float scaledValue = rmsValue * multiplier;
        return scaledValue;
    }

    public float ReturnVolume()
    {
        return volumeLevel;
    }
}
