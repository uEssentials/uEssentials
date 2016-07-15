/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  Leonardosc
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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Essentials.Api.Task {

    public static class Tasks {

        private static readonly List<Task> Pool = new List<Task>();

        /// <summary>
        /// Instantiate a new task with given action
        /// </summary>
        /// <param name="action">New task</param>
        public static Task New(Action<Task> action) {
            return new Task(action);
        }

        /// <summary>
        /// Cancel all tasks
        /// </summary>
        public static void CancelAll() {
            Pool.Clear();
        }

        /// <summary>
        /// Represents an task that will be executed after
        /// given delay, and with(or not) an interval
        /// </summary>
        public class Task {

            /// <summary>
            /// Time who will be executed
            /// </summary>
            internal DateTime NextExecution;

            /// <summary>
            /// Action who will be executed
            /// </summary>
            internal Action<Task> Action;

            /// <summary>
            /// Interval between executions
            /// </summary>
            internal int IntervalValue;

            /// <summary>
            /// Delay of execution
            /// </summary>
            internal int DelayValue;

            internal Task(Action<Task> action) {
                Action = action;
                IntervalValue = -1;
                DelayValue = 0;
            }

            /// <summary>
            /// Set the task interval.
            /// </summary>
            /// <param name="interval">The interval, in milliseconds</param>
            /// <returns>This</returns>
            public Task Interval(int interval) {
                IntervalValue = interval;
                return this;
            }

            /// <summary>
            /// Set the task delay.
            /// </summary>
            /// <param name="delay">The interval, in milliseconds</param>
            /// <returns>This</returns>
            public Task Delay(int delay) {
                DelayValue = delay;
                return this;
            }

            /// <summary>
            /// Start the task.
            /// </summary>
            public void Go() {
                NextExecution = DateTime.Now.AddMilliseconds(DelayValue);

                Pool.Add(this);
            }

            /// <summary>
            /// Cancel the task.
            /// </summary>
            public void Cancel() {
                Pool.Remove(this);
            }

        }

        internal class TaskExecutor : MonoBehaviour {

            private void FixedUpdate() {
                for (var i = 0; i < Pool.Count; i++) {
                    var task = Pool[i];

                    if (task.NextExecution > DateTime.Now) continue;

                    /*
                        If any error occurs he will remove task from Pool.
                    */
                    try {
                        #if DEBUG_PERF
                            var sw = System.Diagnostics.Stopwatch.StartNew();
                        #endif

                        task.Action(task);

                        if (task.IntervalValue < 0) {
                            Pool.Remove(task);
                        } else {
                            task.NextExecution = DateTime.Now.AddMilliseconds(task.IntervalValue);
                        }

                        #if DEBUG_PERF
                            sw.Stop();
                            UEssentials.Logger.LogDebug("Executed task {");
                            UEssentials.Logger.LogDebug($"  Target: '{task.Action.Target.GetType()}'");
                            UEssentials.Logger.LogDebug($"  Method: '{task.Action.Method.Name}'");
                            UEssentials.Logger.LogDebug($"  Delay: '{task.DelayValue} ms'");
                            UEssentials.Logger.LogDebug($"  Interval: '{(task.IntervalValue == -1 ? "-1" : task.IntervalValue + "ms")}'");
                            UEssentials.Logger.LogDebug($"  NextExecution: '{task.NextExecution}'");
                            UEssentials.Logger.LogDebug($"  Took: '{sw.ElapsedTicks} ticks | {sw.ElapsedMilliseconds} ms'");
                            UEssentials.Logger.LogDebug("}");
                        #endif
                    } catch (Exception) {
                        Pool.Remove(task);
                        throw;
                    }
                }
            }

        }

    }
}