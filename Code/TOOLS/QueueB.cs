using Godot;
using System;
using System.Collections.Generic;

public partial class QueueB<T> : Queue<T>
{
    public T _Back;

    public new void Enqueue(T item)
    {
        _Back = item;
        base.Enqueue(item);
    }
}