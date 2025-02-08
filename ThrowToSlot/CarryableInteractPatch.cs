using CG.Game;
using CG.Objects;
using CG.Ship.Hull;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidManager.Utilities;

namespace ThrowToSlot
{
    [HarmonyPatch(typeof(CarryableInteract), "EndThrow")]
    internal class CarryableInteractPatch
    {
        //Runs on client only
        static void Prefix(CarryableInteract __instance)
        {
            if (!VoidManagerPlugin.Enabled || __instance?.player?.Payload == null) return;

            //Inform host that item has been thrown
            MessageHandler.Send(__instance.player.Payload.photonView.ViewID);
        }
    }
}
