/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework.SOAP.ModularSystem
{
    /// <summary>
    /// 定義這個階段（通常為場景）需要運行那些系統模組
    /// </summary>
    public class SystemRegistry : MonoBehaviour
    {
        [SerializeField]
        private SystemModule rootModule;

        private void Awake()
        {
            SystemRunner.Instance.AddChildModule(rootModule, gameObject);
        }
        
        private void OnDestroy()
        {
            if (SystemRunner.Instance != null)
                SystemRunner.Instance.RemoveChildModule(rootModule);
        }
    }
}
