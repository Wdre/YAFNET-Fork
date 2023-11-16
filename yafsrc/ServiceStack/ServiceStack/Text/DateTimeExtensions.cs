﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="ServiceStack, Inc.">
//   Copyright (c) ServiceStack, Inc. All Rights Reserved.
// </copyright>
// <summary>
//   Fork for YetAnotherForum.NET, Licensed under the Apache License, Version 2.0
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ServiceStack.Text;

using System;
using System.Globalization;

using ServiceStack.Text.Common;

/// <summary>
/// A fast, standards-based, serialization-issue free DateTime serializer.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// The unix epoch
    /// </summary>
    public const long UnixEpoch = 621355968000000000L;
    /// <summary>
    /// The unix epoch date time UTC
    /// </summary>
    private readonly static DateTime UnixEpochDateTimeUtc = new(UnixEpoch, DateTimeKind.Utc);
    /// <summary>
    /// The unix epoch date time unspecified
    /// </summary>
    private readonly static DateTime UnixEpochDateTimeUnspecified = new(UnixEpoch, DateTimeKind.Unspecified);
    /// <summary>
    /// The minimum date time UTC
    /// </summary>
    private readonly static DateTime MinDateTimeUtc = new(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Froms the unix time.
    /// </summary>
    /// <param name="unixTime">The unix time.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTime(this int unixTime)
    {
        return UnixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime);
    }

    /// <summary>
    /// Froms the unix time.
    /// </summary>
    /// <param name="unixTime">The unix time.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTime(this double unixTime)
    {
        return UnixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime);
    }

    /// <summary>
    /// Froms the unix time.
    /// </summary>
    /// <param name="unixTime">The unix time.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTime(this long unixTime)
    {
        return UnixEpochDateTimeUtc + TimeSpan.FromSeconds(unixTime);
    }

    /// <summary>
    /// Converts to unixtimemsalt.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>System.Int64.</returns>
    public static long ToUnixTimeMsAlt(this DateTime dateTime)
    {
        return (dateTime.ToStableUniversalTime().Ticks - UnixEpoch) / TimeSpan.TicksPerMillisecond;
    }

    /// <summary>
    /// Converts to unixtimems.
    /// </summary>
    /// <param name="dateTimeOffset">The date time offset.</param>
    /// <returns>System.Int64.</returns>
    public static long ToUnixTimeMs(this DateTimeOffset dateTimeOffset) =>
        (long)ToDateTimeSinceUnixEpoch(dateTimeOffset.UtcDateTime).TotalMilliseconds;

    /// <summary>
    /// Converts to unixtimems.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>System.Int64.</returns>
    public static long ToUnixTimeMs(this DateTime dateTime)
    {
        var universal = ToDateTimeSinceUnixEpoch(dateTime);
        return (long)universal.TotalMilliseconds;
    }

    /// <summary>
    /// Converts to unixtime.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>System.Int64.</returns>
    public static long ToUnixTime(this DateTime dateTime)
    {
        return (dateTime.ToDateTimeSinceUnixEpoch().Ticks) / TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Converts to datetimesinceunixepoch.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>TimeSpan.</returns>
    private static TimeSpan ToDateTimeSinceUnixEpoch(this DateTime dateTime)
    {
        var dtUtc = dateTime;
        if (dateTime.Kind != DateTimeKind.Utc)
        {
            dtUtc = dateTime.Kind == DateTimeKind.Unspecified && dateTime > DateTime.MinValue && dateTime < DateTime.MaxValue
                        ? DateTime.SpecifyKind(dateTime.Subtract(DateTimeSerializer.LocalTimeZone.GetUtcOffset(dateTime)), DateTimeKind.Utc)
                        : dateTime.ToStableUniversalTime();
        }

        var universal = dtUtc.Subtract(UnixEpochDateTimeUtc);
        return universal;
    }

    /// <summary>
    /// Converts to unixtimems.
    /// </summary>
    /// <param name="ticks">The ticks.</param>
    /// <returns>System.Int64.</returns>
    public static long ToUnixTimeMs(this long ticks)
    {
        return (ticks - UnixEpoch) / TimeSpan.TicksPerMillisecond;
    }

#if NET7_0_OR_GREATER
    /// <summary>
    /// Converts to unixtimems.
    /// </summary>
    /// <param name="dateOnly">The date only.</param>
    /// <returns>System.Int64.</returns>
    public static long ToUnixTimeMs(this DateOnly dateOnly) => dateOnly.ToDateTime(default, DateTimeKind.Utc).ToUnixTimeMs();
    /// <summary>
    /// Converts to unixtime.
    /// </summary>
    /// <param name="dateOnly">The date only.</param>
    /// <returns>System.Int64.</returns>
    public static long ToUnixTime(this DateOnly dateOnly) => dateOnly.ToDateTime(default, DateTimeKind.Utc).ToUnixTime();
#endif

    /// <summary>
    /// Froms the unix time ms.
    /// </summary>
    /// <param name="msSince1970">The ms since1970.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTimeMs(this double msSince1970)
    {
        return UnixEpochDateTimeUtc + TimeSpan.FromMilliseconds(msSince1970);
    }

    /// <summary>
    /// Froms the unix time ms.
    /// </summary>
    /// <param name="msSince1970">The ms since1970.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTimeMs(this long msSince1970)
    {
        return UnixEpochDateTimeUtc + TimeSpan.FromMilliseconds(msSince1970);
    }

    /// <summary>
    /// Froms the unix time ms.
    /// </summary>
    /// <param name="msSince1970">The ms since1970.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTimeMs(this long msSince1970, TimeSpan offset)
    {
        return DateTime.SpecifyKind(UnixEpochDateTimeUnspecified + TimeSpan.FromMilliseconds(msSince1970) + offset, DateTimeKind.Local);
    }

    /// <summary>
    /// Froms the unix time ms.
    /// </summary>
    /// <param name="msSince1970">The ms since1970.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTimeMs(this double msSince1970, TimeSpan offset)
    {
        return DateTime.SpecifyKind(UnixEpochDateTimeUnspecified + TimeSpan.FromMilliseconds(msSince1970) + offset, DateTimeKind.Local);
    }

    /// <summary>
    /// Froms the unix time ms.
    /// </summary>
    /// <param name="msSince1970">The ms since1970.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTimeMs(string msSince1970)
    {
        return long.TryParse(msSince1970, out long ms) ? ms.FromUnixTimeMs() : double.Parse(msSince1970).FromUnixTimeMs();

        // Do we really need to support fractional unix time ms time strings??
    }

    /// <summary>
    /// Froms the unix time ms.
    /// </summary>
    /// <param name="msSince1970">The ms since1970.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromUnixTimeMs(string msSince1970, TimeSpan offset)
    {
        if (long.TryParse(msSince1970, out long ms)) return ms.FromUnixTimeMs(offset);

        // Do we really need to support fractional unix time ms time strings??
        return double.Parse(msSince1970).FromUnixTimeMs(offset);
    }

    /// <summary>
    /// Rounds to ms.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>DateTime.</returns>
    public static DateTime RoundToMs(this DateTime dateTime)
    {
        return new DateTime((dateTime.Ticks / TimeSpan.TicksPerMillisecond) * TimeSpan.TicksPerMillisecond, dateTime.Kind);
    }

    /// <summary>
    /// Rounds to second.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>DateTime.</returns>
    public static DateTime RoundToSecond(this DateTime dateTime)
    {
        return new DateTime((dateTime.Ticks / TimeSpan.TicksPerSecond) * TimeSpan.TicksPerSecond, dateTime.Kind);
    }

    /// <summary>
    /// Truncates the specified time span.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <param name="timeSpan">The time span.</param>
    /// <returns>DateTime.</returns>
    public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
    {
        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

    /// <summary>
    /// Converts to shortestxsddatetimestring.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>System.String.</returns>
    public static string ToShortestXsdDateTimeString(this DateTime dateTime)
    {
        return DateTimeSerializer.ToShortestXsdDateTimeString(dateTime);
    }

    /// <summary>
    /// Froms the shortest XSD date time string.
    /// </summary>
    /// <param name="xsdDateTime">The XSD date time.</param>
    /// <returns>DateTime.</returns>
    public static DateTime FromShortestXsdDateTimeString(this string xsdDateTime)
    {
        return DateTimeSerializer.ParseShortestXsdDateTime(xsdDateTime);
    }

    /// <summary>
    /// Determines whether [is equal to the second] [the specified other date time].
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <param name="otherDateTime">The other date time.</param>
    /// <returns><c>true</c> if [is equal to the second] [the specified other date time]; otherwise, <c>false</c>.</returns>
    public static bool IsEqualToTheSecond(this DateTime dateTime, DateTime otherDateTime)
    {
        return dateTime.ToStableUniversalTime().RoundToSecond().Equals(otherDateTime.ToStableUniversalTime().RoundToSecond());
    }

    /// <summary>
    /// Converts to timeoffsetstring.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <param name="seperator">The seperator.</param>
    /// <returns>System.String.</returns>
    public static string ToTimeOffsetString(this TimeSpan offset, string seperator = "")
    {
        var hours = Math.Abs(offset.Hours).ToString(CultureInfo.InvariantCulture);
        var minutes = Math.Abs(offset.Minutes).ToString(CultureInfo.InvariantCulture);
        return (offset < TimeSpan.Zero ? "-" : "+")
               + (hours.Length == 1 ? "0" + hours : hours)
               + seperator
               + (minutes.Length == 1 ? "0" + minutes : minutes);
    }

    /// <summary>
    /// Froms the time offset string.
    /// </summary>
    /// <param name="offsetString">The offset string.</param>
    /// <returns>TimeSpan.</returns>
    public static TimeSpan FromTimeOffsetString(this string offsetString)
    {
        if (!offsetString.Contains(":"))
            offsetString = offsetString.Insert(offsetString.Length - 2, ":");

        offsetString = offsetString.TrimStart('+');

        return TimeSpan.Parse(offsetString);
    }

    /// <summary>
    /// Converts to stableuniversaltime.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <returns>DateTime.</returns>
    public static DateTime ToStableUniversalTime(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime;
        if (dateTime == DateTime.MinValue)
            return MinDateTimeUtc;

        return PclExport.Instance.ToStableUniversalTime(dateTime);
    }

    /// <summary>
    /// FMTs the sortable date.
    /// </summary>
    /// <param name="from">From.</param>
    /// <returns>System.String.</returns>
    public static string FmtSortableDate(this DateTime from)
    {
        return from.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// FMTs the sortable date time.
    /// </summary>
    /// <param name="from">From.</param>
    /// <returns>System.String.</returns>
    public static string FmtSortableDateTime(this DateTime from)
    {
        return from.ToString("u");
    }

    /// <summary>
    /// Lasts the monday.
    /// </summary>
    /// <param name="from">From.</param>
    /// <returns>DateTime.</returns>
    public static DateTime LastMonday(this DateTime from)
    {
        var mondayOfWeek = from.Date.AddDays(-(int)from.DayOfWeek + 1);
        return mondayOfWeek;
    }

    /// <summary>
    /// Starts the of last month.
    /// </summary>
    /// <param name="from">From.</param>
    /// <returns>DateTime.</returns>
    public static DateTime StartOfLastMonth(this DateTime from)
    {
        return new DateTime(from.Date.Year, from.Date.Month, 1).AddMonths(-1);
    }

    /// <summary>
    /// Ends the of last month.
    /// </summary>
    /// <param name="from">From.</param>
    /// <returns>DateTime.</returns>
    public static DateTime EndOfLastMonth(this DateTime from)
    {
        return new DateTime(from.Date.Year, from.Date.Month, 1).AddDays(-1);
    }
}