using System;
using System.Collections;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    public abstract class AsyncSMD : SequenceMethodDefinition
    {
        public sealed override bool waitable => true;

        public IEnumerator Invoke(Sequence sequence, object[] parameters)
        {
            Prepare(sequence, parameters);
            return Execute();
        }

        public abstract IEnumerator Execute();

        protected static IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        protected static IEnumerator ChangePosition(Transform transform, Mode mode, Vector3 value, float duration, Func<float, float> t)
        {
            var a = transform.localPosition;
            var b = value;

            if (mode == Mode.Relative)
                b += a;

            var time = Time.time;

            while (Time.time - time < duration)
            {
                transform.localPosition = Vector3.Lerp(a, b, t(1 / duration * (Time.time - time)));
                yield return null;
            }

            transform.localPosition = b;
        }

        protected static IEnumerator ChangeSingleAngle(Transform transform, Mode mode, Axis axis, float value, float duration, Func<float, float> t)
        {
            var a = transform.localEulerAngles;

            Vector3 b;

            if (mode == Mode.Absolute)
                b = new(axis == Axis.X ? value : a.x, axis == Axis.Y ? value : a.y, axis == Axis.Z ? value : a.z);
            else
                b = a + new Vector3(axis == Axis.X ? value : 0, axis == Axis.Y ? value : 0, axis == Axis.Z ? value : 0);

            var time = Time.time;

            while (Time.time - time < duration)
            {
                transform.localEulerAngles = Vector3.Lerp(a, b, t(1 / duration * (Time.time - time)));
                yield return null;
            }

            transform.localEulerAngles = b;
        }

        protected static IEnumerator ChangeScale(Transform transform, Mode mode, Vector3 value, float duration, Func<float, float> t)
        {
            var a = transform.localScale;
            var b = value;

            if (mode == Mode.Relative)
                b += a;

            var time = Time.time;

            while (Time.time - time < duration)
            {
                transform.localScale = Vector3.Lerp(a, b, t(1 / duration * (Time.time - time)));
                yield return null;
            }

            transform.localScale = b;
        }
    }
}
