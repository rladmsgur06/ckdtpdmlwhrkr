using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace TwoBitMachines.FlareEngine.Timeline
{
        [Serializable]
        public class RecordObjectState
        {
                [SerializeField] public List<FieldStateList> record = new List<FieldStateList>();

                public void RecordTracksOrigin (List<Track> track)
                {
                        record.Clear();
                        for (int i = 0; i < track.Count; i++)
                        {
                                for (int j = 0; j < track[i].action.Count; j++)
                                {
                                        FieldChange fieldChange = track[i].action[j].fieldChange;
                                        Component component = fieldChange.component;
                                        if (component == null)
                                        {
                                                continue;
                                        }

                                        Type componentType = fieldChange.component.GetType();
                                        FieldStateList state = new FieldStateList();
                                        FieldInfo[] fields = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                                        foreach (FieldInfo fieldInfo in fields)
                                        {
                                                try
                                                {
                                                        state.component = component;
                                                        state.state.Add(new FieldState(component , "Field" , fieldInfo.Name , fieldInfo.GetValue(component) , true));
                                                }
                                                catch { }
                                        }

                                        PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                                        foreach (PropertyInfo propertyInfo in properties)
                                        {
                                                try
                                                {
                                                        if (propertyInfo.CanRead && propertyInfo.PropertyType != typeof(Material) && propertyInfo.PropertyType != typeof(Material[]))
                                                        {
                                                                state.component = component;
                                                                state.state.Add(new FieldState(component , "Property" , propertyInfo.Name , propertyInfo.GetValue(component) , false));
                                                        }
                                                }
                                                catch { }
                                        }
                                        record.Add(state);
                                }
                        }
                }

                public void RecordObjectOrigin (GameObject gameObject)
                {
                        if (gameObject == null)
                        {
                                return;
                        }

                        record.Clear();
                        Component[] components = gameObject.GetComponents<Component>();

                        foreach (Component component in components)
                        {
                                Type componentType = component.GetType();
                                FieldStateList state = new FieldStateList();
                                // Debug.Log("FOUND TYPE " + componentType);

                                FieldInfo[] fields = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public);
                                foreach (FieldInfo fieldInfo in fields)
                                {
                                        try
                                        {
                                                state.component = component;
                                                state.state.Add(new FieldState(component , "Field" , fieldInfo.Name , fieldInfo.GetValue(component) , true , false));
                                        }
                                        catch { }
                                }

                                PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                                foreach (PropertyInfo propertyInfo in properties)
                                {
                                        try
                                        {
                                                if (propertyInfo.CanRead && propertyInfo.PropertyType != typeof(Material) && propertyInfo.PropertyType != typeof(Material[]))
                                                {
                                                        state.component = component;
                                                        state.state.Add(new FieldState(component , "Property" , propertyInfo.Name , propertyInfo.GetValue(component) , false , false));
                                                }
                                        }
                                        catch { }
                                }
                                record.Add(state);
                        }
                }

                public bool RecordChange (FieldChange fieldChange)
                {
                        for (int i = 0; i < record.Count; i++)
                        {
                                Component component = record[i].component;
                                Type componentType = component.GetType();

                                for (int j = 0; j < record[i].state.Count; j++)
                                {
                                        try
                                        {
                                                FieldState fieldState = record[i].state[j];
                                                object oldValue = fieldState.value;
                                                MemberInfo memberInfo = fieldState.GetMemberInfo(componentType);

                                                if (memberInfo is FieldInfo fieldInfo)
                                                {
                                                        object newValue = fieldInfo.GetValue(component);

                                                        if (!Equals(oldValue , newValue))
                                                        {
                                                                Debug.Log($"Recorded change in {componentType.Name}\nField: {fieldInfo.Name}");
                                                                fieldChange.Set(fieldState.component , fieldState.fieldName , componentType.Name + ": " + fieldInfo.Name , newValue , fieldState.isField);
                                                                return true;
                                                        }
                                                }
                                                else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
                                                {
                                                        object newValue = propertyInfo.GetValue(component);

                                                        if (!Equals(oldValue , newValue))
                                                        {
                                                                Debug.Log($"Recorded change in {componentType.Name}\nProperty: {propertyInfo.Name}");
                                                                fieldChange.Set(fieldState.component , fieldState.fieldName , componentType.Name + ": " + propertyInfo.Name , newValue , fieldState.isField);
                                                                return true;
                                                        }
                                                }
                                        }
                                        catch { }
                                }
                        }
                        return false;
                }

                public void RevertToOrigin ()
                {
                        for (int i = 0; i < record.Count; i++)
                        {
                                Component component = record[i].component;
                                Type componentType = component.GetType();

                                for (int j = 0; j < record[i].state.Count; j++)
                                {
                                        try
                                        {
                                                FieldState fieldState = record[i].state[j];
                                                object oldValue = fieldState.value;
                                                if (oldValue == null)
                                                {
                                                        // Debug.Log("Is empty! skip to next");
                                                        continue;
                                                }
                                                MemberInfo memberInfo = fieldState.GetMemberInfo(componentType);
                                                if (memberInfo is FieldInfo fieldInfo)
                                                {
                                                        object newValue = fieldInfo.GetValue(component);
                                                        if (!Equals(oldValue , newValue))
                                                        {
                                                                fieldInfo.SetValue(component , oldValue);
                                                        }
                                                }
                                                else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
                                                {
                                                        object newValue = propertyInfo.GetValue(component);
                                                        if (!Equals(oldValue , newValue))
                                                        {
                                                                propertyInfo.SetValue(component , oldValue);
                                                        }
                                                }
                                        }
                                        catch { }
                                }
                        }
                        record.Clear();
                }

                public void Serialize ()
                {
                        for (int i = 0; i < record.Count; i++)
                        {
                                for (int j = 0; j < record[i].state.Count; j++)
                                {
                                        record[i].state[j].Serialize();
                                }
                        }
                }

                public void Deserialize ()
                {
                        for (int i = 0; i < record.Count; i++)
                        {
                                for (int j = 0; j < record[i].state.Count; j++)
                                {
                                        record[i].state[j].Deserialize();
                                }
                        }
                }
        }

        [Serializable]
        public class FieldStateList
        {
                [SerializeField] public Component component;
                [SerializeField] public List<FieldState> state = new List<FieldState>();
        }

        [Serializable]
        public class FieldState
        {
                [SerializeField] public Component component;
                [SerializeField] public string memberType;
                [SerializeField] public string fieldName;
                [SerializeField] public string valueString;
                [SerializeField] public bool isField;
                [SerializeField] public bool serialize;
                [SerializeField] public ExtraSerializeTypes extraType = new ExtraSerializeTypes();
                // Need to Serialize manually
                [SerializeField] public object value;

                public FieldState (Component component , string memberType , string fieldName , object value , bool isField , bool serialize = true)
                {
                        this.component = component;
                        this.fieldName = fieldName;
                        this.value = value;
                        this.isField = isField;
                        this.memberType = memberType;
                        this.serialize = serialize;
                        if (serialize)
                        {
                                ChronoTypes.Serialize(extraType , value , ref valueString);
                        }
                }

                public MemberInfo GetMemberInfo (Type targetType)
                {
                        MemberTypes memberInfo = (MemberTypes) Enum.Parse(typeof(MemberTypes) , memberType);

                        if (memberInfo == MemberTypes.Field)
                        {
                                return targetType.GetField(fieldName , BindingFlags.Instance | BindingFlags.Public);
                        }
                        else if (memberInfo == MemberTypes.Property)
                        {
                                return targetType.GetProperty(fieldName , BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                        }
                        return null;
                }

                public void Serialize ()
                {
                        if (serialize)
                                ChronoTypes.Serialize(extraType , value , ref valueString);
                }

                public void Deserialize ()
                {
                        if (serialize)
                                ChronoTypes.Deserialize(extraType , ref value , valueString);
                }
        }
}


