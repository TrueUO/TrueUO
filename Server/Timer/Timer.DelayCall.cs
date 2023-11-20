/*************************************************************************
 * ModernUO                                                              *
 * Copyright 2019-2023 - ModernUO Development Team                       *
 * Email: hi@modernuo.com                                                *
 * File: Timer.DelayCall.cs                                              *
 *                                                                       *
 * This program is free software: you can redistribute it and/or modify  *
 * it under the terms of the GNU General Public License as published by  *
 * the Free Software Foundation, either version 3 of the License, or     *
 * (at your option) any later version.                                   *
 *                                                                       *
 * You should have received a copy of the GNU General Public License     *
 * along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *************************************************************************/

using System;
using System.Runtime.CompilerServices;

namespace Server;

public partial class Timer
{
    private static string FormatDelegate(Delegate callback) =>
        callback == null ? "null" : $"{callback.Method.DeclaringType?.FullName ?? ""}.{callback.Method.Name}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DelayCallTimer DelayCall(Action callback) => DelayCall(TimeSpan.Zero, TimeSpan.Zero, 1, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DelayCallTimer DelayCall(TimeSpan delay, Action callback) => DelayCall(delay, TimeSpan.Zero, 1, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DelayCallTimer DelayCall(TimeSpan delay, TimeSpan interval, Action callback) =>
        DelayCall(delay, interval, 0, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DelayCallTimer DelayCall(TimeSpan interval, int count, Action callback) =>
        DelayCall(TimeSpan.Zero, interval, count, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DelayCallTimer DelayCall(TimeSpan delay, TimeSpan interval, int count, Action callback)
    {
        var t = new DelayCallTimer(delay, interval, count, callback);
        t.Start();

        return t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartTimer(Action callback) => StartTimer(TimeSpan.Zero, TimeSpan.Zero, 1, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartTimer(TimeSpan delay, Action callback) => StartTimer(delay, TimeSpan.Zero, 1, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartTimer(TimeSpan delay, TimeSpan interval, Action callback) =>
        StartTimer(delay, interval, 0, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartTimer(TimeSpan interval, int count, Action callback) =>
        StartTimer(TimeSpan.Zero, interval, count, callback);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void StartTimer(TimeSpan delay, TimeSpan interval, int count, Action callback)
    {
        var t = DelayCallTimer.GetTimer(delay, interval, count, callback);
        t.Start();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DelayCallTimer Pause(TimeSpan ms) => new(ms);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DelayCallTimer Pause(int ms) => Pause(TimeSpan.FromMilliseconds(ms));

    public sealed class DelayCallTimer : Timer, INotifyCompletion
    {
        private Action _continuation;
        private bool _complete;

        internal DelayCallTimer(TimeSpan delay, TimeSpan interval, int count, Action callback) : base(
            delay,
            interval,
            count
        ) => _continuation = callback;

        internal DelayCallTimer(TimeSpan delay) : base(delay)
        {
            Start();
        }

        protected override void OnTick()
        {
            _complete = true;
            _continuation?.Invoke();
        }

        internal override void OnDetach()
        {
            base.OnDetach();
        }

        public static DelayCallTimer GetTimer(TimeSpan delay, TimeSpan interval, int count, Action callback) =>
            new(delay, interval, count, callback);

        public override string ToString() => $"DelayCallTimer[{FormatDelegate(_continuation)}]";

        public DelayCallTimer GetAwaiter() => this;

        public bool IsCompleted => _complete;

        public void OnCompleted(Action continuation) => _continuation = continuation;

        public void GetResult()
        {
        }
    }
}
