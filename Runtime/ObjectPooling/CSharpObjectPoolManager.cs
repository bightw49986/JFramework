/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using System;

namespace JFramework.ObjectPooling
{
    public class CSharpObjectPoolManager<T> : ObjectPoolManagerBase<T, Type, CSharpObjectPool<T>>
        where T : class, IPoolableObject<Type>, IDisposable
    {
        public static CSharpObjectPoolManager<T> Instance
        {
            get
            {
                return instance;
            }
        }
        private static readonly CSharpObjectPoolManager<T> instance = new CSharpObjectPoolManager<T>(defaultPoolSize: 1);

        public CSharpObjectPoolManager(int defaultPoolSize = 10)
            : base(defaultPoolSize) { }

        #region ObjectPoolManagerBase Implementation
        protected override CSharpObjectPool<T> CreatePoolInstance(Type prototype, int initialSize)
        {
            return new CSharpObjectPool<T>(prototype, initialSize);
        }

        protected override string GetPrototypeName(Type prototype)
        {
            return prototype != null ? prototype.Name : "null";
        }
        #endregion
    }
}