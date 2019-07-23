using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace DisruptorUnity3d
{
    public class Test : MonoBehaviour
    {
        const int QUEUESIZE = 10000;
        const ulong Count = 5000000;

        static readonly Random Rng = new Random();
        static readonly RingBuffer<ulong> _rbQueue = new RingBuffer<ulong>(QUEUESIZE);
        static readonly SPSCQueue _spscQueue = new SPSCQueue(QUEUESIZE);
        static readonly SPSCQueueElegant _spscQueueElegant = new SPSCQueueElegant(QUEUESIZE);
        static readonly ConcurrentQueue<ulong> _conQueue = new ConcurrentQueue<ulong>();

        static readonly Stopwatch sw = new Stopwatch();
        
        private Thread _consumerThread;
        private Thread _producerThread;

        private int numberToEnqueue;

        void Start()
        {
        }

        private void OnDestroy()
        {
            try
            {
                _consumerThread.Abort();
            } catch
            {
            }
            try
            {
                _producerThread.Abort();
            } catch
            {
            }
        }

        void OnGUI()
        {
            if (GUILayout.Button("Test RingBuffer"))
            {
                StartTestRingBuffer();
            }

            if (GUILayout.Button("Test SPSCQueue"))
            {
                StartTestSPSCQueue();
            }

            if (GUILayout.Button("Test SPSCQueueElegant"))
            {
                StartTestSPSCQueueElegant();
            }

            if (GUILayout.Button("Test ConcurrentQueue"))
            {
                StartTestConcurrentQueue();
            }
        }

        void StartTestRingBuffer()
        {
            _consumerThread = new Thread(() =>
            {
                Debug.Log("RingBuffer consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    if (_rbQueue.TryDequeue(out ulong val))
                    {
                        if (val != i) Debug.LogError("wrong value " + val + " ,correct: " + i);
                        i++;
                    }
                }
                Debug.LogFormat("RingBuffer consumer done {0}", sw.Elapsed);
            });

            _producerThread = new Thread(() =>
            {
                Debug.Log("RingBuffer producer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count; i++)
                {
                    _rbQueue.Enqueue(i);
                }
                Debug.LogFormat("RingBuffer producer done {0}", sw.Elapsed);
            });

            _consumerThread.Start();
            _producerThread.Start();
        }

        void StartTestSPSCQueue()
        {
            SpinWait spinner = new SpinWait();
            _consumerThread = new Thread(() =>
            {
                Debug.Log("SPSCQueue consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    ulong val;
                    while (!_spscQueue.TryDequeue(out val)) { spinner.SpinOnce(); }

                    if (val != i) Debug.LogError("wrong value " + val + " ,correct: " + i);
                    i++;

                }
                Debug.LogFormat("SPSCQueue consumer done {0}", sw.Elapsed);
            });

            _producerThread = new Thread(() =>
            {
                Debug.Log("SPSCQueue producer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    while (!_spscQueue.TryEnqueue(i)) { spinner.SpinOnce(); }

                    i++;
                }
                Debug.LogFormat("SPSCQueue producer done {0}", sw.Elapsed);
            });

            _consumerThread.Start();
            _producerThread.Start();
        }

        void StartTestSPSCQueueElegant()
        {
            SpinWait spinner = new SpinWait();
            _consumerThread = new Thread(() =>
            {
                Debug.Log("SPSCQueueElegant consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    ulong val;
                    while (!_spscQueue.TryDequeue(out val)) { spinner.SpinOnce(); }

                    if (val != i) Debug.LogError("wrong value " + val + " ,correct: " + i);
                    i++;

                }
                Debug.LogFormat("SPSCQueueElegant consumer done {0}", sw.Elapsed);
            });

            _producerThread = new Thread(() =>
            {
                Debug.Log("SPSCQueueElegant producer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    while (!_spscQueue.TryEnqueue(i)) { spinner.SpinOnce(); }

                    i++;
                }
                Debug.LogFormat("SPSCQueueElegant producer done {0}", sw.Elapsed);
            });

            _consumerThread.Start();
            _producerThread.Start();
        }

        void StartTestConcurrentQueue()
        {
            _consumerThread = new Thread(() =>
            {
                Debug.Log("ConcurrentQueue consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    if (_conQueue.TryDequeue(out ulong val))
                    {
                        if (val != i) Debug.LogError("wrong value " + val + " ,correct: " + i);
                        i++;
                    }
                }
                Debug.LogFormat("ConcurrentQueue consumer done {0}", sw.Elapsed);
            });

            _producerThread = new Thread(() =>
            {
                Debug.Log("ConcurrentQueue producer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count; i++)
                {
                    _conQueue.Enqueue(i);
                }
                Debug.LogFormat("ConcurrentQueue producer done {0}", sw.Elapsed);
            });

            _consumerThread.Start();
            _producerThread.Start();
        }
    }
}
