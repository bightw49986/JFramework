/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.ObjectPooling
{
    public class CSharpObjectPool<T> : ObjectPoolBase<T, Type>
        where T : class, IPoolableObject<Type>, IDisposable
    {
        public CSharpObjectPool(Type prototype, int initialSize) : base(prototype, initialSize)
        {
            Initialize();
        }

        protected override string PrototypeName => prototype != null ? prototype.Name : "null";

        protected override T CreateInstance(Type prototype)
        {
            return Activator.CreateInstance<T>();
        }

        protected override void DestroyObject(T obj)
        {
            obj.Dispose();
        }
    }
}