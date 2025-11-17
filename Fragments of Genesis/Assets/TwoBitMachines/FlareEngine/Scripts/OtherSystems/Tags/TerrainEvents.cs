using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{

        public class TerrainEvents : MonoBehaviour
        {
                [SerializeField] public TerrainCategory terrainJumped = new TerrainCategory() { name = "Jumped" };
                [SerializeField] public TerrainCategory terrainWalls = new TerrainCategory() { name = "Walls" };
                [SerializeField] public TerrainCategory terrainCeiling = new TerrainCategory() { name = "Ceiling" };
                [SerializeField] public TerrainCategory terrainGround = new TerrainCategory() { name = "Ground" };
                [SerializeField] public TagListSO tagListSO;
                // Do others, that don't take any EventEffect
                [System.NonSerialized] private Character character;

                public void Awake ()
                {
                        character = this.gameObject.GetComponent<Character>();
                }

                public void TerrainGround ()
                {
                        TerrainGround(ImpactPacket.impact);
                }

                public void TerrainGround (ImpactPacket packet)
                {
                        if (character == null || !character.world.onGround || character.world.verticalTransform == null)
                        {
                                return;
                        }
                        if (FoundFlareTag(packet, character.world.verticalTransform, terrainGround))
                        {
                                return;
                        }
                }

                public void TerrainJumped ()
                {
                        TerrainJumped(ImpactPacket.impact);
                }

                public void TerrainJumped (ImpactPacket packet)
                {
                        if (character == null || !character.world.onGround || character.world.verticalTransform == null)
                        {
                                return;
                        }
                        if (FoundFlareTag(packet, character.world.verticalTransform, terrainJumped))
                        {
                                return;
                        }
                }

                public void TerrainCeiling ()
                {
                        TerrainCeiling(ImpactPacket.impact);
                }

                public void TerrainCeiling (ImpactPacket packet)
                {
                        if (character == null || !character.world.onCeiling || character.world.verticalTransform == null)
                        {
                                return;
                        }
                        if (FoundFlareTag(packet, character.world.verticalTransform, terrainCeiling))
                        {
                                return;
                        }
                }

                public void TerrainWalls ()
                {
                        TerrainWalls(ImpactPacket.impact);
                }

                public void TerrainWalls (ImpactPacket packet)
                {
                        if (character == null || !character.world.onWall || character.world.wallTransform == null)
                        {
                                return;
                        }
                        if (FoundFlareTag(packet, character.world.wallTransform, terrainWalls))
                        {
                                return;
                        }
                }

                public bool FoundFlareTag (ImpactPacket packet, Transform transform, TerrainCategory terrain)
                {
                        FlareTag tag = transform.GetComponent<FlareTag>();
                        if (tag == null)
                        {
                                return false;
                        }
                        for (int i = 0; i < tag.tags.Count; i++)
                        {
                                if (SearchForEvent(packet, terrain, tag.tags[i]))
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                public bool SearchForEvent (ImpactPacket packet, TerrainCategory terrain, string name)
                {
                        for (int i = 0; i < terrain.events.Count; i++)
                        {
                                if (terrain.events[i].name == name)
                                {
                                        terrain.events[i].onEvent.Invoke(packet);
                                        return true;
                                }
                        }
                        return false;
                }
        }

        [System.Serializable]
        public class TerrainCategory
        {
                [SerializeField] public string name;
                [SerializeField] public List<TerrainEvent> events = new List<TerrainEvent>();
                [SerializeField] public bool foldOut;
        }

        [System.Serializable]
        public class TerrainEvent
        {
                [SerializeField] public string name;
                [SerializeField] public UnityEventEffect onEvent = new UnityEventEffect();
                [SerializeField] public bool foldOut;
        }
}
