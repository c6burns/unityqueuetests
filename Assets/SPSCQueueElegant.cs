using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Explicit, Size = (CACHE_LINE_SIZE * 3))]
public unsafe class SPSCQueueElegant
{
    const int CACHE_LINE_SIZE = 64;
    [FieldOffset(0)] readonly ulong _capacity;
    [FieldOffset(8)] readonly ulong _mask;
    [FieldOffset(16)] readonly IntPtr _alloc;
    [FieldOffset(24)] readonly void* _buffer;
    [FieldOffset(CACHE_LINE_SIZE)] ulong _head;
    [FieldOffset(CACHE_LINE_SIZE + 8)] ulong _tailCache;
    [FieldOffset(CACHE_LINE_SIZE + 16)] readonly ulong _capacityConsumer;
    [FieldOffset(CACHE_LINE_SIZE + 24)] readonly ulong _maskConsumer;
    [FieldOffset(CACHE_LINE_SIZE + CACHE_LINE_SIZE)] ulong _tail;
    [FieldOffset(CACHE_LINE_SIZE + CACHE_LINE_SIZE + 8)] ulong _headCache;
    [FieldOffset(CACHE_LINE_SIZE + CACHE_LINE_SIZE + 16)] readonly ulong _capacityProducer;
    [FieldOffset(CACHE_LINE_SIZE + CACHE_LINE_SIZE + 24)] readonly ulong _maskProducer;

    public SPSCQueueElegant(uint capacity)
    {
        _capacity = NextPowerOfTwo(capacity);
        _capacityProducer = _capacity;
        _capacityConsumer = _capacity;

        _mask = _capacity - 1;
        _maskProducer = _mask;
        _maskConsumer = _mask;

        _head = 0;
        _tailCache = 0;
        _tail = 0;
        _headCache = 0;
        _buffer = Marshal.AllocHGlobal((IntPtr)(_capacity * sizeof(ulong))).ToPointer();
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

    /* producer only */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsFull()
    {
        if ((_headCache - Volatile.Read(ref _tail)) == _capacityProducer) return true;
        return false;
    }

    /* producer only */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetHeadElement(ulong item)
    {
        *((ulong*)_buffer + (_headCache & _maskProducer)) = item;
    }

    /* producer only */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IncrementHead()
    {
        _headCache++;
        Volatile.Write(ref _head, _headCache);
    }

    /* producer only */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEnqueue(ulong item)
    {
        if (IsFull()) return false;
        SetHeadElement(item);
        IncrementHead();
        return true;
    }

    /* consumer only */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsEmpty()
    {
        if (_tailCache == Volatile.Read(ref _head)) return true;
        return false;
    }

    /* consumer only */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong GetTailElement()
    {
        return *((ulong*)_buffer + (_tailCache & _maskConsumer));
    }

    /* consumer only */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void IncrementTail()
    {
        _tailCache++;
        Volatile.Write(ref _tail, _tailCache);
    }

    /* consumer only */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out ulong item)
    {
        item = default;
        if (IsEmpty()) return false;
        item = GetTailElement();
        IncrementTail();
        return true;
    }
}
