using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    [Serializable]
    public class Sequence
    {
        public SequenceCommand[] commands;

        public Transform activator { get; private set; }
        public MonoBehaviour owner { get; private set; }
        public Transform executer { get; private set; }
        public EventArgs e { get; private set; }
        public SimpleData parameters { get; private set; }

        public class Data
        {
            readonly Dictionary<object, object> values = new();

            public void Reset() => values.Clear();

            public T Get<T>(object key)
            {
                return values.TryGetValue(key, out var value)
                    ? (T) value
                    : default;
            }

            public T Set<T>(object key, T value)
            {
                if (!values.TryAdd(key, value))
                    values[key] = value;
                return value;
            }

            public T Set<T>(object key, Func<T, T> value)
            {
                return Set(key, value(Get<T>(key)));
            }
        }
        public readonly Data data = new();

        public bool repeat { get; private set; }

        Coroutine coroutine;
        readonly List<Coroutine> nestedCoroutines = new();

        public bool isRunning => coroutine is not null;

        public Sequence()
        {

        }

        public Sequence(params SequenceCommand[] commands)
        {
            this.commands = commands;
        }

        public Sequence(string methodName, float delay, params object[] parameters) : this()
        {
            commands = new[] { new SequenceCommand(methodName, delay, parameters) };
        }

        public void Start(Transform activator, MonoBehaviour owner, EventArgs e, SimpleData parameters)
        {
            if (isRunning)
            {
                Debug.Log("Sequence is already running", owner);
                return;
            }

            if (commands.Length == 0)
                return;

            this.activator = activator;
            this.owner = owner;
            this.e = e;
            this.parameters = parameters;

            coroutine = owner.StartCoroutine(StartCommands());
        }

        public void Exit()
        {
            foreach (var command in commands)
                command.method.OnExitSequence();
        }

        IEnumerator StartCommands()
        {
            do
            {
                data.Reset();
                repeat = false;

                foreach (var command in commands)
                {
                    if (!command.enabled)
                        continue;

                    if (command.delay > 0)
                        yield return new WaitForSeconds(command.delay);

                    UpdateExecuter(command);

                    var routine = command.method.Invoke(this, parameters);

                    if (routine is not null)
                    {
                        var coroutine = owner.StartCoroutine(routine);

                        if (command.flow != Flow.AsyncForever)
                            nestedCoroutines.Add(coroutine);

                        if (command.flow == Flow.Sync)
                        {
                            foreach (var nestedCoroutine in nestedCoroutines)
                                yield return nestedCoroutine;

                            nestedCoroutines.Clear();
                        }
                    }

                    if (command.postDelay > 0)
                        yield return new WaitForSeconds(command.postDelay);

                    if (repeat)
                        break;
                }

                foreach (var nestedCoroutine in nestedCoroutines)
                    yield return nestedCoroutine;

                nestedCoroutines.Clear();

            } while (repeat);

            coroutine = null;
        }

        void UpdateExecuter(Command command)
        {
            executer = command.executer == Command.Executer.Activator
                ? activator
                : owner.transform;
        }

        public void Abort()
        {
            foreach (var nestedCoroutine in nestedCoroutines)
                owner.StopCoroutine(nestedCoroutine);

            nestedCoroutines.Clear();

            if (isRunning)
            {
                owner.StopCoroutine(coroutine);
                coroutine = null;
            }
        }
    }
}
