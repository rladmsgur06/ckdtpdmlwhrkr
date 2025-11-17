using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class AddRTBridge
        {
                public static void Execute (PathfindingRT map)
                {
                        if (map.bridge.Count != 0)
                        {
                                AddBridgeNodes(map);
                        }
                }

                public static void AddBridgeNodes (PathfindingRT map)
                {
                        for (int i = 0; i < map.bridge.Count; i++)
                        {
                                if (map.Node(map.bridge[i].x, map.bridge[i].y, out PathNode node))
                                {
                                        node.bridge = true;
                                }
                                else
                                {
                                        map.map[map.bridge[i]] = new PathNode() { position = map.bridge[i] + map.cellOffset, x = map.bridge[i].x, y = map.bridge[i].y, bridge = true };
                                }
                        }
                }

        }

}
