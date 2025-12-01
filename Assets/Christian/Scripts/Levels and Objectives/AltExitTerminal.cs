using UnityEngine;

/// Secondary terminal: interacting unlocks an alternate exit.
public class AltExitTerminal : InteractableBase
{
    public override void Interact(Transform actor)
    {
        ObjectiveManager.I?.UnlockAltExit();
    }
}
