#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  leonardosnt
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
using System.Collections.Generic;
using System.Diagnostics;
using Essentials.Core;

namespace Essentials.Api.Task {

    public abstract class AbstractTaskExecutor : ITaskExecutor {

        protected readonly Queue<Task> Queue = new Queue<Task>();

        protected void Update() {
            lock (Queue) {
                if (Queue.Count == 0) {
                    return;
                }

                var dequeued = 0;

                start:
                var task = Queue.Dequeue();
                dequeued++;

                if (!task.IsAlive) {
                    goto end;
                }

                if (task.NextExecution > DateTime.Now) {
                    Queue.Enqueue(task);
                } else {
                    try {
                        var shouldDebugTask = (EssCore.DebugFlags & EssCore.kDebugTasks) != 0;
                        var sw2 = shouldDebugTask ? Stopwatch.StartNew() : null;

                        // Execute task
                        task.Run();

                        if (shouldDebugTask) {
                            sw2.Stop();
                            UEssentials.Logger.LogDebug("Executed task {");
                            UEssentials.Logger.LogDebug($"  Id: '{task.Id ?? "unknown"}'");
                            UEssentials.Logger.LogDebug($"  IsAlive: '{task.IsAlive}'");
                            UEssentials.Logger.LogDebug($"  IsAsync: '{task.IsAsync}'");
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
                        Queue.Enqueue(task);
                    }
                }

                end:
                if (dequeued < Queue.Count) {
                    goto start;
                }
            }
        }

        
        public virtual void Enqueue(Task task) {
            lock (Queue) {
                Queue.Enqueue(task);
            }
        }

        public virtual void DequeueAll() {
            lock (Queue) {
                Queue.Clear();
            }
        }

        public virtual void Stop() {
            DequeueAll();
        }
    }

}