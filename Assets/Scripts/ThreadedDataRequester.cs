using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
    private const int MaxCallbacksPerFrame = 1;
    
    private static ThreadedDataRequester instance;
    private readonly Queue<ThreadInfo> dataQueue = new();

    private void Awake()
    {
        instance = FindFirstObjectByType<ThreadedDataRequester>();
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        new Thread(ThreadStart).Start();

        return;

        void ThreadStart()
        {
            instance.DataThread(generateData, callback);
        }
    }

    private void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();
        lock (dataQueue)
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    private void Update()
    {
        int processed = 0;
        while (processed < MaxCallbacksPerFrame)
        {
            ThreadInfo threadInfo;

            lock (dataQueue)
            {
                if (dataQueue.Count == 0)
                    return;

                threadInfo = dataQueue.Dequeue();
            }

            threadInfo.callback(threadInfo.parameter);
            processed++;
        }
    }

    private struct ThreadInfo
    {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}