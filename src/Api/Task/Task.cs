#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
#endregion

using System;

namespace Essentials.Api.Task {

    public sealed class Task {

        public string Id { get; internal set; }

        public int Delay { get; internal set; } = - 1;

        public int Interval { get; internal set; } = -1;

        public bool IsAlive { get; internal set; }

        public bool IsAsync { get; internal set; }

        public Action<Task> Action { get; internal set; }

        public DateTime NextExecution { get; internal set; }

        public static Builder Create() {
            return new Builder();
        }

        public void Cancel() {
            IsAlive = false;
        }

        public void Run() {
            Action(this);
        }

        public override string ToString() {
            return $"Id: \"{Id}\", " +
                   $"Delay: {Delay}, " +
                   $"Interval: {Interval}, " +
                   $"IsAlive: {IsAlive}, " +
                   $"IsAsync: {IsAsync}, " +
                   $"Action: {Action}, " +
                   $"NextExecution: {NextExecution}";
        }

        public class Builder {

            readonly Task _task = new Task();

            internal Builder() {}

            public Builder Id(string id) {
                _task.Id = id;
                return this;
            }

            public Builder Delay(int delayInMs) {
                _task.Delay = delayInMs;
                return this;
            }

            public Builder Delay(TimeSpan delay) {
                _task.Delay = (int) delay.TotalMilliseconds;
                return this;
            }

            public Builder Interval(int intervalInMs) {
                _task.Interval = intervalInMs;
                return this;
            }

            public Builder Interval(TimeSpan interval) {
                _task.Interval = (int) interval.TotalMilliseconds;
                return this;
            }

            public Builder Action(Action action) {
                _task.Action = (t => action());
                return this;
            }

            public Builder Action(Action<Task> action) {
                _task.Action = action;
                return this;
            }

            public Builder Async() {
                _task.IsAsync = true;
                return this;
            }

            // TODO: Better naming?
            public Builder UseIntervalAsDelay() {
                _task.Delay = _task.Interval;
                return this;
            }

            public Task Submit() {
                return Submit(UEssentials.TaskExecutor);
            }

            public Task Submit(ITaskExecutor executor) {
                if (_task.Action == null) {
                    throw new InvalidOperationException("Cannot submit a task with null action.");
                }
                _task.NextExecution = DateTime.Now.AddMilliseconds(_task.Delay);
                _task.IsAlive = true;
                executor.Enqueue(_task);
                return _task;
            }
        }
    }
}
