using CG.Game;
using CG.Game.Player;
using CG.Objects;
using CG.Ship.Hull;
using Photon.Pun;
using VoidManager.ModMessages;
using VoidManager.Utilities;

namespace ThrowToSlot
{
    internal class MessageHandler : ModMessage
    {
        private const int version = 0;

        public override void Handle(object[] arguments, Photon.Realtime.Player sender)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            if (arguments.Length < 1) return;
            if (arguments[0] is not int) return;
            int versionReceived = (int)arguments[0];
            if (version != versionReceived)
            {
                BepinPlugin.Log.LogInfo($"Received version {versionReceived} from {sender.NickName}, expected {version}");
                return;
            }
            if (arguments.Length < 2) return;
            if (arguments[1] is not int) return;
            int viewId = (int)arguments[1];

            //Save item for collision checks for the next few seconds
            CarryableObject thrownItem = PhotonView.Find(viewId).gameObject.GetComponent<CarryableObject>();
            if (HostSide.thrownItems.ContainsKey(thrownItem)) HostSide.thrownItems.Remove(thrownItem);
            HostSide.thrownItems.Add(thrownItem, Player.GetByActorId(sender.ActorNumber));
            Tools.DelayDoUnique(thrownItem, () => HostSide.thrownItems.Remove(thrownItem), 5000);

            //Update list of possible sockets - checked multiple times per thrown object
            HostSide.sockets = ClientGame.Current.playerShip.gameObject.GetComponentsInChildren<CarryablesSocket>();
        }

        internal static void Send(int viewId)
        {
            Send(MyPluginInfo.PLUGIN_GUID, GetIdentifier(typeof(MessageHandler)), Photon.Realtime.ReceiverGroup.MasterClient, new object[] { version, viewId });
        }
    }
}
