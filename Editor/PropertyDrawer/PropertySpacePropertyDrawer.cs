/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using UnityEditor;
using UnityEngine;

namespace JFramework.Editor
{
    /// <summary>
    /// PropertySpace屬性的PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(PropertySpaceAttribute))]
    public class PropertySpacePropertyDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var propertySpaceAttribute = (PropertySpaceAttribute)attribute;
            return propertySpaceAttribute.Space;
        }

        public override void OnGUI(Rect position)
        {
            // 不需要繪製任何內容，只是添加空間
        }
    }
}