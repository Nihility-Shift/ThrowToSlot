using CG.Objects;
using CG.Ship.Hull;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using VoidManager;

namespace ThrowToSlot
{
    [HarmonyPatch(typeof(CarryableInteract), "EndThrow")]
    internal class CarryableInteractPatch
    {
        private static readonly Dictionary<CarryableObject, (DateTime, CarryablesSocket, float)> thrown = new();
        private static CarryablesSocket[] sockets = new CarryablesSocket[0];
        private static bool checksRunning = false;

        //Runs on client only
        static void Prefix(CarryableInteract __instance)
        {
            if (!VoidManagerPlugin.Enabled || __instance?.player?.Payload == null) return;

            //Track thrown item for 1.5 seconds
            CarryableObject item = __instance.player.Payload;
            if (thrown.ContainsKey(item))
                thrown.Remove(item);
            thrown.Add(item, (DateTime.Now.AddMilliseconds(1500), null, float.PositiveInfinity));

            //Update available sockets
            sockets = UnityEngine.Object.FindObjectsOfType<CarryablesSocket>();

            //Start running checks every frame
            if (!checksRunning)
            {
                checksRunning = true;
                Events.Instance.LateUpdate += CheckItems;
            }
        }

        //Runs every frame if items have been thrown recently
        private static void CheckItems(object o, EventArgs e)
        {
            //Check each thrown item
            for (int i = thrown.Count - 1; i >= 0; i--)
            {
                KeyValuePair<CarryableObject, (DateTime, CarryablesSocket, float)> pair = thrown.ElementAt(i);
                (DateTime endTime, CarryablesSocket closestSocket, float minDistance) = pair.Value;

                //if the item has been picked up
                if (pair.Key.Carrier != null) continue;

                //stop checking the item if it was thrown more than 1.5 seconds ago
                if (endTime < DateTime.Now)
                {
                    thrown.Remove(pair.Key);

                    //if the item came close enough to a socket
                    if (minDistance < 1.5 && closestSocket.isInput && closestSocket.Payload == null)
                    {
                        //place the item in the socket
                        closestSocket.TryInsertCarryable(pair.Key);
                    }
                    continue;
                }

                //Look for closer sockets
                foreach(CarryablesSocket socket in sockets)
                {
                    if (socket.Payload != null || !socket.isInput) continue;
                    float distance = (socket.transform.position - pair.Key.Transform.position).magnitude;
                    if (distance < minDistance && socket.DoesAccept(pair.Key) && socket.Payload == null)
                    {
                        minDistance = distance;
                        closestSocket = socket;
                    }
                }
                if (closestSocket == null) continue;
                
                //if the item hit the socket
                if (minDistance < 0.75f)
                {
                    //place the item in the socket and stop checking
                    closestSocket.TryInsertCarryable(pair.Key);
                    thrown.Remove(pair.Key);
                    continue;
                }

                //update the closest socket this frame
                thrown[pair.Key] = (endTime, closestSocket, minDistance);
            }

            //Stop checking every frame if no items have been thrown recently
            if (thrown.Count == 0)
            {
                Events.Instance.LateUpdate -= CheckItems;
                checksRunning = false;
            }
        }
    }
}
