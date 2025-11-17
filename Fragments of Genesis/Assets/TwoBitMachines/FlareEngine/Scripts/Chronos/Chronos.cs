using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

namespace TwoBitMachines.FlareEngine.Timeline
{
        public class Chronos : MonoBehaviour, ISerializationCallbackReceiver
        {
                [SerializeField] public List<Track> track = new List<Track>();
                [System.NonSerialized] public float time = 0;

                private void OnEnable ()
                {
                        Reset();
                        time = 0;
                }

                // must set to execute first, so that it runs before sprite engine
                private void LateUpdate ()
                {
                        bool running = false;
                        time += Time.deltaTime;
                        for (int i = 0; i < track.Count; i++)
                        {
                                if (track[i].Exeucte())
                                {
                                        running = true;
                                }
                        }

                        if (!running)
                        {
                                // Debug.Log("Time line complete");
                                enabled = false;
                        }

                }

                public void Reset ()
                {
                        for (int i = 0; i < track.Count; i++)
                        {
                                track[i].Reset();
                        }
                }

                public void OnBeforeSerialize ()
                {
                        for (int i = 0; i < track.Count; i++)
                        {
                                track[i].Serialize();
                        }
#if UNITY_EDITOR
                        play.Serialize();
#endif
                }

                public void OnAfterDeserialize ()
                {
                        for (int i = 0; i < track.Count; i++)
                        {
                                track[i].Deserialize();
                        }
#if UNITY_EDITOR
                        play.Deserialize();
#endif
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public RecordObjectState recordState = new RecordObjectState();
                [SerializeField] public ChronosPlay play = new ChronosPlay();
                [SerializeField] public int currentTrackKey;
                [SerializeField] public int currentActionKey;
                [SerializeField] public int dragIndex;
                [SerializeField] public int signalIndex;
                [SerializeField] public float timeZoom;

                [SerializeField] public bool record;
                [SerializeField] public bool dragContent;
                [SerializeField] public bool dragLeft;
                [SerializeField] public bool dragRight;
                [SerializeField] public bool active;
                [SerializeField] public bool trackDrag;

                [SerializeField] public Track trackRef;
                [SerializeField] public Action action;
                [SerializeField] public Material lineMaterial;
                [SerializeField] public Vector2 scrollPosition;
                [SerializeField] public Vector2 maxContentLength;
                [SerializeField] public List<Action> actionArray;
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class Track
        {
                [SerializeField] public TrackType type;
                [SerializeField] public GameObject gameObject;
                [SerializeField] public List<Action> action = new List<Action>();

                [System.NonSerialized] public float time;
                [System.NonSerialized] public bool hasCompleted;

                public void Reset ()
                {
                        time = 0;
                        hasCompleted = false;
                        for (int i = 0; i < action.Count; i++)
                        {
                                action[i].Reset();
                        }
                }

                public bool Exeucte ()
                {
                        bool running = false;
                        time += Time.deltaTime;
                        for (int i = 0; i < action.Count; i++)
                        {
                                if (action[i].Execute(time))
                                {
                                        running = true;
                                }
                        }
                        if (!running)
                        {
                                if (type == TrackType.Loop)
                                {
                                        Reset();
                                }
                                hasCompleted = true;
                        }
                        return running && !hasCompleted;
                }

                public void Serialize ()
                {
                        for (int i = 0; i < action.Count; i++)
                        {
                                action[i].Serialize();
                        }
#if UNITY_EDITOR
                        ChronoTypes.Serialize(extraType, restoreValue, ref restoreString);
#endif
                }

                public void Deserialize ()
                {
                        for (int i = 0; i < action.Count; i++)
                        {
                                action[i].Deserialize();
                        }
#if UNITY_EDITOR
                        ChronoTypes.Deserialize(extraType, ref restoreValue, restoreString);
#endif
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public string name;
                [SerializeField] public int selected;
                [SerializeField] public bool delete;
                [SerializeField] public bool restoreSet;
                [SerializeField] public string restoreString;
                [SerializeField] public object restoreValue;
                [SerializeField] ExtraSerializeTypes extraType = new ExtraSerializeTypes();

                public void SetRestoreState (ExtraSerializeTypes extraType, object newValue)
                {
                        restoreSet = true;
                        restoreValue = newValue;
                        this.extraType.Copy(extraType);
                        ChronoTypes.Serialize(extraType, restoreValue, ref restoreString);
                }

                public void ExeucteScrub (float time)
                {
                        for (int i = 0; i < action.Count; i++)
                        {
                                action[i].Execute(time);

                        }
                }
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class Action
        {
                [SerializeField] public FieldChange fieldChange = new FieldChange();
                [SerializeField] public UnityEvent onActionEnter;
                [SerializeField] public UnityEvent onActionStay;
                [SerializeField] public ActionType type;
                [SerializeField] public Tween tween = Tween.Linear;

                [SerializeField] public float startTime = 0f;
                [SerializeField] public float endTime = 10f;
                [SerializeField] public bool enterFoldOut;
                [SerializeField] public bool stayFoldOut;
                [SerializeField] public bool eventsFoldOut;

                [System.NonSerialized] public bool initStartValue;
                [System.NonSerialized] public bool initSet;
                [System.NonSerialized] public bool eventUsed;
                [System.NonSerialized] public bool actionUsed;

                public bool isSetType => type == ActionType.Set;

                public void Reset ()
                {
                        initStartValue = false;
                        actionUsed = false;
                        eventUsed = false;
                        initSet = false;
                }

                public bool Execute (float time)
                {
                        if (isSetType)
                        {
                                if (time >= startTime && !initSet)
                                {
                                        ApplyState(time);
                                }
                        }
                        else if (time >= startTime && (time <= endTime || !actionUsed))
                        {
                                ApplyState(time);
                        }
                        return time < endTime;
                }

                public void ApplyState (float time)
                {
                        fieldChange.ApplyChange(this, time - startTime, Mathf.Abs(endTime - startTime)); // duration
                        UseEvent();
                }

                public void UseEvent ()
                {
                        if (!eventUsed)
                        {
                                eventUsed = true;
                                onActionEnter.Invoke();
                        }
                        if (onActionStay.GetPersistentEventCount() > 0)
                        {
                                onActionStay.Invoke();
                        }
                }

                public void Serialize ()
                {
                        fieldChange.Serialize();
                }

                public void Deserialize ()
                {
                        fieldChange.Deserialize();
                }
        }

        [System.Serializable]
        public class FieldChange
        {
                [SerializeField] public FieldDataType dataType;
                [SerializeField] public Component component;
                [SerializeField] public string fieldName;
                [SerializeField] public string valueString;
                [SerializeField] public string recorded;
                [SerializeField] public object value; // unity can't serialize system.object :(
                [SerializeField] public ExtraSerializeTypes extraType = new ExtraSerializeTypes();

                [System.NonSerialized] private FieldInfo fieldInfo;
                [System.NonSerialized] private PropertyInfo propertyInfo;
                [System.NonSerialized] private object startValue;

                public void ApplyChange (Action action, float time, float duration)
                {
                        if (component == null)
                        {
                                return;
                        }
                        if (dataType == FieldDataType.Field)
                        {
                                if (fieldInfo == null)
                                {
                                        fieldInfo = component.GetType().GetField(fieldName);
                                }
                                if (fieldInfo != null)
                                {
                                        if (action.type == ActionType.Set)
                                        {
                                                action.initSet = true;
                                                fieldInfo.SetValue(component, value);
                                        }
                                        else
                                        {
                                                if (!action.initStartValue)
                                                {
                                                        action.initStartValue = true;
                                                        startValue = fieldInfo.GetValue(component);
                                                }
                                                System.Object newValue = ChronoInterpolate.Run(startValue, value, time, duration, action.tween);
                                                fieldInfo.SetValue(component, newValue);
                                        }
                                }
                        }
                        else
                        {
                                if (propertyInfo == null)
                                {
                                        propertyInfo = component.GetType().GetProperty(fieldName);
                                }
                                if (propertyInfo != null)
                                {
                                        if (action.type == ActionType.Set)
                                        {
                                                action.initSet = true;
                                                propertyInfo.SetValue(component, value);
                                        }
                                        else
                                        {
                                                if (!action.initStartValue)
                                                {
                                                        action.initStartValue = true;
                                                        startValue = propertyInfo.GetValue(component);
                                                }
                                                System.Object newValue = ChronoInterpolate.Run(startValue, value, time, duration, action.tween);
                                                propertyInfo.SetValue(component, newValue);
                                        }
                                }
                        }
                        action.actionUsed = true;
                }

                public void Set (Component component, string fieldName, string recorded, object newValue, bool isField)
                {
                        this.component = component;
                        this.fieldName = fieldName;
                        this.recorded = recorded;
                        this.value = newValue;
                        dataType = isField ? FieldDataType.Field : FieldDataType.Property;
                        ChronoTypes.Serialize(extraType, newValue, ref valueString);
                }

                public void Serialize ()
                {
                        ChronoTypes.Serialize(extraType, value, ref valueString);
                }

                public void Deserialize ()
                {
                        ChronoTypes.Deserialize(extraType, ref value, valueString);
                }
        }

        [System.Serializable]
        public class ExtraSerializeTypes
        {
                [SerializeField] public Transform transform;
                [SerializeField] public Sprite sprite;

                public void Copy (ExtraSerializeTypes extraType)
                {
                        sprite = extraType.sprite;
                        transform = extraType.transform;
                }
        }

        public enum FieldDataType
        {
                Property,
                Field
        }

        public enum TrackType
        {
                Normal,
                Loop,
                PinPong
        }

        public enum ActionType
        {
                Interpolate,
                Set
        }
}
