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
    private float audioDuration = 1.5f;

    [Header("Finding Volume")]
    private int sampleSize = 256; // how many samples to read
    private float[] samples;
    private float rmsValue;
    private float scaledValue;
    public float multiplier = 20f; // adjust sensitivity
    public Image volumeBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundONE.Play();
        samples = new float[sampleSize];
    }

    // Update is called once per frame
    void Update()
    {
        PlayAudio();
        PlayStepAudio();
        GetRMS();
    }

    private void PlayAudio()
    {
        if (!soundONE.isPlaying)
        {
            soundONE.Play();
        }
    }

    private void PlayStepAudio()
    {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)){
            if (!soundTWO.isPlaying){
                soundTWO.time = 0.2f;
                soundTWO.Play();
            }
            if(audioDuration > 0) audioDuration -= Time.deltaTime;
            else{
                soundTWO.Stop();
                audioDuration = Input.GetKey(KeyCode.LeftShift) ? 1f : 1.5f;
            }
        }
        else{
            soundTWO.Stop();
            audioDuration = Input.GetKey(KeyCode.LeftShift) ? 1f : 1.5f;
        }
    }

    private void GetRMS()
    {
        if (!soundONE.isPlaying) return;

        soundONE.GetOutputData(samples, 0);

        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        rmsValue = Mathf.Sqrt(sum / samples.Length);

        // scaled version for use in gameplay
        scaledValue = rmsValue * multiplier;
        volumeBar.fillAmount = scaledValue;
    }
}