//    public Dictionary<Component , Dictionary<ReflectionInfo , FieldState>> components = new Dictionary<Component , Dictionary<ReflectionInfo , FieldState>>();
// public Dictionary<Component , Dictionary<MemberInfo , FieldState>> components = new Dictionary<Component , Dictionary<MemberInfo , FieldState>>();
// public void Revert ()
// {
//         foreach (var entry in components)
//         {
//                 Component component = entry.Key;
//                 Dictionary<MemberInfo , FieldState> state = entry.Value;

//                 foreach (MemberInfo memberInfo in state.Keys)
//                 {
//                         try
//                         {
//                                 object oldValue = state[memberInfo].value;
//                                 if (memberInfo is FieldInfo fieldInfo)
//                                 {
//                                         object newValue = fieldInfo.GetValue(component);

//                                         if (!object.Equals(oldValue , newValue))
//                                         {
//                                                 fieldInfo.SetValue(component , oldValue);
//                                         }
//                                 }
//                                 else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
//                                 {
//                                         object newValue = propertyInfo.GetValue(component);

//                                         if (!object.Equals(oldValue , newValue))
//                                         {
//                                                 propertyInfo.SetValue(component , oldValue);
//                                         }
//                                 }
//                         }
//                         catch
//                         {
//                         }
//                 }
//         }
// }

