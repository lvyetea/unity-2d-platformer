using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour,Iinteractable
{
    public ScenesLoadEventSO loadEventSO;
    public Vector3 positionToGo;
    public GameSceneSO sceneToGo;
    public void TriggerAction()
    {
        Debug.Log("");

        loadEventSO.RaiseLoadRequestEvent(sceneToGo, positionToGo, true);
    }

    
}
