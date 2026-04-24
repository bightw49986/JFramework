/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace JFramework.SOAP.ModularSystem
{
    [CreateAssetMenu(menuName = "JFramework/System/Empty")]
    public class SystemModule : ScriptableObject, INode<SystemModule>
    {
        [SerializeField]
        [ListDrawerSettings(Expanded = true)]
        private List<SystemModule> subSystems = new List<SystemModule>();

        public virtual void Initialize(GameObject objectRoot) { }
        public virtual void OnUpdate(float deltaSeconds) { }
        public virtual void Terminate() { }

        #region INode behavior
        public INode<SystemModule> this[int i] => subSystems[i];

        public IReadOnlyCollection<INode<SystemModule>> Children => subSystems.AsReadOnly();

        public INode<SystemModule> Parent { get; private set; }

        public SystemModule Value => this;

        public INode<SystemModule> AddChild(SystemModule value)
        {
            value.Parent = this;
            if (!subSystems.Contains(value))
            {
                subSystems.Add(value);
            }

            return value;
        }

        public INode<SystemModule>[] AddChildren(params SystemModule[] values)
        {
            return values.Select(AddChild).ToArray();
        }

        public IEnumerable<SystemModule> Flatten()
        {
            return new[] { Value }.Concat(subSystems.SelectMany(x => x.Flatten()));
        }

        public bool RemoveChild(INode<SystemModule> node)
        {
            if (node is SystemModule explicitNode)
            {
                return subSystems.Remove(explicitNode);
            }

            throw new Exception("Remove node failed: node is not a system module.");
        }

        public void Traverse(Action<SystemModule> action)
        {
            action(Value);
            foreach (var child in subSystems)
                child.Traverse(action);
        }
        #endregion

    }
}


