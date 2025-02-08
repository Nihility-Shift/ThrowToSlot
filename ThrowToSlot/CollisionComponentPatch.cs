using CG.Game.Player;
using CG.Objects;
using CG.Ship.Hull;
using Client.Galaxy.Interactions;
using HarmonyLib;

namespace ThrowToSlot
{
    [HarmonyPatch(typeof(CollisionComponent), "OnTriggerEnter")]
    internal class CollisionComponentPatch
    {
        //Runs on host only
        static void Prefix(CollisionComponent __instance)
        {
            if (!VoidManagerPlugin.Enabled) return;

            //Only run for recently thrown items not near the local player
            if (!HostSide.thrownItems.ContainsKey(__instance.MyObject)) return;
            Player player = HostSide.thrownItems[__instance.MyObject];
            if ((player.Position - __instance.MyObject.Transform.position).magnitude < 3.5f) return;

            CarryableObject item = __instance.MyObject as CarryableObject;
            if (item == null) return;

            //Find closest available socket to the collision point
            float minDistance = float.PositiveInfinity;
            CarryablesSocket closestSocket = null;
            foreach (CarryablesSocket socket in HostSide.sockets)
            {
                float distance = (socket.transform.position - item.Transform.position).magnitude;
                if (distance < minDistance && socket.DoesAccept(item) && socket.Payload == null)
                {
                    minDistance = distance;
                    closestSocket = socket;
                }
            }
            if (closestSocket == null) return;

            //Socket must be within 2.5 of the collision point
            if (minDistance > 2.5f) return;

            //Put the item in the socket and stop detecting collisions for it
            closestSocket.TryInsertCarryable(item);
            HostSide.thrownItems.Remove(item);
        }
    }
}
