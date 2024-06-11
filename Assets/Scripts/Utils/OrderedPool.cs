using System;
using System.Collections.Generic;

namespace Utils
{
    /// <summary>
    /// Utility class used to manage ordered item instances in a way that allows for ordered extraction and
    /// re-inserting of all instances. When more instances than the amount present in the pool are requested, new
    /// instances are created.
    /// </summary>
    public sealed class OrderedPool<T>
    {
        /// <summary>The underlying data structure to store the item in order.</summary>
        private readonly List<T> _elements;
        /// <summary>The amount of items extracted from the pool.</summary>
        private int _usedCount;

        /// <summary>The controller used to manage the item instantiation and rewinding.</summary>
        private readonly IOrderedPoolController<T> _controller;

        /// <summary>Instantiates a new item through the controller and adds it to the pool.</summary>
        private T Istantiate()
        {
            var t = _controller.Instantiate(_elements.Count);
            _elements.Add(t);
            return t;
        }
        
        /// <summary>Instantiates a new item through the controller and adds it to the pool.</summary>
        public OrderedPool(int startingSize, IOrderedPoolController<T> controller)
        {
            _controller = controller;
            _elements = new List<T>(startingSize);
            for (var i = 0; i < startingSize; i++) Istantiate();
        }

        /// <summary>Extracts an item from the pool if more are available, else instantiates a new one.</summary>
        public T Extract()
        {
            var t = _usedCount < _elements.Count ? _elements[_usedCount++] : Istantiate();
            return t;
        }

        /// <summary>Reverst all items' state to their state before being extracted.</summary>
        public void Rewind()
        {
            for (var i = 0; i < _usedCount; i++) _controller.Rewind(_elements[i]);
            _usedCount = 0;
        }
    }
}