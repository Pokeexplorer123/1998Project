using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SNESScript : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator)
    {
        yield return DialogManager.Instance.ShowDialogText("Red was playing the nes");
    }
}