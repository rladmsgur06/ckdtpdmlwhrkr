using UnityEngine;
using System.Text;

namespace TwoBitMachines.FlareEngine
{
        public class GameTimer : MonoBehaviour
        {
                [SerializeField] private UnityEventString time = new UnityEventString();
                [System.NonSerialized] public bool pause;
                [System.NonSerialized] public float gameTime;
                [System.NonSerialized] private StringBuilder timeStringBuilder = new StringBuilder(); // Create the StringBuilder

                public void LateUpdate()
                {
                        if (pause) return;
                        gameTime += Time.deltaTime;

                        int minutes = Mathf.FloorToInt(gameTime / 60);
                        int seconds = Mathf.FloorToInt(gameTime % 60);
                        timeStringBuilder.Clear();
                        timeStringBuilder.AppendFormat("{0:00}:{1:00}", minutes, seconds);
                        time.Invoke(timeStringBuilder.ToString());
                }

                public void Pause(bool value)
                {
                        pause = value;
                }
                public float GameTime()
                {
                        return gameTime;
                }
        }
}