// public void RecordOrigin (GameObject gameObject)
// {
//         if (gameObject == null)
//         {
//                 return;
//         }

//         this.components.Clear();
//         Component[] components = gameObject.GetComponents<Component>();

//         foreach (Component component in components)
//         {
//                 Type componentType = component.GetType();
//                 Dictionary<MemberInfo , FieldState> state = new Dictionary<MemberInfo , FieldState>();
//                 //Debug.Log("UNITY COMPONENT FOUND:   " + componentType);

//                 FieldInfo[] fields = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public);
//                 foreach (FieldInfo fieldInfo in fields)
//                 {
//                         try
//                         {
//                                 FieldState fieldState = new FieldState(component , fieldInfo.Name , fieldInfo.GetValue(component) , true);
//                                 state[fieldInfo] = fieldState;
//                                 //Debug.Log("Field: " + fieldInfo.Name);
//                         }
//                         catch
//                         {
//                                 // (Exception ex), Debug.LogWarning($"Exception while caching field values for {componentType.Name}: {ex.Message}");
//                         }
//                 }

//                 PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
//                 foreach (PropertyInfo propertyInfo in properties)
//                 {
//                         try
//                         {
//                                 if (propertyInfo.CanRead && propertyInfo.PropertyType != typeof(Material) && propertyInfo.PropertyType != typeof(Material[]))
//                                 {
//                                         FieldState fieldState = new FieldState(component , propertyInfo.Name , propertyInfo.GetValue(component) , false);
//                                         state[propertyInfo] = fieldState;
//                                         //Debug.Log("Property: " + propertyInfo.Name);
//                                 }
//                         }
//                         catch
//                         {
//                                 //(Exception ex)  Debug.LogWarning($"Exception while caching property values for {componentType.Name}: {ex.Message}");
//                         }
//                 }

//                 this.components[component] = state;
//         }
// }

//only change one thing at a time!
// public bool RecordChange (FieldChange fieldChange)
// {
//         foreach (var entry in components)
//         {
//                 Component component = entry.Key;
//                 Dictionary<MemberInfo , FieldState> state = entry.Value;
//                 Type componentType = component.GetType();

//                 foreach (MemberInfo memberInfo in state.Keys)
//                 {
//                         try
//                         {
//                                 FieldState fieldState = state[memberInfo];
//                                 object oldValue = fieldState.value;

