using PixelCrushers.DialogueSystem;
using UnityEngine;

public class Off_Object : MonoBehaviour
{
    string check;
    bool isUmbrella = false;

    private void Update()
    {
        if (check == "success")
        { 
            gameObject.SetActive(false); 
            return;
        }

        Debug.Log("check: " + check);

        check = DialogueLua.GetQuestField("����� ì���ֽǷ���?", "State").asString;
        isUmbrella = DialogueLua.GetVariable("isGeUmbrella").asBool;

        
    }
}
