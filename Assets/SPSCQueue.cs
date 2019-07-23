using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Explicit, Size = (CACHE_LINE_SIZE * 3))]
public unsafe class SPSCQueue
{
    const int CACHE_LINE_SIZE = 64;
    [FieldOffset(0)] readonly ulong _capacity;
    [FieldOffset(8)] readonly ulong _mask;
    [FieldOffset(16)] readonly IntPtr _alloc;
    [FieldOffset(24)] readonly void *_buffer;
    [FieldOffset(CACHE_LINE_SIZE)] ulong _head;
    [FieldOffset(CACHE_LINE_SIZE + 8)] ulong _tailCache;
    [FieldOffset(CACHE_LINE_SIZE + CACHE_LINE_SIZE)] ulong _tail;
    [FieldOffset(CACHE_LINE_SIZE + CACHE_LINE_SIZE + 8)] ulong _headCache;

    public SPSCQueue(uint capacity)
    {
        _capacity = NextPowerOfTwo(capacity);
        _mask = _capacity - 1;
        _head = 0;
        _tailCache = 0;
        _tail = 0;
        _headCache = 0;
        _alloc = Marshal.AllocHGlobal((IntPtr)((_capacity * sizeof(ulong)) + CACHE_LINE_SIZE));
        _buffer = (void *)(CACHE_LINE_SIZE * (((ulong)_alloc + CACHE_LINE_SIZE - 1) / CACHE_LINE_SIZE));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEnqueue(ulong item)
    {
        ulong head = Volatile.Read(ref _head);
        ulong tail = Volatile.Read(ref _tail);
        if ((head - tail) == _capacity) return false;

        *((ulong*)_buffer + (head & _mask)) = item;
        Volatile.Write(ref _head, head + 1);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out ulong item)
    {
        item = default;

        ulong tail = Volatile.Read(ref _tail);
        ulong head = Volatile.Read(ref _head);
        if (head == tail) return false;

        item = *((ulong*)_buffer + (tail & _mask));
        Volatile.Write(ref _tail, tail + 1);

        return true;
    }

    private ulong NextPowerOfTwo(ulong val)
    {
        ulong rv = 2;
        while (rv < val)
        {
            rv <<= 1;
        }
        return rv;
    }
}
