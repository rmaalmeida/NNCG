//-----------------------------------------------------------------------
// <copyright file="Sorting.cs" company="Math.NET Project">
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
using System.Collections.Generic;

namespace MathNet.Numerics
{
    /// <summary>
    /// Sorting algorithms for single, tuple and triple lists.
    /// </summary>
    public static class Sorting
    {
        /// <summary>
        /// Sort a list of keys, inplace.
        /// </summary>
        /// <param name="keys">List to sort.</param>
        public static
        void
        Sort<T>(IList<T> keys)
        {
            Sort(keys, Comparer<T>.Default);
        }

        /// <summary>
        /// Sort a list of keys and items with respect to the keys, inplace.
        /// </summary>
        /// <param name="keys">List to sort.</param>
        /// <param name="items">List to permute the same way as the key list.</param>
        public static
        void
        Sort<TKey, TItem>(
            IList<TKey> keys,
            IList<TItem> items)
        {
            Sort(keys, items, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Sort a list of keys, items1 and items2 with respect to the keys, inplace.
        /// </summary>
        /// <param name="keys">List to sort.</param>
        /// <param name="items1">First list to permute the same way as the key list.</param>
        /// <param name="items2">Second list to permute the same way as the key list.</param>
        public static
        void
        Sort<TKey, TItem1, TItem2>(
            IList<TKey> keys,
            IList<TItem1> items1,
            IList<TItem2> items2)
        {
            Sort(keys, items1, items2, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Sort a range of a list of keys, inplace.
        /// </summary>
        /// <param name="keys">List to sort.</param>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="count">The length of the range to sort.</param>
        public static
        void
        Sort<T>(
            IList<T> keys,
            int index,
            int count)
        {
            Sort(keys, index, count, Comparer<T>.Default);
        }

        /// <summary>
        /// Sort a list of keys, inplace.
        /// </summary>
        /// <param name="keys">List to sort.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static
        void
        Sort<T>(
            IList<T> keys,
            IComparer<T> comparer)
        {
            if(null == keys)
            {
                throw new ArgumentNullException("keys");
            }

            if(null == comparer)
            {
                throw new ArgumentNullException("comparer");
            }

            // basic cases
            if(keys.Count <= 1)
            {
                return;
            }

            if(keys.Count == 2)
            {
                if(comparer.Compare(keys[0], keys[1]) > 0)
                {
                    Swap(keys, 0, 1);
                }

                return;
            }

            // generic list case
            List<T> list = keys as List<T>;
            if(null != list)
            {
                list.Sort(comparer);
                return;
            }

            // array case
            T[] array = keys as T[];
            if(null != array)
            {
                Array.Sort(array, comparer);
                return;
            }

            // local sort implementation
            QuickSort(keys, comparer, 0, keys.Count - 1);
        }

        /// <summary>
        /// Sort a list of keys and items with respect to the keys, inplace.
        /// </summary>
        /// <param name="keys">List to sort.</param>
        /// <param name="items">List to permute the same way as the key list.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static
        void
        Sort<TKey, TItem>(
            IList<TKey> keys,
            IList<TItem> items,
            IComparer<TKey> comparer)
        {
            if(null == keys)
            {
                throw new ArgumentNullException("keys");
            }

            if(null == items)
            {
                throw new ArgumentNullException("items");
            }

            if(null == comparer)
            {
                throw new ArgumentNullException("comparer");
            }

            // array case
            TKey[] keysArray = keys as TKey[];
            TItem[] itemsArray = items as TItem[];
            if((null != keysArray) && (null != itemsArray))
            {
                Array.Sort(keysArray, itemsArray, comparer);
                return;
            }

            // local sort implementation
            QuickSort(keys, items, comparer, 0, keys.Count - 1);
        }

        /// <summary>
        /// Sort a list of keys, items1 and items2 with respect to the keys, inplace.
        /// </summary>
        /// <param name="keys">List to sort.</param>
        /// <param name="items1">First list to permute the same way as the key list.</param>
        /// <param name="items2">Second list to permute the same way as the key list.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static
        void
        Sort<TKey, TItem1, TItem2>(
            IList<TKey> keys,
            IList<TItem1> items1,
            IList<TItem2> items2,
            IComparer<TKey> comparer)
        {
            if(null == keys)
            {
                throw new ArgumentNullException("keys");
            }

            if(null == items1)
            {
                throw new ArgumentNullException("items1");
            }

            if(null == items2)
            {
                throw new ArgumentNullException("items2");
            }

            if(null == comparer)
            {
                throw new ArgumentNullException("comparer");
            }

            // local sort implementation
            QuickSort(keys, items1, items2, comparer, 0, keys.Count - 1);
        }

        /// <summary>
        /// Sort a range of a list of keys, inplace.
        /// </summary>
        /// <param name="keys">List to sort.</param>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="count">The length of the range to sort.</param>
        /// <param name="comparer">Comparison, defining the sort order.</param>
        public static
        void
        Sort<T>(
            IList<T> keys,
            int index,
            int count,
            IComparer<T> comparer)
        {
            if(null == keys)
            {
                throw new ArgumentNullException("keys");
            }

            if(null == comparer)
            {
                throw new ArgumentNullException("comparer");
            }

            if(index < 0 || index >= keys.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if(count < 0 || index + count > keys.Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // basic cases
            if(count <= 1)
            {
                return;
            }

            if(count == 2)
            {
                if(comparer.Compare(keys[index], keys[index + 1]) > 0)
                {
                    Swap(keys, index, index + 1);
                }

                return;
            }

            // generic list case
            List<T> list = keys as List<T>;
            if(null != list)
            {
                list.Sort(index, count, comparer);
                return;
            }

            // array case
            T[] array = keys as T[];
            if(null != array)
            {
                Array.Sort(array, index, count, comparer);
                return;
            }

            // local sort implementation
            QuickSort(keys, comparer, index, count - 1);
        }

        static
        void
        QuickSort<T>(
            IList<T> keys,
            IComparer<T> comparer,
            int left,
            int right)
        {
            do
            {
                // Pivoting
                int a = left;
                int b = right;
                int p = a + ((b - a) >> 1); // midpoint

                if(comparer.Compare(keys[a], keys[p]) > 0)
                {
                    Swap(keys, a, p);
                }

                if(comparer.Compare(keys[a], keys[b]) > 0)
                {
                    Swap(keys, a, b);
                }

                if(comparer.Compare(keys[p], keys[b]) > 0)
                {
                    Swap(keys, p, b);
                }

                T pivot = keys[p];

                // Hoare Partitioning
                do
                {
                    while(comparer.Compare(keys[a], pivot) < 0)
                    {
                        a++;
                    }

                    while(comparer.Compare(pivot, keys[b]) < 0)
                    {
                        b--;
                    }

                    if(a > b)
                    {
                        break;
                    }

                    if(a < b)
                    {
                        Swap(keys, a, b);
                    }

                    a++;
                    b--;
                }
                while(a <= b);

                // In order to limit the recusion depth to log(n), we sort the 
                // shorter partition recusively and the longer partition iteratively.
                if((b - left) <= (right - a))
                {
                    if(left < b)
                    {
                        QuickSort(keys, comparer, left, b);
                    }

                    left = a;
                }
                else
                {
                    if(a < right)
                    {
                        QuickSort(keys, comparer, a, right);
                    }

                    right = b;
                }
            }
            while(left < right);
        }

        static
        void
        QuickSort<T, TItems>(
            IList<T> keys,
            IList<TItems> items,
            IComparer<T> comparer,
            int left,
            int right)
        {
            do
            {
                // Pivoting
                int a = left;
                int b = right;
                int p = a + ((b - a) >> 1); // midpoint

                if(comparer.Compare(keys[a], keys[p]) > 0)
                {
                    Swap(keys, a, p);
                    Swap(items, a, p);
                }

                if(comparer.Compare(keys[a], keys[b]) > 0)
                {
                    Swap(keys, a, b);
                    Swap(items, a, b);
                }

                if(comparer.Compare(keys[p], keys[b]) > 0)
                {
                    Swap(keys, p, b);
                    Swap(items, p, b);
                }

                T pivot = keys[p];

                // Hoare Partitioning
                do
                {
                    while(comparer.Compare(keys[a], pivot) < 0)
                    {
                        a++;
                    }

                    while(comparer.Compare(pivot, keys[b]) < 0)
                    {
                        b--;
                    }

                    if(a > b)
                    {
                        break;
                    }

                    if(a < b)
                    {
                        Swap(keys, a, b);
                        Swap(items, a, b);
                    }

                    a++;
                    b--;
                }
                while(a <= b);

                // In order to limit the recusion depth to log(n), we sort the 
                // shorter partition recusively and the longer partition iteratively.
                if((b - left) <= (right - a))
                {
                    if(left < b)
                    {
                        QuickSort(keys, items, comparer, left, b);
                    }

                    left = a;
                }
                else
                {
                    if(a < right)
                    {
                        QuickSort(keys, items, comparer, a, right);
                    }

                    right = b;
                }
            }
            while(left < right);
        }

        static
        void
        QuickSort<T, TItems1, TItems2>(
            IList<T> keys,
            IList<TItems1> items1,
            IList<TItems2> items2,
            IComparer<T> comparer,
            int left,
            int right)
        {
            do
            {
                // Pivoting
                int a = left;
                int b = right;
                int p = a + ((b - a) >> 1); // midpoint

                if(comparer.Compare(keys[a], keys[p]) > 0)
                {
                    Swap(keys, a, p);
                    Swap(items1, a, p);
                    Swap(items2, a, p);
                }

                if(comparer.Compare(keys[a], keys[b]) > 0)
                {
                    Swap(keys, a, b);
                    Swap(items1, a, b);
                    Swap(items2, a, b);
                }

                if(comparer.Compare(keys[p], keys[b]) > 0)
                {
                    Swap(keys, p, b);
                    Swap(items1, p, b);
                    Swap(items2, p, b);
                }

                T pivot = keys[p];

                // Hoare Partitioning
                do
                {
                    while(comparer.Compare(keys[a], pivot) < 0)
                    {
                        a++;
                    }

                    while(comparer.Compare(pivot, keys[b]) < 0)
                    {
                        b--;
                    }

                    if(a > b)
                    {
                        break;
                    }

                    if(a < b)
                    {
                        Swap(keys, a, b);
                        Swap(items1, a, b);
                        Swap(items2, a, b);
                    }

                    a++;
                    b--;
                }
                while(a <= b);

                // In order to limit the recusion depth to log(n), we sort the 
                // shorter partition recusively and the longer partition iteratively.
                if((b - left) <= (right - a))
                {
                    if(left < b)
                    {
                        QuickSort(keys, items1, items2, comparer, left, b);
                    }

                    left = a;
                }
                else
                {
                    if(a < right)
                    {
                        QuickSort(keys, items1, items2, comparer, a, right);
                    }

                    right = b;
                }
            }
            while(left < right);
        }

        internal static
        void
        Swap<T>(
            IList<T> keys,
            int a,
            int b)
        {
            T local = keys[a];
            keys[a] = keys[b];
            keys[b] = local;
        }
    }
}
