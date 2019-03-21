using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T> {

    private List<T> List;

    public PriorityQueue()
    {
        List = new List<T>();
    }

    public int Count
    {
        get { return List.Count; }
    }

    public T Peek
    {
        get { return List[List.Count - 1]; }
    }

    public void Enqueue(T item)
    {
        List.Add(item);
        List.Sort();
    }

    public T Dequeue()
    {
        if (List.Count <= 0) throw new System.Exception("PriorityQueue Empty!");
        T ret = List[0];
        List.RemoveAt(0);
        return ret;
    }

    public bool Contains(T item) => List.Contains(item);
}