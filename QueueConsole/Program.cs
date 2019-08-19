using System;
using DisruptorUnity3d;

namespace QueueConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Test t = new Test();
            t.StartTestRingBuffer();
            // t.StartTestSPSCQueue();
            t.StartTestSPSCQueueElegant();
            t.StartTestConcurrentQueue();
        }
    }
}
