using UnityEngine;

public class CustomUIScreen : UIScreen
{
    [field:SerializeField] public EScreenType ScreenType { get; private set; } = EScreenType.MainMenu;
    protected override void Focus()
    {
        base.Focus();
        
        if (LobbySceneCamController.Instance != null)
            LobbySceneCamController.Instance.ExecuteChangeScreenEffect(ScreenType);
    }
}
