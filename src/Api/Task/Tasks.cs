using System;
using System.Collections.Generic;
using UnityEngine;

namespace Essentials.Api.Task
{
    public class Tasks : MonoBehaviour
    {
        private static readonly List<Task> Pool = new List<Task>();

        private Tasks() {}

        /// <summary>
        /// Instantiate a new task with given action
        /// </summary>
        /// <param name="action">New task</param>
        public static Task New( Action<Task> action )
        {
            return new Task( action );
        }

        /// <summary>
        /// Cancel all tasks
        /// </summary>
        public static void CancelAll()
        {
            Pool.Clear();
        }

        /// <summary>
        /// Internal method, called by mono
        /// </summary>
        internal void FixedUpdate()
        {
            for ( var i = 0; i < Pool.Count; i++ )
            {
                var task = Pool[i];

                if ( task.NextExecution > DateTime.Now ) continue;

                task.Action(task);

                if ( task.IntervalValue < 0 )
                    Pool.Remove(task);
                else
                    task.NextExecution = DateTime.Now.AddMilliseconds( task.IntervalValue );
            }
        }

        /// <summary>
        /// Represents an task that will be executed after one
        /// given delay, and with(or not) an interval
        /// </summary>
        public class Task
        {
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

            internal Task( Action<Task> action )
            {
                Action = action;
                IntervalValue = -1;
                DelayValue = 0;
            }

            /// <summary>
            /// Set the task interval to given value
            /// </summary>
            /// <param name="interval">The interval, in milliseconds</param>
            /// <returns>This</returns>
            public Task Interval( int interval )
            {
                IntervalValue = interval;
                return this;
            }

            /// <summary>
            /// Set the task delay to given value
            /// </summary>
            /// <param name="delay">The interval, in milliseconds</param>
            /// <returns>This</returns>
            public Task Delay( int delay )
            {
                DelayValue = delay;
                return this;
            }

            /// <summary>
            /// Start the task
            /// </summary>
            public void Go()
            {
                NextExecution = DateTime.Now.AddMilliseconds( DelayValue );

                Pool.Add( this );
            }

            /// <summary>
            /// Cancel the task
            /// </summary>
            public void Cancel()
            {
                Pool.Remove( this );
            }
        }
    }
}