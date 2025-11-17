using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace TwoBitMachines.FlareEngine
{
        public class PathfindingRTThread
        {
                // public Queue<PathfindingRequest> pathRequests = new Queue<PathfindingRequest>();
                // public List<PathfindingResult> pathResults = new List<PathfindingResult>();
                // private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                // private Thread pathfindingThread;

                // public void Start ()
                // {
                //         pathfindingThread = new Thread(() => PathfindingThread(cancellationTokenSource.Token));
                //         pathfindingThread.Start();
                // }

                // public void OnDisable ()
                // {
                //         // Cancel the thread using the CancellationToken.
                //         cancellationTokenSource.Cancel();
                // }

                // public void Execute ()
                // {
                //         // Process completed pathfinding results.
                //         lock (pathResults)
                //         {
                //                 while (pathResults.Count > 0)
                //                 {
                //                         PathfindingResult result = pathResults.Dequeue();
                //                         result.Callback(result.Path);
                //                 }
                //         }
                // }

                // public void ClearRequestsAndResults ()
                // {
                //         lock (pathRequests)
                //         {
                //                 pathRequests.Clear();
                //         }

                //         lock (pathResults)
                //         {
                //                 pathResults.Clear();
                //         }
                // }

                // public void RequestPathfinding (Vector3 start, Vector3 end, System.Action<List<Vector3>> callback)
                // {
                //         PathfindingRequest request = new PathfindingRequest(start, end, callback);

                //         // Queue the request for the pathfinding thread.
                //         lock (pathRequests)
                //         {
                //                 pathRequests.Enqueue(request);
                //         }
                // }

                // private void PathfindingThread ()
                // {
                //         while (!cancellationToken.IsCancellationRequested)
                //         {
                //                 PathfindingRequest request = null;

                //                 // Dequeue a pathfinding request.
                //                 lock (pathRequests)
                //                 {
                //                         if (pathRequests.Count > 0)
                //                         {
                //                                 request = pathRequests.Dequeue();
                //                         }
                //                 }

                //                 if (request != null)
                //                 {
                //                         // Simulate pathfinding or use your actual pathfinding algorithm.
                //                         List<Vector3> path = SimulatePathfinding(request.Start, request.End);

                //                         // Queue the pathfinding result.
                //                         lock (pathResults)
                //                         {
                //                                 pathResults.Enqueue(new PathfindingResult(request.Callback, path));
                //                         }
                //                 }
                //         }
                // }

                // private List<Vector3> SimulatePathfinding (Vector3 start, Vector3 end)
                // {
                //         // Replace this with your actual pathfinding algorithm.
                //         // For simplicity, we'll just return a straight path between start and end.
                //         List<Vector3> path = new List<Vector3> { start, end };
                //         return path;
                // }

                // private class PathfindingRequest
                // {
                //         public Vector3 Start;
                //         public Vector3 End;
                //         public System.Action<List<Vector3>> Callback;

                //         public PathfindingRequest (Vector3 start, Vector3 end, System.Action<List<Vector3>> callback)
                //         {
                //                 Start = start;
                //                 End = end;
                //                 Callback = callback;
                //         }
                // }

                // private class PathfindingResult
                // {
                //         public System.Action<List<Vector3>> Callback;
                //         public List<Vector3> Path;

                //         public PathfindingResult (System.Action<List<Vector3>> callback, List<Vector3> path)
                //         {
                //                 Callback = callback;
                //                 Path = path;
                //         }
                // }
        }
}
