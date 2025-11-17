using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

namespace TwoBitMachines.MapSystem
{
        [System.Serializable]
        public class Room
        {
                [SerializeField] public Vector2 playerIcon;
                [SerializeField] public Vector2 offset;
                [SerializeField] public bool visible = false;
                [SerializeField] public float thickness = 0.1f;
                [SerializeField] public bool useRoundEnds;
                [SerializeField] public bool opacity;

                [SerializeField] public MeshData roomMesh = new MeshData();
                [SerializeField] public MeshData outlineMesh = new MeshData();
                [SerializeField] public List<Point> lineList = new List<Point>();
                [SerializeField] public List<Point> pointList = new List<Point>();

#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public int lineIndex;
                [SerializeField] public int pointIndex;
                [SerializeField] public bool foldOut;
                [SerializeField] public bool deleteAsk;
                [SerializeField] public bool delete;
                [SerializeField] public bool arrowFoldOut;
#pragma warning restore 0414
#endif
        }
}
