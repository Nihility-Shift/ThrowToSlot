using CG.Game.Player;
using CG.Game.SpaceObjects;
using CG.Ship.Hull;
using System.Collections.Generic;

namespace ThrowToSlot
{
    internal class HostSide
    {
        internal static Dictionary<IOrbitObject, Player> thrownItems = new();
        internal static CarryablesSocket[] sockets = new CarryablesSocket[0];
    }
}
