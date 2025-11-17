using UnityEditor;
using UnityEngine;
using System.Text;
using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Timeline;

namespace TwoBitMachines.FlareEngine.Editors
{
        public class ChronosVariables
        {
                //// no need to serialize
                public float sideBar = 50f;
                public float timeSeconds = 10f;
                public float timeRate = 50f; // 50 pixels = 10 seconds, 5 pixels per second
                public float time => timeRate + timeZoom.floatValue;
                public float timeConvert => timeRate / timeSeconds;
                public float moveOffsetX;
                public bool initOffset;
                public bool actionWasPressed;
                public bool sameAction;
                public bool restoreState;

                ///// Remember
                public SerializedProperty currentTrackKey;
                public SerializedProperty currentActionKey;
                public SerializedProperty timeZoom;
                public SerializedProperty track;
                public SerializedProperty action;
                public SerializedProperty oldAction;
                public SerializedProperty actionArray;
                public SerializedProperty scrollPosition;
                public SerializedProperty maxContentLength;

                public bool actionPressed
                {
                        get
                        {
                                bool pressed = actionWasPressed;
                                actionWasPressed = false;
                                return pressed;
                        }
                        set { actionWasPressed = value; }
                }

                public int actionKey
                {
                        get { return currentActionKey.intValue; }
                        set { currentActionKey.intValue = value; }
                }

                public int trackKey
                {
                        get { return currentTrackKey.intValue; }
                        set { currentTrackKey.intValue = value; }
                }

                public float zoom
                {
                        get { return timeZoom.floatValue; }
                        set { timeZoom.floatValue = value; }
                }

                public float scrollX
                {
                        get { return scrollPosition.vector2Value.x; }
                        set { scrollPosition.vector2Value = new Vector2(value , scrollPosition.vector2Value.y); }
                }

                public float scrollY
                {
                        get { return scrollPosition.vector2Value.y; }
                        set { scrollPosition.vector2Value = new Vector2(scrollPosition.vector2Value.x , value); }
                }

                public float contentLengthX
                {
                        get { return maxContentLength.vector2Value.x; }
                        set { maxContentLength.vector2Value = new Vector2(value , maxContentLength.vector2Value.y); }
                }

                public float contentLengthY
                {
                        get { return maxContentLength.vector2Value.y; }
                        set { maxContentLength.vector2Value = new Vector2(maxContentLength.vector2Value.x , value); }
                }


                public void Initialize (SerializedObject parent)
                {
                        track = parent.Get("trackRef");
                        action = parent.Get("action");
                        timeZoom = parent.Get("timeZoom");
                        actionArray = parent.Get("actionArray");
                        currentTrackKey = parent.Get("currentTrackKey");
                        currentActionKey = parent.Get("currentActionKey");
                        scrollPosition = parent.Get("scrollPosition");
                        maxContentLength = parent.Get("maxContentLength");
                }
        }
}
