using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Concurrent;
using Random = System.Random;
using DisruptorUnity3d;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class BufferTest
{
    const int QUEUESIZE = 10000;
    const ulong Count = 50000000;

    static readonly Random Rng = new Random();
    static readonly RingBuffer<ulong> _rbQueue = new RingBuffer<ulong>(QUEUESIZE);
    static readonly SPSCQueue _spscQueue = new SPSCQueue(QUEUESIZE);
    static readonly SPSCQueueElegant _spscQueueElegant = new SPSCQueueElegant(QUEUESIZE);
    static readonly ConcurrentQueue<ulong> _conQueue = new ConcurrentQueue<ulong>();

    static readonly Stopwatch sw = new Stopwatch();

    private Thread _consumerThread;
    private Thread _producerThread;

    public BufferTest()
    {
    }

    [Benchmark]
    public void RB()
    {
        _consumerThread = new Thread(() =>
            {
                // Console.WriteLine("RingBuffer consumer started");
                Stopwatch sw = Stopwatch.StartNew();
                for (ulong i = 0; i < Count;)
                {
                    if (_rbQueue.TryDequeue(out ulong val))
                    {
                        if (val != i) Console.WriteLine("wrong value " + val + " ,correct: " + i);
                        i++;
                    }
                }
                // Console.WriteLine("RingBuffer consumer done {0}", sw.Elapsed);
            });

        _producerThread = new Thread(() =>
        {
            // Console.WriteLine("RingBuffer producer started");
            Stopwatch sw = Stopwatch.StartNew();
            for (ulong i = 0; i < Count; i++)
            {
                _rbQueue.Enqueue(i);
            }
            // Console.WriteLine("RingBuffer producer done {0}", sw.Elapsed);
        });

        _consumerThread.Start();
        _producerThread.Start();

#if !UNITY_EDITOR && !UNITY_STANDALONE
        _consumerThread.Join();
        _producerThread.Join();
#endif
    }

    [Benchmark]
    public void SPSCQ()
    {
        SpinWait spinner = new SpinWait();
        _consumerThread = new Thread(() =>
        {
            // Console.WriteLine("SPSCQueueElegant consumer started");
            Stopwatch sw = Stopwatch.StartNew();
            for (ulong i = 0; i < Count;)
            {
                ulong val;
                while (!_spscQueue.TryDequeue(out val)) { spinner.SpinOnce(); }

                if (val != i) Console.WriteLine("wrong value " + val + " ,correct: " + i);
                i++;

            }
            // Console.WriteLine("SPSCQueueElegant consumer done {0}", sw.Elapsed);
        });

        _producerThread = new Thread(() =>
        {
            // Console.WriteLine("SPSCQueueElegant producer started");
            Stopwatch sw = Stopwatch.StartNew();
            for (ulong i = 0; i < Count;)
            {
                while (!_spscQueue.TryEnqueue(i)) { spinner.SpinOnce(); }

                i++;
            }
            // Console.WriteLine("SPSCQueueElegant producer done {0}", sw.Elapsed);
        });

        _consumerThread.Start();
        _producerThread.Start();

#if !UNITY_EDITOR && !UNITY_STANDALONE
        _consumerThread.Join();
        _producerThread.Join();
#endif
    }

    [Benchmark]
    public void CCQ()
    {
        _consumerThread = new Thread(() =>
        {
            // Console.WriteLine("ConcurrentQueue consumer started");
            Stopwatch sw = Stopwatch.StartNew();
            for (ulong i = 0; i < Count;)
            {
                if (_conQueue.TryDequeue(out ulong val))
                {
                    if (val != i) Console.WriteLine("wrong value " + val + " ,correct: " + i);
                    i++;
                }
            }
            // Console.WriteLine("ConcurrentQueue consumer done {0}", sw.Elapsed);
        });

        _producerThread = new Thread(() =>
        {
            // Console.WriteLine("ConcurrentQueue producer started");
            Stopwatch sw = Stopwatch.StartNew();
            for (ulong i = 0; i < Count; i++)
            {
                _conQueue.Enqueue(i);
            }
            // Console.WriteLine("ConcurrentQueue producer done {0}", sw.Elapsed);
        });

        _consumerThread.Start();
        _producerThread.Start();

#if !UNITY_EDITOR && !UNITY_STANDALONE
        _consumerThread.Join();
        _producerThread.Join();
#endif
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<BufferTest>();
        }
    }
}