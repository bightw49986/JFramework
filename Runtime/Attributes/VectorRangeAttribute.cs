/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEngine;

namespace JFramework
{
    /// <summary>
    /// 在Inspector中為Vector2字段設置範圍限制
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = false)]
    public class Vector2RangeAttribute : PropertyAttribute
    {
        // Min/Max values for the X axis
        public readonly float MinX;
        public readonly float MaxX;
        // Min/Max values for the Y axis
        public readonly float MinY;
        public readonly float MaxY;

        public Vector2RangeAttribute(float fMinX, float fMaxX, float fMinY, float fMaxY)
        {
            MinX = fMinX;
            MaxX = fMaxX;
            MinY = fMinY;
            MaxY = fMaxY;
        }
    }

    /// <summary>
    /// 在Inspector中為Vector3字段設置範圍限制
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = false)]
    public class Vector3RangeAttribute : PropertyAttribute
    {
        // Min/Max values for the X axis
        public readonly float MinX;
        public readonly float MaxX;
        // Min/Max values for the Y axis
        public readonly float MinY;
        public readonly float MaxY;
        // Min/Max values for the Z axis
        public readonly float MinZ;
        public readonly float MaxZ;

        public Vector3RangeAttribute(float fMinX, float fMaxX, float fMinY, float fMaxY, float fMinZ, float fMaxZ)
        {
            MinX = fMinX;
            MaxX = fMaxX;
            MinY = fMinY;
            MaxY = fMaxY;
            MinZ = fMinZ;
            MaxZ = fMaxZ;
        }
    }
}