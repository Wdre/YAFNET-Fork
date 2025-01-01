/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2025 Ingo Herbote
 * https://www.yetanotherforum.net/
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * https://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using System;

namespace YAF.Core.Events;

using YAF.Core.Hubs;
using YAF.Types.Attributes;

/// <summary>
/// Activity Notification Event
/// </summary>
[ExportService(ServiceLifetimeScope.OwnedByContainer)]
public class ActivityNotification : IHaveServiceLocator, IHandleEvent<ActivityNotificationEvent>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ActivityNotification"/> class.
    /// </summary>
    /// <param name="serviceLocator">
    /// The service locator.
    /// </param>
    public ActivityNotification(IServiceLocator serviceLocator)
    {
        ArgumentNullException.ThrowIfNull(serviceLocator);

        this.ServiceLocator = serviceLocator;
    }

    /// <summary>
    ///   Gets or sets ServiceLocator.
    /// </summary>
    public IServiceLocator ServiceLocator { get; set; }

    /// <summary>
    ///     Gets the order.
    /// </summary>
    public int Order => 10000;

    /// <summary>
    /// The handle.
    /// </summary>
    /// <param name="event">
    /// The event.
    /// </param>
    public void Handle(ActivityNotificationEvent @event)
    {
        this.Get<NotificationClient>().SendActivityAsync(@event.UserName, 1);
    }
}