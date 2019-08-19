using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;
using Random = System.Random;
using System;

namespace DisruptorUnity3d
{
#if UNITY_EDITOR || UNITY_STANDALONE
    public class Test : MonoBehaviour
#else
    public class Test
#endif
    {
        const int QUEUESIZE = 100000000;
        const ulong Count = 100000000;

        static readonly Random Rng = new Random();
        static readonly RingBuffer<ulong> _rbQueue = new RingBuffer<ulong>(QUEUESIZE);
        static readonly SPSCQueue _spscQueue = new SPSCQueue(QUEUESIZE);
        static readonly SPSCQueueElegant _spscQueueElegant = new SPSCQueueElegant(QUEUESIZE);
        static readonly ConcurrentQueue<ulong> _conQueue = new ConcurrentQueue<ulong>();

        static readonly Stopwatch sw = new Stopwatch();
        
        private Thread _consumerThread;
        private Thread _producerThread;

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

#if UNITY_EDITOR || UNITY_STANDALONE
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
#endif

        public void StartTestRingBuffer()
        {
            _consumerThread = new Thread(() =>
            {
                Console.WriteLine("RingBuffer consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    if (_rbQueue.TryDequeue(out ulong val))
                    {
                        if (val != i) Console.WriteLine("wrong value " + val + " ,correct: " + i);
                        i++;
                    }
                }
                Console.WriteLine("RingBuffer consumer done {0}", sw.Elapsed);
            });

            _producerThread = new Thread(() =>
            {
                Console.WriteLine("RingBuffer producer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count; i++)
                {
                    _rbQueue.Enqueue(i);
                }
                Console.WriteLine("RingBuffer producer done {0}", sw.Elapsed);
            });

            _consumerThread.Start();
            _producerThread.Start();

#if !UNITY_EDITOR && !UNITY_STANDALONE
            _consumerThread.Join();
            _producerThread.Join();
#endif
        }

        public void StartTestSPSCQueue()
        {
            SpinWait spinner = new SpinWait();
            _consumerThread = new Thread(() =>
            {
                Console.WriteLine("SPSCQueue consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    ulong val;
                    while (!_spscQueue.TryDequeue(out val)) { spinner.SpinOnce(); }

                    if (val != i) Console.WriteLine("wrong value " + val + " ,correct: " + i);
                    i++;

                }
                Console.WriteLine("SPSCQueue consumer done {0}", sw.Elapsed);
            });

            _producerThread = new Thread(() =>
            {
                Console.WriteLine("SPSCQueue producer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    while (!_spscQueue.TryEnqueue(i)) { spinner.SpinOnce(); }

                    i++;
                }
                Console.WriteLine("SPSCQueue producer done {0}", sw.Elapsed);
            });

            _consumerThread.Start();
            _producerThread.Start();

#if !UNITY_EDITOR && !UNITY_STANDALONE
            _consumerThread.Join();
            _producerThread.Join();
#endif
        }

        public void StartTestSPSCQueueElegant()
        {
            SpinWait spinner = new SpinWait();
            _consumerThread = new Thread(() =>
            {
                Console.WriteLine("SPSCQueueElegant consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    ulong val;
                    while (!_spscQueue.TryDequeue(out val)) { spinner.SpinOnce(); }

                    if (val != i) Console.WriteLine("wrong value " + val + " ,correct: " + i);
                    i++;

                }
                Console.WriteLine("SPSCQueueElegant consumer done {0}", sw.Elapsed);
            });

            _producerThread = new Thread(() =>
            {
                Console.WriteLine("SPSCQueueElegant producer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    while (!_spscQueue.TryEnqueue(i)) { spinner.SpinOnce(); }

                    i++;
                }
                Console.WriteLine("SPSCQueueElegant producer done {0}", sw.Elapsed);
            });

            _consumerThread.Start();
            _producerThread.Start();

#if !UNITY_EDITOR && !UNITY_STANDALONE
            _consumerThread.Join();
            _producerThread.Join();
#endif
        }

        public void StartTestConcurrentQueue()
        {
            _consumerThread = new Thread(() =>
            {
                Console.WriteLine("ConcurrentQueue consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    if (_conQueue.TryDequeue(out ulong val))
                    {
                        if (val != i) Console.WriteLine("wrong value " + val + " ,correct: " + i);
                        i++;
                    }
                }
                Console.WriteLine("ConcurrentQueue consumer done {0}", sw.Elapsed);
            });

            _producerThread = new Thread(() =>
            {
                Console.WriteLine("ConcurrentQueue producer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count; i++)
                {
                    _conQueue.Enqueue(i);
                }
                Console.WriteLine("ConcurrentQueue producer done {0}", sw.Elapsed);
            });

            _consumerThread.Start();
            _producerThread.Start();

#if !UNITY_EDITOR && !UNITY_STANDALONE
            _consumerThread.Join();
            _producerThread.Join();
#endif
        }
    }
}
