
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class zabuton : UdonSharpBehaviour
{
    public override void Interact()
    {
            Networking.LocalPlayer.UseAttachedStation();
    }
}