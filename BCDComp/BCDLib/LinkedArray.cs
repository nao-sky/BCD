using System;
using System.Collections;
using System.Collections.Generic;

namespace LinkedArray
{
    public class LinkedArray<T> : ICollection<T>, IEnumerable<T>, IList<T>
    {
        /// <summary>
        /// FirstNode
        /// </summary>
        private Node FirstNode { get; }
        /// <summary>
        /// LastNode
        /// </summary>
        private Node LastNode { get; }
        /// <summary>
        /// Caching Index Info
        /// </summary>
        private IndexInfo cacheIndexInfo;
        /// <summary>
        /// prev Index
        /// </summary>
        private int prevIndex = -1;
        /// <summary>
        /// cached count
        /// </summary>
        private int count = 0;
        /// <summary>
        /// Tale Size
        /// </summary>
        public int TableCapacity { get; protected set; } = 8192;

        /// <summary>
        /// Default Constractor
        /// </summary>
        public LinkedArray()
        {
            FirstNode = new TerminatedNode(this);
            LastNode = new TerminatedNode(this);
            Node node = new Node(FirstNode, LastNode, this);

            FirstNode.NextNode = node;
            LastNode.PrevNode = node;
        }

        /// <summary>
        /// constractor from List<typeparamref name="T"/>
        /// </summary>
        /// <param name="original">original List<typeparamref name="T"/></param>
        public LinkedArray(List<T> original) : this()
        {
            if (original == null)
                throw new NullReferenceException();

            this.AddRange(original.ToArray());
        }

        public LinkedArray(T[] original) : this()
        {
            if (original == null)
                throw new NullReferenceException();

            this.AddRange(original);
        }

        /// <summary>
        /// sort
        /// </summary>
        public void Sort()
        {
            if (this.Count == 0)
                return;

            List<T> tmp = this.ToList();

            tmp.Sort();

            this.Clear();

            this.AddRange(tmp.ToArray());
        }

        public void Sort(Comparison<T> comparison)
        {
            if (this.Count == 0)
                return;

            List<T> tmp = this.ToList();

            tmp.Sort(comparison);

            this.Clear();

            this.AddRange(tmp.ToArray());
        }

        public void SortedAdd(T item, Comparison<T> comparison)
        {
            if (this.count == 0)
                this.Add(item);

            Node current = FirstNode.NextNode;

            while (current != LastNode)
            {
                if (current.NextNode != LastNode)
                {
                    int ti = comparison(current.NextNode[0], item);

                    if (ti < 0)
                    {
                        current = current.NextNode;
                        continue;
                    }

                }

                int nodeIndex = 0;

                foreach (T t in current)
                {
                    int i = comparison(t, item);

                    if(i > 0)
                    {
                        current.Insert(nodeIndex, item);
                        count++;
                        prevIndex = -1;
                        return;
                    }

                    nodeIndex++;
                }

                current = current.NextNode;
            }
            this.Add(item);
        }

        public List<T> FindAll(Predicate<T> matche)
        {
            List<T> ret = new List<T>();

            foreach(T t in this)
            {
                if (matche(t))
                    ret.Add(t);
            }

            return ret;
        }

        public void ForEach(Action<T> action)
        {
            foreach(T t in this)
            {
                action(t);
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Generic T</returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 ||  index>= Count)
                    throw new IndexOutOfRangeException();

                IndexInfo info = GetIndexInfo(index);
                this.prevIndex = index;
                return info.Current;
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                IndexInfo info = GetIndexInfo(index);
                info.Current = value;
                this.prevIndex = index;
            }
        }

