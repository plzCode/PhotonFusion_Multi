using Fusion;
using FusionExamples.Utility;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [SerializeField] private ProfileSetupUI profileSetup;

    public UIScreen mainMenu;
    public UIScreen pauseMenu;
    public UIScreen lobbyMenu;
    public CharacterHUDContainer_UI characterHUDcontainter;
    public CharacterHUDUnit[] characterHUDUnits;
    public CountdownTimer countdownTimer;
    public AmmoDisplay_UI ammoDisplay;
    public Image fadeImage;

    public static InterfaceManager Instance => Singleton<InterfaceManager>.Instance;

    private void Start()
    {
        profileSetup.AssertProfileSetup();
    }

    public void OpenPauseMenu()
    {
        // open pause menu only if the kart can drive and the menu isn't open already
        if (UIScreen.activeScreen != pauseMenu)
        {
            UIScreen.Focus(pauseMenu);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    // Audio Hooks
    public void SetVolumeMaster(float value) => AudioManager.SetVolumeMaster(value);
    public void SetVolumeSFX(float value) => AudioManager.SetVolumeSFX(value);
    public void SetVolumeUI(float value) => AudioManager.SetVolumeUI(value);
    public void SetVolumeMusic(float value) => AudioManager.SetVolumeMusic(value);

    
    public void ResetUI()
    {
        // 스탯 UI 연결 ( 다른 플레이어 )
        for (int i = 0; i < 3; i++)
        {
            characterHUDUnits[i].player = null;
            characterHUDUnits[i].gameObject.SetActive(false);
        }
    }
}