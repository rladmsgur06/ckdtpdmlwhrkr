using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace TwoBitMachines.FlareEngine.Timeline
{
        [Serializable]
        public class ChronosPlay
        {
                [SerializeField] public RecordObjectState record = new RecordObjectState();
                [SerializeField] public Chronos chronosRef;
                [SerializeField] public bool isScrubbing;
                [SerializeField] public bool isPlaying;
                [SerializeField] public float scrubTime = 0;
                [SerializeField] public float time = 0;
                [SerializeField] public bool pause;
                [SerializeField] public bool canScrub;

#if UNITY_EDITOR
                #region Auto Play
                public void InitializeAuto (Chronos chronos)
                {
                        if (!isPlaying)
                        {
                                StopScrub();
                                RecordOrigin(chronos);
                                chronosRef = chronos;
                                chronosRef.Reset();
                                isPlaying = true;
                                pause = false;
                                time = 0;
                        }
                }

                public void PlayAuto ()
                {
                        if (!isPlaying || chronosRef == null)
                        {
                                isPlaying = pause = false;
                                return;
                        }

                        if (pause)
                        {
                                return;
                        }

                        bool running = false;
                        time += Time.deltaTime;
                        for (int i = 0; i < chronosRef.track.Count; i++)
                        {
                                if (chronosRef.track[i].Exeucte())
                                {
                                        running = true;
                                }
                        }
                        if (!running)
                        {
                                StopAuto();
                        }
                }

                public void StopAuto ()
                {
                        if (!isPlaying)
                        {
                                isPlaying = pause = false;
                                return;
                        }
                        pause = false;
                        isPlaying = false;
                        RevertToOrigin();
                }

                public void PauseAuto ()
                {
                        if (isPlaying)
                        {
                                pause = !pause;
                        }
                }
                #endregion

                #region Scrub Play
                public void InitializeScrub (Chronos chronos)
                {
                        if (!isScrubbing)
                        {
                                StopAuto();
                                RecordOrigin(chronos);
                                chronosRef = chronos;
                                chronosRef.Reset();
                                isScrubbing = true;
                        }
                }

                public void PlayScrub (float time)
                {
                        if (!isScrubbing || chronosRef == null)
                        {
                                return;
                        }

                        scrubTime = time;
                        for (int i = 0; i < chronosRef.track.Count; i++)
                        {
                                chronosRef.track[i].ExeucteScrub(time);
                        }
                }

                public void StopScrub ()
                {
                        if (!isScrubbing)
                        {
                                return;
                        }
                        isScrubbing = false;
                        RevertToOrigin();
                }
                #endregion

                #region Record State
                private void RecordOrigin (Chronos chronos)
                {
                        RestoreTrackState(chronos);
                        record.RecordTracksOrigin(chronos.track);
                }

                private void RevertToOrigin ()
                {
                        record.RevertToOrigin();
                }

                private void RestoreTrackState (Chronos chronos)
                {
                        for (int i = 0; i < chronos.track.Count; i++)
                        {
                                for (int j = 0; j < chronos.track[i].action.Count; j++)
                                {
                                        Track track = chronos.track[i];
                                        if (track.restoreSet)
                                        {
                                                RestoreState(track.action[j].fieldChange, track);
                                        }
                                }
                        }
                }

                private void RestoreState (FieldChange fieldChange, Track track)
                {
                        if (fieldChange.component == null || track.restoreValue == null || track.restoreString == "" || !track.restoreSet)
                        {
                                return;
                        }
                        if (fieldChange.dataType == FieldDataType.Field)
                        {
                                track.restoreSet = false;
                                FieldInfo fieldInfo = fieldChange.component.GetType().GetField(fieldChange.fieldName);
                                if (fieldInfo != null)
                                {
                                        fieldInfo.SetValue(fieldChange.component, track.restoreValue);
                                }
                        }
                        else
                        {
                                track.restoreSet = false;
                                PropertyInfo propertyInfo = fieldChange.component.GetType().GetProperty(fieldChange.fieldName);
                                if (propertyInfo != null)
                                {
                                        propertyInfo.SetValue(fieldChange.component, track.restoreValue);
                                }
                        }
                }
                #endregion
#endif

                #region Serialize
                public void Serialize ()
                {
                        record.Serialize();
                }

                public void Deserialize ()
                {
                        record.Deserialize();
                }
                #endregion

        }
}
