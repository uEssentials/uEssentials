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
using System.Diagnostics;
using Essentials.Core;
using UnityEngine;

namespace Essentials.Api.Task {

    internal class DefaultTaskExecutor : MonoBehaviour, ITaskExecutor {

        private readonly Queue<Task> _queue = new Queue<Task>(); 

        private void FixedUpdate() {
            lock (_queue) {
                if (_queue.Count == 0) {
                    return;
                }

#if DEBUG_TASK_EXECUTOR
                var sw = System.Diagnostics.Stopwatch.StartNew();
#endif
                var dequeued = 0;

                start:
                var task = _queue.Dequeue();
                dequeued++;

                if (!task.IsAlive) {
                    goto end;
                }

                if (task.NextExecution > DateTime.Now) {
                    _queue.Enqueue(task);
                } else {
                    try {
                        var shouldDebugTask = (EssCore.DebugFlags & EssCore.kDebugTasks) != 0;
                        var sw2 = shouldDebugTask ? Stopwatch.StartNew() : null;

                        // Execute task action
                        task.Action.Invoke(task);

                        if (shouldDebugTask) {
                            sw2.Stop();
                            UEssentials.Logger.LogDebug("Executed task {");
                            UEssentials.Logger.LogDebug($"  Id: '{task.Id ?? "unknown"}'");
                            UEssentials.Logger.LogDebug($"  IsAlive: '{task.IsAlive}'");
                            UEssentials.Logger.LogDebug($"  Delay: '{task.Delay} ms'");
                            UEssentials.Logger.LogDebug($"  Interval: '{(task.Interval == -1 ? "-1" : task.Interval + "ms")}'");
                            UEssentials.Logger.LogDebug($"  NextExecution: '{task.NextExecution}'");
                            UEssentials.Logger.LogDebug($"  Took: '{sw2.ElapsedTicks} ticks | {sw2.ElapsedMilliseconds} ms'");
                            UEssentials.Logger.LogDebug("}");
                        }

                        // The task can be cancelled when executed...
                        if (!task.IsAlive) {
                            goto end;
                        }
                    } catch (Exception ex) {
                        UEssentials.Logger.LogError($"An error ocurred while executing task '{task.Id ?? "unknown_id"}'");
                        UEssentials.Logger.LogError(ex.ToString());
                        goto end;
                    }

                    if (task.Interval > 0) {
                        task.NextExecution = DateTime.Now.AddMilliseconds(task.Interval);
                        _queue.Enqueue(task);
                    }
                }

                end:
                if (dequeued < _queue.Count) {
                    goto start;
                }
#if DEBUG_TASK_EXECUTOR
                sw.Stop();

                System.Diagnostics.Debug.WriteLine($"Elapsed {sw.ElapsedTicks} ticks | " +
                                                   $"{sw.ElapsedMilliseconds} ms", "TASK_EXECUTOR");
#endif
            }
        }

        public void Enqueue(Task task) {
            lock (_queue) {
                _queue.Enqueue(task);
            }
        }

        public void DequeueAll() {
            lock (_queue) {
                _queue.Clear();
            }
        }

    }

}