        /// <summary>
        /// Getting Index Info
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>IndexInfo</returns>
        private IndexInfo GetIndexInfo(int index)
        {
            if (index != this.prevIndex)
            {
                // cache compute
                if(this.prevIndex != -1)
                {
                    int sub = index - this.prevIndex;
                    int max = cacheIndexInfo.Node.ElementCount - cacheIndexInfo.NodeIndex;
                    int min = - cacheIndexInfo.NodeIndex;

                    if (min <= sub && sub < max)
                    {
                        cacheIndexInfo.NodeIndex += sub;
                        return cacheIndexInfo;
                    }
                }

                Node current = FirstNode.NextNode;
                int currentIndex = 0;
                int maxCount = 0;

                while (current != LastNode)
                {
                    maxCount += current.ElementCount;

                    if (maxCount > index)
                    {
                        int sub = maxCount - index;
                        currentIndex = current.ElementCount - sub;

                        cacheIndexInfo = new IndexInfo(currentIndex, current);
                        return cacheIndexInfo;
                    }
                    else if (maxCount == index)
                    {
                        current = current.NextNode;
                        cacheIndexInfo = new IndexInfo(0, current);
                        return cacheIndexInfo;
                    }
                    else
                    {
                        current = current.NextNode;
                    }

                }
                throw new ArgumentException();
            }
            else
            {
                return cacheIndexInfo;
            }
        }


        /// <summary>
        /// AddRange
        /// </summary>
        /// <param name="items">items</param>
        public void AddRange(T[] items)
        {
            if (items == null || items.Length <= 0)
                return;

            Node last = this.LastNode.PrevNode;

            last.AddRange(items);
            count += items.Length;
            prevIndex = -1;
        }

        /// <summary>
        /// Count
        /// </summary>
        public int Count
        {
            get
            {
                return (int)count;
            }
        }

        bool ICollection<T>.IsReadOnly { get; } = false;

        /// <summary>
        /// Add Item
        /// </summary>
        /// <param name="item">Item</param>
        public void Add(T item)
        {
            prevIndex = - 1;

            Node current = LastNode.PrevNode;

            current.Add(item);

            this.count++;
        }

        public bool LastOut(out T item)
        {
            if (Count < 1)
            {
                item = default;
                return false;
            }

            prevIndex = -1;

            Node current = LastNode.PrevNode;
            T t = current[current.ElementCount - 1];
            current.RemoveAt(current.ElementCount - 1);

            this.count--;
            item = t;
            return true;
        }
        public bool FirstOut(out T item)
        {
            if (Count < 1)
            {
                item = default;
                return false;
            }

            prevIndex = -1;

            Node current = FirstNode.NextNode;
            while (current.ElementCount == 0)
                current = current.NextNode;

            T t = current[0];
            current.RemoveAt(0);

            this.count--;
            item = t;
            return true;
        }

        /// <summary>
        /// Item Clear
        /// </summary>
        public void Clear()
        {
            this.FirstNode.InsertNextNode(FirstNode, LastNode);

            this.count = 0;
            this.prevIndex = -1;
            this.cacheIndexInfo.Node = null;

        }

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="item">Find Item</param>
        /// <returns>already</returns>
        public bool Contains(T item)
        {
            return this.IndexOf(item) > -1;
        }

        /// <summary>
        /// CopyTo
        /// </summary>
        /// <param name="array">Dest Array</param>
        /// <param name="arrayIndex">Start Index</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            List<T> list = BuildTable();

            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Get Enumerator
        /// </summary>
        /// <returns>IENumerator<typeparamref name="T"/></returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            Node current = FirstNode.NextNode;

