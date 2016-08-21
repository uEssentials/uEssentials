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

using System;
using System.Diagnostics;
using Essentials.Core;

namespace Essentials.Api.Task {

    internal class EssentialsTaskExecutor : ITaskExecutor {

        private readonly ITaskExecutor _syncExecutor;

        internal EssentialsTaskExecutor() {
            _syncExecutor = EssCore.Instance.TryAddComponent<SyncTaskExecutor>();
        }

        internal void Stop() {
            if (_syncExecutor != null)
                EssCore.Instance.TryRemoveComponent<SyncTaskExecutor>();
            DequeueAll();
        }

        public void Enqueue(Task task) {
            Debug.Assert(_syncExecutor != null, "_syncExecutor != null");

            if (task.IsAsync) {
                throw new NotImplementedException();
            } else {
                _syncExecutor.Enqueue(task);
            }
        }

        public void DequeueAll() {
            _syncExecutor?.DequeueAll();
            //_asyncExecutor.DequeueAll();
        }

    }

}
