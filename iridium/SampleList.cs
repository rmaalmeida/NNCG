//-----------------------------------------------------------------------
// <copyright file="SampleList.cs" company="Math.NET Project">
//    Copyright (c) 2002-2009, Christoph R�egg.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph R�egg, http://christoph.ruegg.name
// </author>
// <product>
//    Math.NET Iridium, part of the Math.NET Project.
//    http://mathnet.opensourcedotnet.info
// </product>
// <license type="opensource" name="LGPL" version="2 or later">
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published 
//    by the Free Software Foundation; either version 2 of the License, or
//    any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public 
//    License along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;

namespace MathNet.Numerics
{
    /// <summary>
    /// SampleList is a sorted list in ascending order for discrete function samples x=f(t) or value pairs (t,x).
    /// Multiple values x for the same t are replaced with the mean.
    /// </summary>
    /// <remarks>
    /// This list is designed for rare additions and removals only and is slow when adding
    /// items in not-ascending order. Better fill it directly in the constructor.
    /// </remarks>
    [Serializable]
    public class SampleList :
        ICloneable,
        IDictionary
    {
        int _size;
        int[] _sampleCount; // for mean calculation
        double[] _sampleT;
        double[] _sampleX;

        KeyList _keyList;
        ValueList _valueList;

        /// <summary>
        /// Event which notifies when a sample has been altered.
        /// </summary>
        public event EventHandler<SampleAlteredEventArgs> SampleAltered;

        #region Construction

        /// <summary>
        /// Initializes a new instance of the SampleList class.
        /// </summary>
        public
        SampleList()
        {
            _sampleCount = new int[16];
            _sampleT = new double[16];
            _sampleX = new double[16];
        }

        /// <summary>
        /// Initializes a new instance of the SampleList class.
        /// </summary>
        /// <param name="capacity">initial capacity</param>
        public
        SampleList(int capacity)
        {
            if(capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", Properties.LocalStrings.ArgumentNotNegative);
            }

            _sampleCount = new int[capacity];
            _sampleT = new double[capacity];
            _sampleX = new double[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the SampleList class.
        /// </summary>
        public
        SampleList(IDictionary d)
            : this((d != null) ? d.Count : 16)
        {
            if(null == d)
            {
                throw new ArgumentNullException("d");
            }

            d.Keys.CopyTo(_sampleT, 0);
            d.Values.CopyTo(_sampleX, 0);
            Array.Sort(_sampleT, _sampleX);
            _size = d.Count;
        }

        /// <summary>
        /// Initializes a new instance of the SampleList class.
        /// </summary>
        public
        SampleList(IDictionary d, int capacity)
            : this((d != null) ? (capacity >= d.Count ? capacity : d.Count) : 16)
        {
            if(null == d)
            {
                throw new ArgumentNullException("d");
            }

            d.Keys.CopyTo(_sampleT, 0);
            d.Values.CopyTo(_sampleX, 0);
            Array.Sort(_sampleT, _sampleX);
            _size = d.Count;
        }

        /// <summary>
        /// Initializes a new instance of the SampleList class,
        /// based on a copy of two arrays.
        /// </summary>
        /// <param name="t">keys t, where x=f(t) or (t,x).</param>
        /// <param name="x">values x, where x=f(t) or (t,x).</param>
        /// <exception cref="ArgumentOutOfRangeException">If the dimensionality of the two arrays doesn't match.</exception>
        public
        SampleList(
            IList<double> t,
            IList<double> x)
            : this((t != null) ? t.Count : 0)
        {
            if(null == t)
            {
                throw new ArgumentNullException("t");
            }

            if(null == x)
            {
                throw new ArgumentNullException("x");
            }

            if(t.Count != x.Count)
            {
                throw new ArgumentOutOfRangeException("x");
            }

            t.CopyTo(_sampleT, 0);
            x.CopyTo(_sampleX, 0);
            Array.Sort(_sampleT, _sampleX);
            _size = t.Count;
        }

        #endregion

        #region Items

        /// <summary>
        /// Doubles the capacity. Do not use this method directly, use <see cref="Add"/> instead.
        /// </summary>
        void
        EnsureCapacity(int min)
        {
            int capacitySuggestion = (_sampleCount.Length < 8) ? 16 : (_sampleCount.Length << 1);
            Capacity = (capacitySuggestion < min) ? min : capacitySuggestion;
        }

        /// <summary>
        /// Append a sample to existing samples. Do not use this method directly, use <see cref="Add"/> instead.
        /// </summary>
        void
        AppendMean(int index, double x)
        {
            double cntBefore = _sampleCount[index];
            double cntAfter = _sampleCount[index] = _sampleCount[index] + 1;
            _sampleX[index] = ((_sampleX[index] * cntBefore) + x) / cntAfter;

            if(SampleAltered != null)
            {
                SampleAltered(this, new SampleAlteredEventArgs(_sampleT[index]));
            }
        }

        /// <summary>
        /// Insert a new sample. Do not use this method directly, use <see cref="Add"/> instead.
        /// </summary>
        void
        Insert(
            int index,
            double t,
            double x)
        {
            if(_size == _sampleCount.Length)
            {
                EnsureCapacity(_size + 1);
            }

            if(index < _size)
            {
                Array.Copy(_sampleCount, index, _sampleCount, index + 1, _size - index);
                Array.Copy(_sampleT, index, _sampleT, index + 1, _size - index);
                Array.Copy(_sampleX, index, _sampleX, index + 1, _size - index);
            }

            _sampleCount[index] = 1;
            _sampleT[index] = t;
            _sampleX[index] = x;
            _size++;

            if(SampleAltered != null)
            {
                SampleAltered(this, new SampleAlteredEventArgs(t));
            }
        }

        /// <summary>
        /// Add a new sample to the list. 
        /// </summary>
        /// <param name="t">key t, where x=f(t) or (t,x).</param>
        /// <param name="x">value x, where x=f(t) or (t,x).</param>
        public virtual
        void
        Add(double t, double x)
        {
            int index = Locate(t);
            if(index > 0 && Number.AlmostEqual(_sampleT[index], t))
            {
                AppendMean(index, x);
            }
            else if(index < _sampleT.Length - 1 && Number.AlmostEqual(_sampleT[index + 1], t))
            {
                AppendMean(index + 1, x);
            }
            else
            {
                Insert(index + 1, t, x);
            }
        }

        /// <summary>
        /// Remove the sample at the given index.
        /// </summary>
        public virtual
        void
        RemoveAt(int index)
        {
            double t = _sampleT[index];

            if(index < 0 || index >= _size)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            _size--;
            if(index < _size)
            {
                Array.Copy(_sampleCount, index + 1, _sampleCount, index, _size - index);
                Array.Copy(_sampleT, index + 1, _sampleT, index, _size - index);
                Array.Copy(_sampleX, index + 1, _sampleX, index, _size - index);
            }

            _sampleCount[_size] = 0;

            if(SampleAltered != null)
            {
                SampleAltered(this, new SampleAlteredEventArgs(t));
            }
        }

        /// <summary>
        /// Remove all samples with a key exactly equal to t, where x=f(t) or (t,x).
        /// </summary>
        public virtual
        void
        Remove(double t)
        {
            int index = IndexOfT(t);
            if(index >= 0)
            {
                RemoveAt(index);
            }
        }

        /// <summary>
        /// Remove all samples from the list.
        /// </summary>
        public virtual
        void
        Clear()
        {
            _size = 0;

            if(SampleAltered != null)
            {
                SampleAltered(this, new SampleAlteredEventArgs(double.NaN));
            }
        }

        /// <summary>
        /// The index of the sample with a key exactly equal to t.
        /// </summary>
        int
        IndexOfT(double t)
        {
            int index = Locate(t);
            if(Number.AlmostEqual(_sampleT[index], t))
            {
                return index;
            }

            if(Number.AlmostEqual(_sampleT[index + 1], t))
            {
                return index + 1;
            }

            return -1;
        }

        /// <summary>
        /// The index of the first sample with a value exactly equal to x.
        /// </summary>
        int
        IndexOfX(double x)
        {
            return Array.IndexOf(_sampleX, x, 0, _size);
        }

        /// <summary>
        /// Sets the X matching the given T (and sets its count to 1) or inserts a new sample.
        /// Do not use this to add a sample to the list!
        /// </summary>
        void
        SetX(double t, double x)
        {
            int index = Locate(t);
            if(Number.AlmostEqual(_sampleT[index], t))
            {
                _sampleX[index] = x;
                _sampleCount[index] = 1;
            }
            else if(Number.AlmostEqual(_sampleT[index + 1], t))
            {
                _sampleX[index + 1] = x;
                _sampleCount[index + 1] = 1;
            }
            else
            {
                Insert(index + 1, t, x);
            }
        }

        /// <summary>
        /// The smallest key t in the sample list, where x=f(t) or (t,x).
        /// </summary>
        public double MinT
        {
            get { return _sampleT[0]; }
        }

        /// <summary>
        /// The biggest key t in the sample list, where x=f(t) or (t,x).
        /// </summary>
        public double MaxT
        {
            get { return _sampleT[_size - 1]; }
        }

        /// <summary>
        /// Get the key t stored at the given index, where x=f(t) or (t,x).
        /// </summary>
        public
        double
        GetT(int index)
        {
            return _sampleT[index];
        }

        /// <summary>
        /// Get the value x stored at the given index, where x=f(t) or (t,x).
        /// </summary>
        public
        double
        GetX(int index)
        {
            return _sampleX[index];
        }

        #endregion

        #region Search

        /// <summary>
        /// Find the index i of a sample near t such that t[i] &lt;= t &lt;= t[i+1]. 
        /// </summary>
        /// <returns>The lower bound index of the interval, -1 or Size-1 if out of bounds.</returns>
        public
        int
        Locate(double t)
        {
            return LocateBisection(t, -1, _size);
        }

        /// <summary>
        /// Find the index i of a sample near t such that t[i] &lt;= t &lt;= t[i+1]. 
        /// </summary>
        /// <remarks>This method is faster if the expected index is near 
        /// <paramref name="nearIndex"/> (compared to the list size) but may be slower
        /// otherwise (worst case: factor 2 slower, best case: log2(n) faster).</remarks>
        /// <returns>The lower bound index of the interval, -1 or Size-1 if out of bounds.
        /// </returns>
        public
        int
        Locate(double t, int nearIndex)
        {
            if(nearIndex < 0 || nearIndex > _size - 1)
            {
                return LocateBisection(t, -1, _size);
            }

            int lower = nearIndex;
            int upper;
            int increment = 1;

            if(t >= _sampleT[lower]) 
            {
                /* Hunt Up */

                if(lower == _size - 1)
                {
                    return lower;
                }

                upper = lower + 1;
                while(t >= _sampleT[upper])
                {
                    lower = upper;
                    increment <<= 1;
                    upper = lower + increment;

                    if(upper > _size - 1)
                    {
                        upper = _size;
                        break;
                    }
                }
            }
            else 
            {
                /* Hunt Down */

                if(lower == 0)
                {
                    return -1;
                }

                upper = lower--;
                while(t < _sampleT[lower])
                {
                    upper = lower;
                    increment <<= 1;

                    if(increment > upper)
                    {
                        lower = -1;
                        break;
                    }

                    lower = upper - increment;
                }
            }

            return LocateBisection(t, lower, upper);
        }

        /// <summary>
        /// Bisection Search Helper. Do not use this method directly, use <see cref="Locate(double)"/> instead.
        /// </summary>
        int
        LocateBisection(
            double t,
            int lowerIndex,
            int upperIndex)
        {
            while(upperIndex - lowerIndex > 1)
            {
                int midpointIndex = (upperIndex + lowerIndex) >> 1;

                if(t >= _sampleT[midpointIndex])
                {
                    lowerIndex = midpointIndex;
                }
                else
                {
                    upperIndex = midpointIndex;
                }
            }

            if(_size > 0 && Number.AlmostEqual(t, _sampleT[_size - 1]))
            {
                return _size - 2;
            }

            return lowerIndex;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The count of unique samples supported.
        /// </summary>
        public int Capacity
        {
            get
            {
                return _sampleCount.Length;
            }

            set
            {
                if(value != _sampleCount.Length)
                {
                    if(value < _size)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }

                    if(value > 0)
                    {
                        int[] sampleCountNew = new int[value];
                        double[] sampleTNew = new double[value];
                        double[] sampleXNew = new double[value];

                        if(_size > 0)
                        {
                            Array.Copy(_sampleCount, 0, sampleCountNew, 0, _size);
                            Array.Copy(_sampleT, 0, sampleTNew, 0, _size);
                            Array.Copy(_sampleX, 0, sampleXNew, 0, _size);
                        }

                        _sampleCount = sampleCountNew;
                        _sampleT = sampleTNew;
                        _sampleX = sampleXNew;
                    }
                    else
                    {
                        _sampleCount = new int[16];
                        _sampleT = new double[16];
                        _sampleX = new double[16];
                    }
                }
            }
        }

        #endregion

        #region ICloneable Member

        /// <summary>
        /// Create a copy of this sample list.
        /// </summary>
        object
        ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Create a copy of this sample list.
        /// </summary>
        public virtual
        SampleList
        Clone()
        {
            SampleList list = new SampleList(_size);
            Array.Copy(_sampleCount, 0, list._sampleCount, 0, _size);
            Array.Copy(_sampleT, 0, list._sampleT, 0, _size);
            Array.Copy(_sampleX, 0, list._sampleX, 0, _size);
            list._size = _size;
            return list;
        }

        #endregion

        #region ICollection Member

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// The count of unique samples stored.
        /// </summary>
        public int Count
        {
            get { return _size; }
        }

        void
        ICollection.CopyTo(
            Array array,
            int index)
        {
            if(array == null || array.Rank != 1)
            {
                throw new ArgumentException("array", Properties.LocalStrings.ArgumentSingleDimensionArray);
            }

            if(index < 0 || (array.Length - index) < _size)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            for(int i = 0; i < _size; i++)
            {
                DictionaryEntry entry = new DictionaryEntry(_sampleT[i], _sampleX[i]);
                array.SetValue(entry, i + index);
            }
        }

        #endregion

        #region IDictionary Member

        bool IDictionary.IsReadOnly
        {
            get { return false; }
        }

        bool IDictionary.IsFixedSize
        {
            get { return false; }
        }

        object IDictionary.this[object key]
        {
            get
            {
                int index = IndexOfT(Convert.ToDouble(key));
                if(index >= 0)
                {
                    return _sampleX[index];
                }

                return null;
            }

            set
            {
                if(key == null)
                {
                    throw new ArgumentNullException("key");
                }

                double dkey = Convert.ToDouble(key);
                double dvalue = Convert.ToDouble(value);
                SetX(dkey, dvalue);
            }
        }

        bool IDictionary.Contains(object key)
        {
            return ContainsT(Convert.ToDouble(key));
        }

        /// <summary>
        /// True if this sample list contains a sample at t
        /// </summary>
        public
        bool
        ContainsT(double t)
        {
            return IndexOfT(t) != -1;
        }
        
        /// <summary>
        /// True if this sample list contains a sample value x
        /// </summary>
        public
        bool
        ContainsX(double x)
        {
            return IndexOfX(x) != -1;
        }

        void 
        IDictionary.Add(
            object key,
            object value)
        {
            Add(Convert.ToDouble(key), Convert.ToDouble(value));
        }

        void 
        IDictionary.Remove(object key)
        {
            Remove(Convert.ToDouble(key));
        }

        IDictionaryEnumerator
        IDictionary.GetEnumerator()
        {
            return new SampleListEnumerator(this, 0, _size, SampleListEnumerator.EnumerationMode.DictEntry);
        }

        ICollection IDictionary.Keys
        {
            get
            {
                if(_keyList == null)
                {
                    _keyList = new KeyList(this);
                }

                return _keyList;
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                if(_valueList == null)
                {
                    _valueList = new ValueList(this);
                }

                return _valueList;
            }
        }

        [Serializable]
        private sealed class KeyList : IList
        {
            private SampleList sampleList;

            internal KeyList(SampleList sampleList)
            {
                this.sampleList = sampleList;
            }

            public int Add(object key)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(object key)
            {
                return sampleList.ContainsT(Convert.ToDouble(key));
            }

            public void CopyTo(Array array, int index)
            {
                if(array == null || array.Rank != 1)
                {
                    throw new ArgumentException(Properties.LocalStrings.ArgumentSingleDimensionArray, "array");
                }

                Array.Copy(sampleList._sampleT, 0, array, index, sampleList.Count);
            }

            public IEnumerator GetEnumerator()
            {
                return new SampleListEnumerator(sampleList, 0, sampleList.Count, SampleListEnumerator.EnumerationMode.Keys);
            }

            public int IndexOf(object key)
            {
                return sampleList.IndexOfT(Convert.ToDouble(key));
            }

            public void Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            public void Remove(object key)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get { return sampleList.Count; }
            }

            public bool IsFixedSize
            {
                get { return true; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool IsSynchronized
            {
                get { return ((ICollection)sampleList).IsSynchronized; }
            }

            public object this[int index]
            {
                get { return sampleList._sampleT[index]; }
                set { throw new NotSupportedException(); }
            }

            public object SyncRoot
            {
                get { return ((ICollection)sampleList).SyncRoot; }
            }
        }

        [Serializable]
        private sealed class ValueList : IList
        {
            private readonly SampleList _sampleList;

            internal ValueList(SampleList sampleList)
            {
                _sampleList = sampleList;
            }

            public int Add(object value)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(object value)
            {
                return _sampleList.ContainsX(Convert.ToDouble(value));
            }

            public void CopyTo(Array array, int index)
            {
                if(array == null || array.Rank != 1)
                {
                    throw new ArgumentException(Properties.LocalStrings.ArgumentSingleDimensionArray, "array");
                }

                Array.Copy(_sampleList._sampleX, 0, array, index, _sampleList.Count);
            }

            public IEnumerator GetEnumerator()
            {
                return new SampleListEnumerator(_sampleList, 0, _sampleList.Count, SampleListEnumerator.EnumerationMode.Values);
            }

            public int IndexOf(object value)
            {
                return _sampleList.IndexOfX(Convert.ToDouble(value));
            }

            public void Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            public void Remove(object value)
            {
                throw new NotSupportedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get { return _sampleList.Count; }
            }

            public bool IsFixedSize
            {
                get { return true; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool IsSynchronized
            {
                get { return ((ICollection)_sampleList).IsSynchronized; }
            }

            public object this[int index]
            {
                get { return _sampleList._sampleX[index]; }
                set { throw new NotSupportedException(); }
            }

            public object SyncRoot
            {
                get { return ((ICollection)_sampleList).SyncRoot; }
            }
        }
        #endregion

        #region IEnumerable Member

        IEnumerator
        IEnumerable.GetEnumerator()
        {
            return new SampleListEnumerator(this, 0, _size, SampleListEnumerator.EnumerationMode.DictEntry);
        }

        [Serializable]
        private sealed class SampleListEnumerator : IDictionaryEnumerator, ICloneable
        {
            private readonly int _endIndex;
            private readonly EnumerationMode _mode;
            private readonly SampleList _sampleList;
            private readonly int _startIndex;
            private bool _current;
            private int _index;
            private object _key;
            private object _value;

            internal enum EnumerationMode : int
            {
                /// <summary>Enumerate the keys.</summary>
                Keys = 0,

                /// <summary>Enumerate the values.</summary>
                Values = 1,

                /// <summary>Enumerate the dictionary entries.</summary>
                DictEntry = 2
            }

            internal SampleListEnumerator(SampleList sampleList, int index, int count, EnumerationMode mode)
            {
                _sampleList = sampleList;
                _index = index;
                _startIndex = index;
                _endIndex = index + count;
                _mode = mode;
            }

            public object Clone()
            {
                return MemberwiseClone();
            }

            public bool MoveNext()
            {
                if(_index < _endIndex)
                {
                    _key = _sampleList._sampleT[_index];
                    _value = _sampleList._sampleX[_index];
                    _index++;
                    _current = true;
                    return true;
                }

                _key = null;
                _value = null;
                _current = false;
                return false;
            }

            public void Reset()
            {
                _index = _startIndex;
                _current = false;
                _key = null;
                _value = null;
            }

            public object Current
            {
                get
                {
                    if(!_current)
                    {
                        throw new InvalidOperationException();
                    }

                    if(_mode == EnumerationMode.Keys)
                    {
                        return _key;
                    }

                    if(_mode == EnumerationMode.Values)
                    {
                        return _value;
                    }

                    return new DictionaryEntry(_key, _value);
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    if(!_current)
                    {
                        throw new InvalidOperationException();
                    }

                    return new DictionaryEntry(_key, _value);
                }
            }

            public object Key
            {
                get
                {
                    if(!_current)
                    {
                        throw new InvalidOperationException();
                    }

                    return _key;
                }
            }

            public object Value
            {
                get
                {
                    if(!_current)
                    {
                        throw new InvalidOperationException();
                    }

                    return _value;
                }
            }
        }
        #endregion

        #region Event Handler

        /// <summary>
        /// Event Argument for <see cref="SampleList.SampleAltered"/> Event.
        /// </summary>
        public class SampleAlteredEventArgs : EventArgs
        {
            readonly double _t;

            /// <summary>
            /// Initializes a new instance of the SampleAlteredEventArgs class.
            /// </summary>
            /// <param name="t">The t-value of the x=f(t) or (t,x) samples.</param>
            public SampleAlteredEventArgs(double t)
            {
                _t = t;
            }

            /// <summary>
            /// he t-value of the x=f(t) or (t,x) samples.
            /// </summary>
            public double T
            {
                get { return _t; }
            }
        }

        #endregion
    }
}
