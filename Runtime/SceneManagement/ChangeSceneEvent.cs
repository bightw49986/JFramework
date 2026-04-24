/*
 * JFramework - Unity Development Framework
 * Copyright (c) 2026 Jonathan Ho. All rights reserved.
 */

﻿using JFramework.SOAP.Event;
using UnityEngine;

namespace JFramework.SceneManagement
{
    [CreateAssetMenu(menuName = "JFramework/Event/1 arg/ChangeScene", fileName = "ChangeSceneEvent")]
    public class ChangeSceneEvent : StaticEvent<ChangeSceneInfo>
    {
    }
}