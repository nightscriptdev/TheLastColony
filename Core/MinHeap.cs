using System;
using System.Collections.Generic;

namespace Core
{
    public class MinHeap<T> where T : IComparable<T>
    {
        private List<T> heap;
        
        public int Count => heap.Count;
        
        public bool IsEmpty => heap.Count == 0;

        public MinHeap()
        {
            heap = new List<T>();
        }
        
        public MinHeap(int capacity)
        {
            heap = new List<T>(capacity);
        }
        
        public void Insert(T item)
        {
            heap.Add(item);
            
            // 向上调整新元素位置，维持堆性质
            HeapifyUp(heap.Count - 1);
        }
        
        public T ExtractMin()
        {
            if (IsEmpty)
                throw new InvalidOperationException("堆为空，无法提取元素");
            
            // 保存要返回的最小元素
            T minItem = heap[0];
            
            // 将最后一个元素移到根位置
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);
            
            // 如果堆不为空，向下调整根元素位置
            if (!IsEmpty)
            {
                HeapifyDown(0);
            }
            
            return minItem;
        }
        
        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("堆为空，无法查看元素");
                
            return heap[0];
        }
        
        public bool Contains(T item)
        {
            return heap.Contains(item);
        }
        
        public void Clear()
        {
            heap.Clear();
        }
        
        public T[] ToArray()
        {
            return heap.ToArray();
        }
        
        private void HeapifyUp(int index)
        {
            // 当不是根节点时继续向上比较
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                
                // 如果当前节点不小于父节点，堆性质已满足
                if (heap[index].CompareTo(heap[parentIndex]) >= 0)
                    break;
                
                // 交换当前节点与父节点
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }
        
        private void HeapifyDown(int index)
        {
            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;
                
                // 找出父节点和子节点中值最小的节点
                if (leftChild < heap.Count && heap[leftChild].CompareTo(heap[smallest]) < 0)
                    smallest = leftChild;
                    
                if (rightChild < heap.Count && heap[rightChild].CompareTo(heap[smallest]) < 0)
                    smallest = rightChild;
                
                // 如果当前节点已经是最小的，堆性质满足
                if (smallest == index)
                    break;
                
                // 交换当前节点与最小子节点
                Swap(index, smallest);
                index = smallest;
            }
        }
        
        private void Swap(int i, int j)
        {
            T temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }
    }
}