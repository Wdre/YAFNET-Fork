﻿/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2024 Ingo Herbote
 * https://www.yetanotherforum.net/
 * 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace YAF.Tests.AdminTests.Users;

/// <summary>
/// The Role (Group) Tests
/// </summary>
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class RoleTests : TestBase
{
    /// <summary>
    /// Create a new Test Group (Role)
    /// </summary>
    [Test]
    [Description("Create a new Test Group (Role)")]
    public async Task CreateNewGroupTest()
    {
        await this.Base.PlaywrightFixture.GotoPageAsync(
            this.Base.TestSettings.TestForumUrl,
            async page =>
                {
                    // Log user in first!
                    Assert.That(
                        await page.LoginUserAsync(
                            this.Base.TestSettings,
                            this.Base.TestSettings.AdminUserName,
                            this.Base.TestSettings.AdminPassword), Is.True,
                        "Login failed");

                    // Do actual test

                    var random = new Random();

                    var roleName = $"TestRole{random.Next()}";

                    await page.GotoAsync($"{this.Base.TestSettings.TestForumUrl}Admin/Groups");

                    await page.Locator("//a[contains(@href,'EditGroup')]").Last.ClickAsync();

                    await page.Locator("#Input_Name").FillAsync(roleName);
                    await page.Locator("#Input_Description").FillAsync("Test Role for testing");

                    await page.Locator("#Input_NewAccessMaskID").SelectOptionAsync(["3"]);

                    await page.Locator("#Save").ClickAsync();

                    var pageSource = await page.ContentAsync();

                    Assert.That(pageSource, Does.Contain(roleName), "Test Role creating failed");
                },
            this.BrowserType);
    }
}