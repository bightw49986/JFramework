/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿namespace JFramework
{
    /// <summary>
    /// JFramework 統一的更新時機枚舉
    /// 定義各種組件可以使用的更新時機類型
    /// </summary>
    public enum UpdateTiming
    {
        /// <summary>
        /// 手動更新，不自動執行
        /// </summary>
        Manual,
        
        /// <summary>
        /// 每幀更新（Update）
        /// </summary>
        Update,
        
        /// <summary>
        /// 固定時間間隔更新（FixedUpdate）
        /// </summary>
        FixedUpdate,
        
        /// <summary>
        /// 延遲更新（LateUpdate）
        /// </summary>
        LateUpdate,
        
        /// <summary>
        /// 自定義時間間隔更新
        /// </summary>
        CustomInterval
    }
}