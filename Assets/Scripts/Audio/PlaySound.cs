using UnityEngine;
using UnityEngine.InputSystem;

public class PlaySound : MonoBehaviour
{
    [SerializeField] private AudioSource soundONE;
    private bool playAudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundONE.Play();
    }

    // Update is called once per frame
    void Update()
    {
        PlayAudio();
    }

    private void PlayAudio()
    {
        if (!soundONE.isPlaying)
        {
            soundONE.Play();
        }
    }
}