//                                 if (memberInfo is FieldInfo fieldInfo)
//                                 {
//                                         object newValue = fieldInfo.GetValue(component);

//                                         if (!object.Equals(oldValue , newValue))
//                                         {
//                                                 Debug.Log($"Recorded change in {componentType.Name} \n                    Field: {fieldInfo.Name}");
//                                                 fieldChange.Set(fieldState.component , fieldState.fieldName , componentType.Name + ": " + fieldInfo.Name , newValue , fieldState.isField);
//                                                 return true;
//                                         }
//                                 }
//                                 else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
//                                 {
//                                         object newValue = propertyInfo.GetValue(component);

//                                         if (!object.Equals(oldValue , newValue))
//                                         {
//                                                 Debug.Log($"Recorded change in {componentType.Name} \n                    Property: {propertyInfo.Name}");
//                                                 fieldChange.Set(fieldState.component , fieldState.fieldName , componentType.Name + ": " + propertyInfo.Name , newValue , fieldState.isField);
//                                                 return true;
//                                         }
//                                 }
//                         }
//                         catch (Exception ex)
//                         {
//                                 Debug.LogWarning($"Exception while tracking member changes for {componentType.Name}: {ex.Message}");
//                         }
//                 }
//         }
//         return false;
// }

// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Reflection;

// namespace TwoBitMachines.FlareEngine.Timeline
// {
//         [Serializable]
//         public class RecordObjectState
//         {
//                 [SerializeField] public RecordDictionary record = new RecordDictionary();

//                 public void RecordOrigin (GameObject gameObject)
//                 {
//                         if (gameObject == null)
//                         {
//                                 return;
//                         }

//                         record.Clear();
//                         Component[] components = gameObject.GetComponents<Component>();

//                         foreach (Component component in components)
//                         {
//                                 Type componentType = component.GetType();
//                                 RecordStateDictionary state = new RecordStateDictionary();

//                                 FieldInfo[] fields = componentType.GetFields(BindingFlags.Instance | BindingFlags.Public);
//                                 foreach (FieldInfo fieldInfo in fields)
//                                 {
//                                         try
//                                         {
//                                                 FieldState fieldState = new FieldState(component , fieldInfo.Name , fieldInfo.GetValue(component) , true);
//                                                 state[new ReflectionInfo("Field" , fieldInfo.Name)] = fieldState;
//                                         }
//                                         catch { }
//                                 }

//                                 PropertyInfo[] properties = componentType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
//                                 foreach (PropertyInfo propertyInfo in properties)
//                                 {
//                                         try
//                                         {
//                                                 if (propertyInfo.CanRead && propertyInfo.PropertyType != typeof(Material) && propertyInfo.PropertyType != typeof(Material[]))
//                                                 {
//                                                         FieldState fieldState = new FieldState(component , propertyInfo.Name , propertyInfo.GetValue(component) , false);
//                                                         state[new ReflectionInfo("Property" , propertyInfo.Name)] = fieldState;
//                                                 }
//                                         }
//                                         catch { }
//                                 }
//                                 record[component] = state;
//                         }
//                 }

//                 public bool RecordChange (FieldChange fieldChange)
//                 {
//                         foreach (var entry in record)
//                         {
//                                 Component component = entry.Key;
//                                 Dictionary<ReflectionInfo , FieldState> state = entry.Value;
//                                 Type componentType = component.GetType();

//                                 foreach (ReflectionInfo reflectionInfo in state.Keys)
//                                 {
//                                         try
//                                         {
//                                                 FieldState fieldState = state[reflectionInfo];
//                                                 object oldValue = fieldState.value;

//                                                 // Convert the serializedMemberInfo to MemberInfo using the targetType
//                                                 MemberInfo memberInfo = reflectionInfo.ToMemberInfo(componentType);

//                                                 if (memberInfo is FieldInfo fieldInfo)
//                                                 {
//                                                         object newValue = fieldInfo.GetValue(component);

