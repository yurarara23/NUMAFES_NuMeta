using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Tablet : UdonSharpBehaviour
{
    public AudioClip[] audioClips;
    public Sprite[] albumArt;
    
    public AudioSource audioSource;
    public Image displayImage;

    public GameObject playButton;
    public GameObject stopButton;
    public GameObject backButton;
    public GameObject skipButton;

    private int currentIndex = 0;
    private bool isPlaying = false;

    private void UpdateButtons()
    {
        if (playButton != null && stopButton != null)
        {
            playButton.SetActive(!isPlaying);
            stopButton.SetActive(isPlaying);
        }
    }

    public void PlaySound()
    {
        if (audioSource == null || audioClips.Length == 0) return;

        if (currentIndex < albumArt.Length && displayImage != null)
        {
            displayImage.sprite = albumArt[currentIndex];
        }

        audioSource.clip = audioClips[currentIndex];
        audioSource.Play();
        
        isPlaying = true;
        UpdateButtons();
    }

    public void StopSound()
    {
        if (audioSource == null) return;

        audioSource.Stop();
        
        isPlaying = false;
        UpdateButtons();
    }

    public void SkipSound()
    {
        currentIndex = (currentIndex + 1) % audioClips.Length;
        PlaySound();
    }

    public void BackSound()
    {
        currentIndex = (currentIndex - 1 + audioClips.Length) % audioClips.Length;
        PlaySound();
    }
    
    void Start()
    {
        isPlaying = false;
        PlaySound();
        UpdateButtons();
    }
}