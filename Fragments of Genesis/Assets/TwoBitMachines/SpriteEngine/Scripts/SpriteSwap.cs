using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [CreateAssetMenu(menuName = "FlareEngine/SpriteSwap")]
        public class SpriteSwap : ScriptableObject
        {
                [SerializeField] public List<CharacterSkin> characterSkin = new List<CharacterSkin>();
                [System.NonSerialized] public List<SpritePacket> sprites = new List<SpritePacket>();

                public void Initialize (List<SpritePacket> sprites)
                {
                        this.sprites = sprites;
                }

                public void Swap (string skinName)
                {
                        if (sprites != null)
                        {
                                Swap(skinName, sprites);
                        }
                }

                public void Swap (string skinName, List<SpritePacket> sprites)
                {
                        for (int i = 0; i < characterSkin.Count; i++)
                        {
                                if (characterSkin[i].name != skinName)
                                {
                                        continue;
                                }

                                List<Skins> skin = characterSkin[i].skin;
                                for (int j = 0; j < skin.Count; j++)
                                {
                                        for (int z = 0; z < sprites.Count; z++)
                                        {
                                                if (sprites[z].name != skin[j].name)
                                                {
                                                        continue;
                                                }
                                                for (int x = 0; x < skin[j].sprite.Count; x++)
                                                {
                                                        if (x < sprites[z].frame.Count)
                                                        {
                                                                sprites[z].frame[x].sprite = skin[j].sprite[x];
                                                        }
                                                }
                                                break;
                                        }
                                }
                                return;
                        }
                }

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool active;
                [SerializeField, HideInInspector] public int signalIndex;
#endif
        }

        [System.Serializable]
        public class CharacterSkin
        {
                [SerializeField] public string name; // category name
                [SerializeField] public List<Skins> skin = new List<Skins>();

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool add;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool delete;
                [SerializeField, HideInInspector] public bool deleteAsk;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool active;
                [SerializeField, HideInInspector] public int signalIndex;
#endif
        }

        [System.Serializable]
        public class Skins
        {
                [SerializeField] public string name; // sprite name
                [SerializeField] public List<Sprite> sprite = new List<Sprite>();

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool add;
                [SerializeField, HideInInspector] public bool edit;
                [SerializeField, HideInInspector] public bool delete;
                [SerializeField, HideInInspector] public bool deleteAsk;
                [SerializeField, HideInInspector] public bool replace;
                [SerializeField, HideInInspector] public bool foldOut;
                [SerializeField, HideInInspector] public bool active;
                [SerializeField, HideInInspector] public int signalIndex;
#endif
        }
}
