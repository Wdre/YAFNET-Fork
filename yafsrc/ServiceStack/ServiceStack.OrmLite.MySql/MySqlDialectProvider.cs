﻿// ***********************************************************************
// <copyright file="MySqlDialectProvider.cs" company="ServiceStack, Inc.">
//     Copyright (c) ServiceStack, Inc. All Rights Reserved.
// </copyright>
// <summary>Fork for YetAnotherForum.NET, Licensed under the Apache License, Version 2.0</summary>
// ***********************************************************************

namespace ServiceStack.OrmLite.MySql;

using System;
using System.Collections.Generic;
using System.Data;

using global::MySql.Data.MySqlClient;

using ServiceStack.OrmLite.MySql.Converters;

/// <summary>
/// Class MySqlDialectProvider.
/// Implements the <see cref="MySqlDialectProvider" />
/// </summary>
/// <seealso cref="MySqlDialectProvider" />
public class MySqlDialectProvider : MySqlDialectProviderBase<MySqlDialectProvider>
{
    /// <summary>
    /// The instance
    /// </summary>
    public static MySqlDialectProvider Instance = new();

    /// <summary>
    /// The text column definition
    /// </summary>
    private const string TextColumnDefinition = "TEXT";

    /// <summary>
    /// Creates the connection.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="options">The options.</param>
    /// <returns>IDbConnection.</returns>
    public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
    {
        return new MySqlConnection(connectionString);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDialectProvider" /> class.
    /// </summary>
    public MySqlDialectProvider()
    {
        this.RegisterConverter<DateTime>(new MySqlDateTimeConverter());
    }

    /// <summary>
    /// Creates the parameter.
    /// </summary>
    /// <returns>IDbDataParameter.</returns>
    public override IDbDataParameter CreateParam()
    {
        return new MySqlParameter();
    }

    /// <summary>
    /// Bulks the insert.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db">The database.</param>
    /// <param name="objs">The objs.</param>
    /// <param name="config">The configuration.</param>
    public override void BulkInsert<T>(IDbConnection db, IEnumerable<T> objs, BulkInsertConfig config = null)
    {
        db.InsertAll(objs);
    }
}

/// <summary>
/// Class MySql55DialectProvider.
/// Implements the <see cref="MySqlDialectProvider" />
/// </summary>
/// <seealso cref="MySqlDialectProvider" />
public class MySql55DialectProvider : MySqlDialectProviderBase<MySqlDialectProvider>
{
    /// <summary>
    /// The instance
    /// </summary>
    public static MySql55DialectProvider Instance = new();

    /// <summary>
    /// The text column definition
    /// </summary>
    private const string TextColumnDefinition = "TEXT";

    /// <summary>
    /// Creates the connection.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="options">The options.</param>
    /// <returns>IDbConnection.</returns>
    public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
    {
        return new MySqlConnection(connectionString);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MySql55DialectProvider" /> class.
    /// </summary>
    public MySql55DialectProvider()
    {
        this.RegisterConverter<DateTime>(new MySql55DateTimeConverter());
        this.RegisterConverter<string>(new MySql55StringConverter());
        this.RegisterConverter<char[]>(new MySql55CharArrayConverter());
    }

    /// <summary>
    /// Creates the parameter.
    /// </summary>
    /// <returns>IDbDataParameter.</returns>
    public override IDbDataParameter CreateParam()
    {
        return new MySqlParameter();
    }
}