            while (current != LastNode)
            {
                foreach (T item in current)
                {
                    yield return item;
                }
                current = current.NextNode;
            }
        }

        public virtual IEnumerator<T> GetReverseEnumerator()
        {
            Node current = LastNode.PrevNode;

            while (current != FirstNode)
            {
                T[] buf = current.GetTableArray();
                Array.Reverse(buf);

                foreach (T item in buf)
                {
                    yield return item;
                }
                current = current.PrevNode;
            }
        }

        /// <summary>
        /// Inner Joining All Tables
        /// </summary>
        /// <returns>List<typeparamref name="T"/></returns>
        private List<T> BuildTable()
        {
            List<T> list = new List<T>();

            Node current = FirstNode.NextNode;

            do
            {
                T[] array = current.GetTableArray();

                list.AddRange(array);
                current = current.NextNode;
            } while (current != LastNode);

            return list;
        }

        /// <summary>
        /// Find Item Index
        /// </summary>
        /// <param name="item">Find Item</param>
        /// <returns>Index</returns>
        public int IndexOf(T item)
        {

            Node current = FirstNode.NextNode;
            int index = 0;
            do
            {
                int cindex = 0;

                for (int i = 0; current.ElementCount > i; i++)
                {
                    if (current[cindex].Equals(item))
                    {
                        return index;
                    }
                    else
                    {
                        cindex++;
                        index++;
                    }
                }

            } while ((current = current.NextNode) != LastNode);

            return -1;
        }

        /// <summary>
        /// Item Insert
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="item">item</param>
        public void Insert(int index, T item)
        {
            if (index >= Count || index < 0)
            {
                this.Add(item);
                return;
            }

            prevIndex = -1;
            IndexInfo indexInfo = this.GetIndexInfo(index);

            indexInfo.Node.Insert(indexInfo.NodeIndex, item);

            this.count++;
        }

        /// <summary>
        /// insert Range Items 
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="items">Items</param>
        public void InsertRange(int index, T[] items)
        {
            if (index >= Count || index < 0)
                throw new IndexOutOfRangeException();

            this.prevIndex = -1;
            IndexInfo indexinfo = GetIndexInfo(index);

            indexinfo.Node.InsertRange(indexinfo.NodeIndex, items);

            this.count += items.Length;

        }

        /// <summary>
        /// Remove Item
        /// </summary>
        /// <param name="item">Finding First Item</param>
        /// <returns>isRemoved</returns>
        public bool Remove(T item)
        {

            Node current = FirstNode.NextNode;

            do
            {
                bool already = current.Remove(item);
                if (already)
                {
                    this.count--;
                    this.prevIndex = -1;
                    return true;
                }

            } while ((current = current.NextNode) != this.LastNode);

            return false;
        }

        /// <summary>
        /// Remove at Index
        /// </summary>
        /// <param name="index">Index</param>
        public void RemoveAt(int index)
        {
            if (index >= Count || index < 0)
                throw new IndexOutOfRangeException();

            this.prevIndex = -1;
            IndexInfo indexInfo = GetIndexInfo(index);

            indexInfo.Node.RemoveAt(indexInfo.NodeIndex);

            this.count--;
        }

        /// <summary>
        /// IEnumerator.GetEnumerator() impliment IList<typeparamref name="T"/>
        /// </summary>
        /// <returns>IEnumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// To Array
        /// </summary>
        /// <returns>All Item Array</returns>
        public T[]  ToArray()
        {
            List<T> list = BuildTable();

            return list.ToArray();
        }

        /// <summary>
        /// to List<typeparamref name="T"/>
        /// </summary>
        /// <returns>List<typeparamref name="T"/></returns>
        public List<T> ToList()
        {
            List<T> list = BuildTable();

            return list;
        }

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        void ICollection<T>.Clear()
        {
            this.Clear();
        }

        bool ICollection<T>.Contains(T item)
        {
            return this.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            return this.Remove(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        /// <summary>
        /// Inner Node
        /// </summary>
        private class Node  : IEnumerable<T>
        {
            /// <summary>
            /// Prev Node
            /// </summary>
            public virtual Node PrevNode { get; set; }
            /// <summary>
            /// Next Node
            /// </summary>
            public virtual Node NextNode { get; set; }
            /// <summary>
            /// Outer Instance
            /// </summary>
            private LinkedArray<T> outerInstance;
            /// <summary>
            /// ElementCount at Node
            /// </summary>
            public int ElementCount = 0;
            /// <summary>
            /// inner Table field
            /// </summary>
            private T[] table = null;

            /// <summary>
            /// Constractor
            /// </summary>
            /// <param name="prev">prev Node</param>
            /// <param name="next">next Node</param>
            /// <param name="outerInstance">outer Instance</param>
            public Node(Node prev, Node next, LinkedArray<T> outerInstance)
            {
                this.PrevNode = prev;
                this.NextNode = next;
                this.outerInstance = outerInstance;
            }

            /// <summary>
            /// Inssert Next Node
            /// </summary>
            /// <param name="prev">prev Node</param>
            /// <param name="next">next Node</param>
            public void InsertNextNode(Node prev, Node next)
            {
                Node newNext = new Node(prev, next, this.outerInstance);

                prev.NextNode = newNext;
                next.PrevNode = newNext;
            }
            
            /// <summary>
            /// Add Item
            /// </summary>
            /// <param name="item">Item</param>
            public virtual void Add(T item)
            {
                if(ElementCount < Table.Length)
                {
                    Table[ElementCount++] = item;
                }
                else
                {
                    InsertNextNode(this, NextNode);
                    this.NextNode.Add(item);
                   
                }
            }

            /// <summary>
            /// AddRange
            /// </summary>
            /// <param name="items">items</param>
            public virtual void AddRange(T[] items)
            {
                InnerTable[] tables =this.SplitLargeTable(items);
                Node current = this;

                foreach(InnerTable table in tables)
                {
                    InsertNextNode(current, current.NextNode);

                    current.NextNode.Table = table.Data;
                    current.NextNode.ElementCount = table.ElementCount;

                    current = current.NextNode;
                }
            }

            /// <summary>
            /// Insert Item at Index
            /// </summary>
            /// <param name="index">Index</param>
            /// <param name="item">Item</param>
            public virtual void Insert(int index, T item)
            {
                if (ElementCount == outerInstance.TableCapacity)
                {
                    HalfSplitNode();

                    int splitLength = outerInstance.TableCapacity / 2;
                    int splitIndex = index - splitLength;

                    if(splitLength <= index)
                    {
                        this.NextNode.Insert(splitIndex, item);
                    }
                    else
                    {
                        this.Insert(index, item);
                    }
                }
                else
                {
                    int length = ElementCount - index;
                    InsertTableItem(Table, index, length, item);
                    ElementCount++;
                }

            }

            /// <summary>
            /// Insert Range Items
            /// </summary>
            /// <param name="index">index</param>
            /// <param name="items">Items</param>
            public virtual void InsertRange(int index, T[] items)
            {
                int currentCount = index + items.Length;
                InnerTable[] tables = this.SplitLargeTable(items);
                
                if (tables.Length > 1)
                {

                    if (index > 0)
                    {
                        SplitNode(index);
                        Node current = this;

                        foreach (InnerTable table in tables)
                        {
                            Node prev = current;
                            Node next = current.NextNode;

                            InsertNextNode(prev, next);

                            current.NextNode.Table = table.Data;
                            current.NextNode.ElementCount = table.ElementCount;

                            current = current.NextNode;
                        }
                    }
                    else
                    {

                        var reversedTables = tables.AsSpan<InnerTable>();
                        reversedTables.Reverse();

                        Node prev = this.PrevNode;

                        foreach (InnerTable innerTable in reversedTables)
                        {
                            InsertNextNode(prev, prev.NextNode);

                            Node current = prev.NextNode;


                            current.Table = innerTable.Data;
                            current.ElementCount = innerTable.ElementCount;
                        }

                    }
                }
                else if (tables.Length == 1)
                {
                    if (index > 0)
                    {
                        SplitNode(index);
                        Node current = this;

                        Node next = this.NextNode;

                        InsertNextNode(this, next);

                        this.NextNode.Table = tables[0].Data;
                        this.NextNode.ElementCount = tables[0].ElementCount;
                    }
                    else
                    {
                        Node prev = this.PrevNode;

                        InsertNextNode(prev, this);

                        Array.Copy(tables[0].Data, prev.NextNode.Table, tables[0].ElementCount);
                        prev.NextNode.ElementCount = tables[0].ElementCount;
                    }
                }
                else
                    throw new ArgumentOutOfRangeException();
            }

            /// <summary>
            /// split table
            /// </summary>
            /// <param name="items">items</param>
            /// <returns>tables</returns>
            private InnerTable[] SplitLargeTable(T[] items)
            {
                int tableCount = items.Length / outerInstance.TableCapacity;
                int quotient = items.Length % outerInstance.TableCapacity;
                List<InnerTable> tables = new List<InnerTable>();

                for (int i = 0; tableCount > i; i++)
                {
                    var span = items.AsSpan<T>(i * outerInstance.TableCapacity, outerInstance.TableCapacity);

                    tables.Add(new InnerTable(span.ToArray(), outerInstance.TableCapacity));
                }

                int blocks = tableCount * outerInstance.TableCapacity;

                if (quotient > 0)
                {
                    var span = items.AsSpan<T>(blocks);

                    T[] ts = new T[outerInstance.TableCapacity];
                    Array.Copy(span.ToArray(), ts, span.Length);
                    tables.Add(new InnerTable(ts, quotient));
                }

                return tables.ToArray();
            }

            /// <summary>
            /// Splitting Node
            /// </summary>
            private void HalfSplitNode()
            {
                Node newNode = new Node(this, this.NextNode, this.outerInstance);
                this.NextNode.PrevNode = newNode;
                this.NextNode = newNode;

                int length = outerInstance.TableCapacity / 2;

                int index = outerInstance.TableCapacity - length;

                this.Table.AsSpan<T>(index, length).ToArray().CopyTo(newNode.Table , 0);

                this.Table.AsSpan<T>(index, length).Fill(default);

                this.ElementCount -= length;

                newNode.ElementCount = length;

            }

            /// <summary>
            /// split node
            /// </summary>
            /// <param name="threshold">threshold</param>
            private void SplitNode(int threshold)
            {
                this.InsertNextNode(this, this.NextNode);

                int length = this.ElementCount - threshold;

                int index = threshold;

                var span = this.Table.AsSpan<T>(index, length);

                int sub = span.Length;

                T[] ts = new T[outerInstance.TableCapacity];
                Array.Copy(span.ToArray(), ts, sub);
                this.NextNode.Table = ts;
                
                this.Table.AsSpan<T>(index).Fill(default);

                this.ElementCount = threshold;

                this.NextNode.ElementCount = sub;
            }

            /// <summary>
            /// Inner Item Insert To Table
            /// </summary>
            /// <param name="array">source array</param>
            /// <param name="index">Index</param>
            /// <param name="length">Length</param>
            /// <param name="item">Item</param>
            private void InsertTableItem(T[] array, int index, int length, T item)
            {
                if (length == 0)
                {
                    array[0] = item;
                }
                else
                {
                    Array.Copy(array, index, array, index + 1, length);

                    array[index] = item;
                }
            }

            /// <summary>
            /// remove First Item
            /// </summary>
            /// <param name="item">Generic item</param>
            /// <returns>is Removed</returns>
            public bool Remove(T item)
            {
                int thisIndex = IndexOf(item);
                
                if(thisIndex >=0)
                {
                    this.RemoveAt(thisIndex);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Find First Item
            /// </summary>
            /// <param name="item">Item</param>
            /// <returns>Index</returns>
            public int IndexOf(T item)
            {
                T[] ts = Table;
                return Array.FindIndex<T>(ts, 0, this.ElementCount, m =>
                {
                    if (m.GetHashCode() != item.GetHashCode())
                        return false;

                    return m.Equals(item);
                });


                //for (int i = 0; ElementCount > i; i++)
                //{
                //    T m = ts[i];
                //    if (m.GetHashCode() == item.GetHashCode())

                //        if (m.Equals(item))
                //            return i;

                //}
                //return -1;
            }

            /// <summary>
            /// Remove At Index
            /// </summary>
            /// <param name="index">Index</param>
            public void RemoveAt(int index)
            {
                if (this.ElementCount == 1)
                {
                    if (this.PrevNode == outerInstance.FirstNode && this.NextNode == this.outerInstance.LastNode)
                    {
                        this.ElementClear();
                        return;
                    }
                    else
                    {
                        RemoveThisNode();
                        return;
                    }
                }

                if (index != ElementCount - 1)
                {
                    int srcIndex = index + 1;
                    int destIndex = index;
                    int length = ElementCount - index;

                    Array.Copy(Table, srcIndex, Table, destIndex, length - 1);
                    Table[destIndex + length - 1] = default;
                    ElementCount--;
                }
                else
                {
                    Table[index] = default;
                    ElementCount--;
                }
            }

            /// <summary>
            /// Remove This Node
            /// </summary>
            private void RemoveThisNode()
            {
                Node prev = this.PrevNode;
                Node next = this.NextNode;

                prev.NextNode = next;
                next.PrevNode = prev;
            }

            /// <summary>
            /// Clear This Elements
            /// </summary>
            private void ElementClear()
            {
                Array.Fill<T>(Table, default);
                ElementCount = 0;
            }

            /// <summary>
            /// Table Property
            /// </summary>
            protected virtual T[] Table
            {
                get
                {
                    table = table ?? new T[outerInstance.TableCapacity];
                    return table;
                }
                set
                {
                    table = value;
                }
            }

            /// <summary>
            /// Indexer
            /// </summary>
            /// <param name="index">Index</param>
            /// <returns>Item</returns>
            public T this[int index]
            {
                get
                {
                    return Table[index];
                }
                set
                {
                    Table[index] = value;
                }
            }

            /// <summary>
            /// Get Table Array
            /// </summary>
            /// <returns>Generic T array</returns>
            public T[] GetTableArray()
            {
                T[] array = Table.AsSpan<T>(0, ElementCount).ToArray();
                return array;
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                for(int i =0; ElementCount>i;i++)
                {
                    yield return table[i];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<T>)this).GetEnumerator();
            }
        }

        /// <summary>
        /// First and Last Node Class
        /// </summary>
        private class TerminatedNode : Node
        {
            /// <summary>
            /// Constractor
            /// </summary>
            /// <param name="outerInstance">outer Instance</param>
            public TerminatedNode(LinkedArray<T> outerInstance) : base(null, null, outerInstance) { }

            protected override T[] Table { get => throw new Exception(); set => throw new Exception(); }

        }

        /// <summary>
        /// IndexInfo
        /// </summary>
        private struct IndexInfo
        {
            /// <summary>
            /// Node Index
            /// </summary>
            public int NodeIndex { get; set; }
            /// <summary>
            /// Node
            /// </summary>
            public Node Node { get; set; }

            /// <summary>
            /// Constractor
            /// </summary>
            /// <param name="nodeIndex">node Index</param>
            /// <param name="node">node</param>
            /// <param name="outerIndex">outer Index</param>
            public IndexInfo(int nodeIndex, Node node)
            {
                this.NodeIndex = nodeIndex;
                this.Node = node;
            }

            public T Current
            {
                get
                {
                    return Node[NodeIndex];
                }
                set
                {
                    Node[NodeIndex] = value;
                }
            }
        }

        /// <summary>
        /// inner table
        /// </summary>
        private class InnerTable
        {
            /// <summary>
            /// raw data
            /// </summary>
            public T[] Data { get; set; }
            /// <summary>
            /// element count
            /// </summary>
            public int ElementCount { get; set; }

            /// <summary>
            /// constractor
            /// </summary>
            /// <param name="data">innerdata</param>
            /// <param name="count">elementCount</param>
            public InnerTable(T[] data, int count)
            {
                this.Data = data;
                this.ElementCount = count;
            }
        }
    }
}
