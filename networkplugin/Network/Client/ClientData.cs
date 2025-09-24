using System;
using LBoL.Core;

namespace NetworkPlugin.Network.Client;

using System.Collections.Generic;
using LBoL.Core;

public class ClientData
{
    public static Dictionary<string, GameMap> SharedMapData = new Dictionary<string, GameMap>();

    public static string username;
}
