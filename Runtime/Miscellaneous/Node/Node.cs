/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace JFramework
{
    public class Node<T> : INode<T>
    {
        private readonly T value;
        private readonly List<INode<T>> children = new List<INode<T>>();

        public Node(T value, INode<T> parent = null, T[] children = null)
        {
            this.value = value;
            this.Parent = parent;
            
            if (null != children)
            {
                foreach (var child in children)
                {
                    AddChild(child);
                }
            }
        }

        public INode<T> this[int i]
        {
            get { return children[i]; }
        }

        public INode<T> Parent { get; private set; }

        public T Value { get { return value; } }

        public IReadOnlyCollection<INode<T>> Children
        {
            get { return children.AsReadOnly(); }
        }

        public INode<T> AddChild(T value)
        {
            var node = new Node<T>(value, this);
            children.Add(node);
            return node;
        }

        public INode<T>[] AddChildren(params T[] values)
        {
            return values.Select(AddChild).ToArray();
        }

        public bool RemoveChild(INode<T> node)
        {
            return children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in children)
                child.Traverse(action);
        }

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Concat(children.SelectMany(x => x.Flatten()));
        }
    }
}