using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class JournalObject : ScriptableObject
        {
                public virtual string Name ( )
                {
                        return "";
                }

                public virtual string Description ( )
                {
                        return "";
                }

                public virtual string ExtraInfo ( )
                {
                        return "";
                }

                public virtual bool Complete ( )
                {
                        return false;
                }

                public virtual Sprite Icon ( )
                {
                        return null;
                }
        }
}