//                                                         if (!object.Equals(oldValue , newValue))
//                                                         {
//                                                                 Debug.Log($"Recorded change in {componentType.Name}\nField: {fieldInfo.Name}");
//                                                                 fieldChange.Set(fieldState.component , fieldState.fieldName , componentType.Name + ": " + fieldInfo.Name , newValue , fieldState.isField);
//                                                                 return true;
//                                                         }
//                                                 }
//                                                 else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
//                                                 {
//                                                         object newValue = propertyInfo.GetValue(component);

//                                                         if (!object.Equals(oldValue , newValue))
//                                                         {
//                                                                 Debug.Log($"Recorded change in {componentType.Name}\nProperty: {propertyInfo.Name}");
//                                                                 fieldChange.Set(fieldState.component , fieldState.fieldName , componentType.Name + ": " + propertyInfo.Name , newValue , fieldState.isField);
//                                                                 return true;
//                                                         }
//                                                 }
//                                         }
//                                         catch { }
//                                 }
//                         }
//                         return false;
//                 }

//                 public void Revert ()
//                 {
//                         foreach (var entry in record)
//                         {
//                                 Component component = entry.Key;
//                                 Dictionary<ReflectionInfo , FieldState> state = entry.Value;

//                                 foreach (ReflectionInfo reflectionInfo in state.Keys)
//                                 {
//                                         try
//                                         {
//                                                 object oldValue = state[reflectionInfo].value;

//                                                 if (oldValue == null)
//                                                 {
//                                                         Debug.Log("Is empty! skip to next");
//                                                         continue;
//                                                 }
//                                                 MemberInfo memberInfo = reflectionInfo.ToMemberInfo(component.GetType());

//                                                 if (memberInfo is FieldInfo fieldInfo)
//                                                 {
//                                                         object newValue = fieldInfo.GetValue(component);
//                                                         if (!object.Equals(oldValue , newValue))
//                                                         {
//                                                                 fieldInfo.SetValue(component , oldValue);
//                                                         }
//                                                 }
//                                                 else if (memberInfo is PropertyInfo propertyInfo && propertyInfo.CanRead)
//                                                 {
//                                                         object newValue = propertyInfo.GetValue(component);
//                                                         if (!object.Equals(oldValue , newValue))
//                                                         {
//                                                                 Debug.Log(propertyInfo.Name);
//                                                                 propertyInfo.SetValue(component , oldValue);
//                                                         }
//                                                 }
//                                         }
//                                         catch { }
//                                 }
//                         }
//                 }

//         }

//         [Serializable]
//         public struct FieldState
//         {
//                 public Component component;
//                 public string fieldName;
//                 public object value;
//                 public bool isField;

//                 public FieldState (Component component , string fieldName , object value , bool isField)
//                 {
//                         this.component = component;
//                         this.fieldName = fieldName;
//                         this.value = value;
//                         this.isField = isField;
//                 }
//         }

//         [Serializable]
//         public class ReflectionInfo
//         {
//                 public string MemberType;
//                 public string MemberName;

//                 public ReflectionInfo (string memberType , string memberName)
//                 {
//                         MemberType = memberType;
//                         MemberName = memberName;
//                 }

//                 public MemberInfo ToMemberInfo (Type targetType)
//                 {
//                         MemberTypes memberType = (MemberTypes) Enum.Parse(typeof(MemberTypes) , MemberType);

//                         if (memberType == MemberTypes.Field)
//                         {
//                                 return targetType.GetField(MemberName , BindingFlags.Instance | BindingFlags.Public);
//                         }
//                         else if (memberType == MemberTypes.Property)
//                         {
//                                 return targetType.GetProperty(MemberName , BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
//                         }
//                         return null;
//                 }
//         }

//         [Serializable] public class RecordDictionary : SerializableDictionary<Component , RecordStateDictionary> { }

//         [Serializable] public class RecordStateDictionary : SerializableDictionary<ReflectionInfo , FieldState> { }
// }
