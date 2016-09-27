using System.Collections;
using System.Collections.Generic;
using System;


public class Heap<T> where T : IHeapItem
{
    T[] items;
    int maxHeapSize;
    int currentSize;

    public int Count
    {
        get { return currentSize; }
    }
    public Heap(int _maxHeapSize)
    {
        maxHeapSize = _maxHeapSize;
        currentSize = 0;
        items = new T[maxHeapSize];
    }
    public bool Contains(T item)
    {
        if(item.HeapIndex > currentSize - 1)
        {
            return false;
        }
        return items[item.HeapIndex].Equals(item);
    }
    public void Add(T item)
    {
        item.HeapIndex = currentSize;
        items[currentSize++] = item;
        SortUp(item);
    }
    public T RemoveTop()
    {
        T top = items[0];
        items[0] = items[--currentSize];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return top;
    }
    void Swap(T item1, T item2)
    {
        if (item1.HeapIndex == item2.HeapIndex)
        {
            return;
        }
        items[item1.HeapIndex] = item2;
        items[item2.HeapIndex] = item1;
        int hi1 = item1.HeapIndex;
        item1.HeapIndex = item2.HeapIndex;
        item2.HeapIndex = hi1;
    }
    void SortUp(T item)
    {
        int currentIndex = Parent(item.HeapIndex);
        // int p = Parent(currentIndex);
        while (!IsValid(currentIndex))
        {
            int minIndex = currentIndex;
            int left = Left(currentIndex);
            int right = Right(currentIndex);
            if (left < currentSize)
            {
                if (items[left].CompareTo(items[minIndex]) < 0)
                {
                    minIndex = left;
                }
            }
            if (right < currentSize)
            {
                if (items[right].CompareTo(items[minIndex]) < 0)
                {
                    minIndex = right;
                }
            }
            Swap(items[currentIndex], items[minIndex]);
            currentIndex = Parent(currentIndex);
            if (currentIndex < 0)
            {
                break;
            }
        }
    }
    void SortDown(T item)
    {
        int currentIndex = item.HeapIndex;

        while (!IsValid(currentIndex))
        {
            int minIndex = currentIndex;
            int left = Left(currentIndex);
            int right = Right(currentIndex);
            if (left < currentSize)
            {
                if (items[left].CompareTo(items[minIndex]) < 0)
                {
                    minIndex = left;
                }
            }
            if (right < currentSize)
            {
                if (items[right].CompareTo(items[minIndex]) < 0)
                {
                    minIndex = right;
                }
            }
            Swap(items[currentIndex], items[minIndex]);
            currentIndex = minIndex;
            if (currentIndex >= currentSize - 1)
            {
                break;
            }
        }
    }
    int Parent(int i)
    {
        return (i - 1) / 2;
    }
    int Left(int i)
    {
        return 2 * i + 1;
    }
    int Right(int i)
    {
        return 2 * i + 2;
    }
    bool IsValid(int i)
    {
        // Value must be less than its children's value
        int left = Left(i);
        int right = Right(i);
        bool c1 = left < currentSize ? items[i].CompareTo(items[left]) <= 0 : true;
        bool c2 = right < currentSize ? items[i].CompareTo(items[right]) <= 0 : true;
        return c1 && c2;
    }
}

public interface IHeapItem : IComparable
{
    int HeapIndex
    {
        get;
        set;
    }
}