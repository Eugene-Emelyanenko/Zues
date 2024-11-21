using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private Button[] musicButtons;
    [SerializeField] private Button[] sfxButtons;
    [SerializeField] private Button[] vibroButtons;
    [SerializeField] private Sprite[] onSprites;
    [SerializeField] private Sprite[] offSprites;

    private bool vibrateOn = true;
    private bool musicOn = true;
    private bool sfxOn = true;

    private void Start()
    {
        ToogleSettingsMenu(false);
        foreach (Button b in musicButtons)
        {
            b.onClick.AddListener(ToogleMusic);
        }
        foreach (Button b in sfxButtons)
        {
            b.onClick.AddListener(ToogleSfx);
        }
        foreach (Button b in vibroButtons)
        {
            b.onClick.AddListener(ToogleVibro);
        }
    }

    public void ToogleSettingsMenu(bool isOpen)
    {
        settingsMenu.SetActive(isOpen);
        musicOn = PlayerPrefs.GetInt("Music", 1) == 1;
        vibrateOn = PlayerPrefs.GetInt("Vibrate", 1) == 1;
        sfxOn = PlayerPrefs.GetInt("Sfx", 1) == 1;

        UpdateSettings();
    }

    private void UpdateSettings()
    {
        UpdateButtonState(musicButtons, musicOn);
        UpdateButtonState(sfxButtons, sfxOn);
        UpdateButtonState(vibroButtons, vibrateOn);

        PlayerPrefs.SetInt("Music", musicOn ? 1 : 0);

        PlayerPrefs.SetInt("Sfx", sfxOn ? 1 : 0);

        PlayerPrefs.SetInt("Vibrate", vibrateOn ? 1 : 0);

        PlayerPrefs.Save();

        if (!musicOn)
            SoundManager.Instance.TurnOffMusic();
        else
            SoundManager.Instance.TurnOnMusic();

        if (!sfxOn)
            SoundManager.Instance.TurnOffSfx();
        else
            SoundManager.Instance.TurnOnSfx();
    }

    private void UpdateButtonState(Button[] buttons, bool isOn)
    {
        buttons[1].image.sprite = isOn ? onSprites[1] : onSprites[0];
        buttons[0].image.sprite = isOn ? offSprites[0] : offSprites[1];
    }

    public void ToogleMusic()
    {
        musicOn = !musicOn;
        UpdateSettings();
    }

    public void ToogleSfx()
    {
        sfxOn = !sfxOn;
        UpdateSettings();
    }

    public void ToogleVibro()
    {
        vibrateOn = !vibrateOn;
        if (vibrateOn)
            Handheld.Vibrate();
        UpdateSettings();
    }
}
