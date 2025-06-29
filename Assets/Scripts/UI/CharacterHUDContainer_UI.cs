using System.Collections.Generic;
using UnityEngine;

public class CharacterHUDContainer_UI : Base_UI
{
    [field:SerializeField] public Gradient HealthGradient { get; private set; }

    [field:SerializeField] public CharacterHUDUnit MyPlayerStatus_UI { get; private set; }
    [field:SerializeField] public CharacterHUDUnit OtherPlayer1Status_UI { get; private set; }
    [field:SerializeField] public CharacterHUDUnit OtherPlayer2Status_UI { get; private set; }
    [field:SerializeField] public CharacterHUDUnit OtherPlayer3Status_UI { get; private set; }

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// CharacterHUDUnit�� SetCharacter() �Լ� �ۼ� �ʿ�
    /// </summary>
    void RegisterEventHandler(PlayerController myCharacter, PlayerController otherChar1, PlayerController otherChar2, PlayerController otherChar3) // �ڽ��� ĳ���Ϳ� 
    {
        MyPlayerStatus_UI.SetCharacter(myCharacter);
        OtherPlayer1Status_UI.SetCharacter(otherChar1);
        OtherPlayer2Status_UI.SetCharacter(otherChar2);
        OtherPlayer3Status_UI.SetCharacter(otherChar3);
    }
}
