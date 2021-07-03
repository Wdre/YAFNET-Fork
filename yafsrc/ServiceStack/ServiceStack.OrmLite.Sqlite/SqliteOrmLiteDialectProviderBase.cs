namespace ServiceStack.OrmLite.Sqlite
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ServiceStack.OrmLite.Sqlite.Converters;
    using ServiceStack.Text;

    public abstract class SqliteOrmLiteDialectProviderBase : OrmLiteDialectProviderBase<SqliteOrmLiteDialectProviderBase>
    {
        protected SqliteOrmLiteDialectProviderBase()
        {
            base.SelectIdentitySql = "SELECT last_insert_rowid()";

            base.InitColumnTypeMap();

            OrmLiteConfig.DeoptimizeReader = true;
            base.RegisterConverter<DateTime>(new SqliteCoreDateTimeConverter());
            //Old behavior using native sqlite3.dll
            //base.RegisterConverter<DateTime>(new SqliteNativeDateTimeConverter());

            base.RegisterConverter<string>(new SqliteStringConverter());
            base.RegisterConverter<DateTimeOffset>(new SqliteDateTimeOffsetConverter());
            base.RegisterConverter<Guid>(new SqliteGuidConverter());
            base.RegisterConverter<bool>(new SqliteBoolConverter());
            base.RegisterConverter<byte[]>(new SqliteByteArrayConverter());
#if NETSTANDARD2_0
            base.RegisterConverter<char>(new SqliteCharConverter());
#endif
            this.Variables = new Dictionary<string, string>
            {
                { OrmLiteVariables.SystemUtc, "CURRENT_TIMESTAMP" },
                { OrmLiteVariables.MaxText, "VARCHAR(1000000)" },
                { OrmLiteVariables.MaxTextUnicode, "NVARCHAR(1000000)" },
                { OrmLiteVariables.True, SqlBool(true) },
                { OrmLiteVariables.False, SqlBool(false) },
            };
        }

        public static string Password { get; set; }
        public static bool UTF8Encoded { get; set; }
        public static bool ParseViaFramework { get; set; }

        public static string RowVersionTriggerFormat = "{0}RowVersionUpdateTrigger";

        public override string ToPostDropTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = GetTriggerName(modelDef);
                return $"DROP TRIGGER IF EXISTS {GetQuotedName(triggerName)}";
            }

            return null;
        }

        private string GetTriggerName(ModelDefinition modelDef)
        {
            return RowVersionTriggerFormat.Fmt(GetTableName(modelDef));
        }

        public override string ToPostCreateTableStatement(ModelDefinition modelDef)
        {
            if (modelDef.RowVersion != null)
            {
                var triggerName = GetTriggerName(modelDef);
                var tableName = GetTableName(modelDef);
                var triggerBody = string.Format("UPDATE {0} SET {1} = OLD.{1} + 1 WHERE {2} = NEW.{2};",
                    tableName,
                    modelDef.RowVersion.FieldName.SqlColumn(this),
                    modelDef.PrimaryKey.FieldName.SqlColumn(this));

                var sql = $"CREATE TRIGGER {triggerName} BEFORE UPDATE ON {tableName} FOR EACH ROW BEGIN {triggerBody} END;";

                return sql;
            }

            return null;
        }

        public static string CreateFullTextCreateTableStatement(object objectWithProperties)
        {
            var sbColumns = StringBuilderCache.Allocate();
            foreach (var propertyInfo in objectWithProperties.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var columnDefinition = (sbColumns.Length == 0)
                    ? $"{propertyInfo.Name} TEXT PRIMARY KEY"
                    : $", {propertyInfo.Name} TEXT";

                sbColumns.AppendLine(columnDefinition);
            }

            var tableName = objectWithProperties.GetType().Name;
            var sql = $"CREATE VIRTUAL TABLE \"{tableName}\" USING FTS3 ({StringBuilderCache.ReturnAndFree(sbColumns)});";

            return sql;
        }

        public override IDbConnection CreateConnection(string connectionString, Dictionary<string, string> options)
        {
            var isFullConnectionString = connectionString.Contains(";");
            var connString = StringBuilderCache.Allocate();
            if (!isFullConnectionString)
            {
                if (connectionString != ":memory:")
                {
                    var existingDir = Path.GetDirectoryName(connectionString);
                    if (!string.IsNullOrEmpty(existingDir) && !Directory.Exists(existingDir))
                    {
                        Directory.CreateDirectory(existingDir);
                    }
                }
#if NETSTANDARD2_0
                connString.AppendFormat(@"Data Source={0};", connectionString.Trim());
#else
                connString.AppendFormat(@"Data Source={0};Version=3;New=True;Compress=True;", connectionString.Trim());
#endif
            }
            else
            {
                connString.Append(connectionString);
            }
            if (!string.IsNullOrEmpty(Password))
            {
                connString.AppendFormat("Password={0};", Password);
            }
            if (UTF8Encoded)
            {
                connString.Append("UseUTF16Encoding=True;");
            }

            if (options != null)
            {
                foreach (var option in options)
                {
                    connString.AppendFormat("{0}={1};", option.Key, option.Value);
                }
            }

            return CreateConnection(StringBuilderCache.ReturnAndFree(connString));
        }

        protected abstract IDbConnection CreateConnection(string connectionString);

        public override string GetQuotedName(string name, string schema) => GetQuotedName(name); //schema name is embedded in table name in MySql

        public override string ToTableNamesStatement(string schema)
        {
            return schema == null
                ? "SELECT name FROM sqlite_master WHERE type ='table' AND name NOT LIKE 'sqlite_%'"
                : "SELECT name FROM sqlite_master WHERE type ='table' AND name LIKE {0}".SqlFmt(this, GetTableName("",schema) + "%");
        }

        public override string GetSchemaName(string schema)
        {
            return schema != null
                ? NamingStrategy.GetSchemaName(schema).Replace(".", "_")
                : NamingStrategy.GetSchemaName(schema);
        }

        public override string GetTableName(string table, string schema = null) => GetTableName(table, schema, useStrategy: true);

        public override string GetTableName(string table, string schema, bool useStrategy)
        {
            if (useStrategy)
            {
                return schema != null && !table.StartsWithIgnoreCase(schema + "_")
                    ? $"{NamingStrategy.GetSchemaName(schema)}_{NamingStrategy.GetTableName(table)}"
                    : NamingStrategy.GetTableName(table);
            }

            return schema != null && !table.StartsWithIgnoreCase(schema + "_")
                ? $"{schema}_{table}"
                : table;
        }

        public override string GetQuotedTableName(string tableName, string schema = null) =>
            GetQuotedName(GetTableName(tableName, schema));

        public override SqlExpression<T> SqlExpression<T>()
        {
            return new SqliteExpression<T>(this);
        }

        public override bool DoesSchemaExist(IDbCommand dbCmd, string schemaName)
        {
            throw new NotImplementedException("Schemas are not supported by sqlite");
        }

        public override string ToCreateSchemaStatement(string schemaName)
        {
            throw new NotImplementedException("Schemas are not supported by sqlite");
        }

        public override bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null)
        {
            var sql = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = {0}"
                .SqlFmt(GetTableName(tableName, schema));

            dbCmd.CommandText = sql;
            var result = dbCmd.LongScalar();

            return result > 0;
        }

        public override bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null)
        {
            var sql = "PRAGMA table_info({0})"
                .SqlFmt(GetTableName(tableName, schema));

            var columns = db.SqlList<Dictionary<string, object>>(sql);
            foreach (var column in columns)
            {
                if (column.TryGetValue("name", out var name) && name.ToString().EqualsIgnoreCase(columnName))
                    return true;
            }
            return false;
        }

        public override string GetColumnDefinition(FieldDefinition fieldDef)
        {
            // http://www.sqlite.org/lang_createtable.html#rowid
            var ret = base.GetColumnDefinition(fieldDef);
            if (fieldDef.IsPrimaryKey)
                return ret.Replace(" BIGINT ", " INTEGER ");
            if (fieldDef.IsRowVersion)
                return ret + " DEFAULT 1";

            return ret;
        }

        public override string SqlConflict(string sql, string conflictResolution)
        {
            // http://www.sqlite.org/lang_conflict.html
            var parts = sql.SplitOnFirst(' ');
            return parts[0] + " OR " + conflictResolution + " " + parts[1];
        }

        public override string SqlConcat(IEnumerable<object> args) => string.Join(" || ", args);

        public override string SqlCurrency(string fieldOrValue, string currencySymbol) => SqlConcat(new []{ "'" + currencySymbol + "'", "printf(\"%.2f\", " + fieldOrValue + ")" });

        public override string SqlBool(bool value) => value ? "1" : "0";

        public override string SqlRandom => "random()";

        public override void EnableForeignKeysCheck(IDbCommand cmd) => cmd.ExecNonQuery("PRAGMA foreign_keys = ON;");
        public override Task EnableForeignKeysCheckAsync(IDbCommand cmd, CancellationToken token = default) =>
            cmd.ExecNonQueryAsync("PRAGMA foreign_keys = ON;", null, token);

        public override void DisableForeignKeysCheck(IDbCommand cmd) => cmd.ExecNonQuery("PRAGMA foreign_keys = OFF;");
        public override Task DisableForeignKeysCheckAsync(IDbCommand cmd, CancellationToken token = default) =>
            cmd.ExecNonQueryAsync("PRAGMA foreign_keys = OFF;", null, token);

        public override string GetDropFunction(string database, string functionName)
        {
            return string.Empty; // Not Supported in Sqlite
        }

        public override string GetCreateView(string database, ModelDefinition modelDef, StringBuilder selectSql)
        {
            var sb = StringBuilderCache.Allocate();

            var tableName = $"{base.NamingStrategy.GetTableName(modelDef.ModelName)}";

            sb.AppendFormat("CREATE VIEW {0} as ", tableName);

            sb.Append(selectSql);

            sb.Append(";");

            return StringBuilderCache.ReturnAndFree(sb);
        }

        public override string GetDropView(string database, ModelDefinition modelDef)
        {
            var sb = StringBuilderCache.Allocate();

            var tableName = $"{database}.{base.NamingStrategy.GetTableName(modelDef.ModelName)}";

            sb.Append("DROP VIEW IF EXISTS ");
            sb.AppendFormat("{0}", tableName);

            sb.Append(";");

            return StringBuilderCache.ReturnAndFree(sb);
        }

        public override string GetUtcDateFunction()
        {
            return "datetime('now')";
        }

        public override string DateDiffFunction(string interval, string date1, string date2)
        {
            return interval == "minute"
                ? $@"((JulianDay({date2}) - JulianDay({date1})) * 24 * 60)"
                : $@"(JulianDay({date2}) - JulianDay({date1}))";
        }

        /// <summary>
        /// Gets the SQL ISNULL Function
        /// </summary>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <param name="alternateValue">
        /// The alternate Value.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string IsNullFunction(string expression, object alternateValue)
        {
            return $"IFNULL(({expression}), {alternateValue})";
        }

        public override string ConvertFlag(string expression)
        {
            var value = expression.Substring(expression.Length - 1, 1);

            return $"CAST(({expression} = {value}) as integer)";
        }

        public override string DatabaseFragmentationInfo(string database)
        {
            return string.Empty;// NOT SUPPORTED
        }

        public override string DatabaseSize(string database)
        {
            return "SELECT (page_count * page_size) / 1048576.0 as size FROM pragma_page_count(), pragma_page_size();";
        }

        public override string SQLVersion()
        {
            return "select sqlite_version()";
        }

        public override string SQLServerName()
        {
            return "SQLite";
        }

        public override string ShrinkDatabase(string database)
        {
            return "PRAGMA auto_vacuum = FULL;";
        }

        public override string ReIndexDatabase(string database, string objectQualifier)
        {
            return "PRAGMA auto_vacuum = INCREMENTAL;";
        }

        public override string ChangeRecoveryMode(string database, string mode)
        {
            return string.Empty;// NOT SUPPORTED
        }

        /// <summary>
        /// Just runs the SQL command according to specifications.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <returns>
        /// Returns the Results
        /// </returns>
        public override string InnerRunSqlExecuteReader(IDbCommand command)
        {
            var sqlCommand = command as SQLiteCommand;

            SQLiteDataReader reader = null;
            var results = new StringBuilder();

            try
            {
                try
                {
                    reader = sqlCommand.ExecuteReader();

                    if (reader.HasRows)
                    {
                        var rowIndex = 1;
                        var columnNames = reader.GetSchemaTable().Rows.Cast<DataRow>()
                            .Select(r => r["ColumnName"].ToString()).ToList();

                        results.Append("RowNumber");

                        columnNames.ForEach(
                            n =>
                            {
                                results.Append(",");
                                results.Append(n);
                            });

                        results.AppendLine();

                        while (reader.Read())
                        {
                            results.AppendFormat(@"""{0}""", rowIndex++);

                            // dump all columns...
                            columnNames.ForEach(
                                col => results.AppendFormat(
                                    @",""{0}""",
                                    reader[col].ToString().Replace("\"", "\"\"")));

                            results.AppendLine();
                        }
                    }
                    else if (reader.RecordsAffected > 0)
                    {
                        results.AppendFormat("{0} Record(s) Affected", reader.RecordsAffected);
                        results.AppendLine();
                    }
                    else
                    {
                        results.AppendLine("No Results Returned.");
                    }

                    reader.Close();

                    command.Transaction?.Commit();
                }
                finally
                {
                    command.Transaction?.Rollback();
                }
            }
            catch (Exception x)
            {
                reader?.Close();

                results.AppendLine();
                results.AppendFormat("SQL ERROR: {0}", x);
            }

            return results.ToString();
        }
    }

    public static class SqliteExtensions
    {
        public static IOrmLiteDialectProvider Configure(this IOrmLiteDialectProvider provider,
            string password = null, bool parseViaFramework = false, bool utf8Encoding = false)
        {
            if (password != null)
                SqliteOrmLiteDialectProviderBase.Password = password;
            if (parseViaFramework)
                SqliteOrmLiteDialectProviderBase.ParseViaFramework = true;
            if (utf8Encoding)
                SqliteOrmLiteDialectProviderBase.UTF8Encoded = true;

            return provider;
        }
    }
}
