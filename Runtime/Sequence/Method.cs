using System.Collections.Generic;

using UnityEngine;

namespace Prototype.SequenceFlow
{
    public abstract class Method
    {
        [System.Serializable]
        public class Parameter
        {
            public int index;
            public string name;
        }

        public string name;

        public List<Parameter> boolParameters;
        public List<Parameter> intParameters;
        public List<Parameter> floatParameters;
        public List<Parameter> stringParameters;
        public List<Parameter> objectParameters;

        public List<bool> bools;
        public List<int> ints;
        public List<float> floats;
        public List<string> strings;

        public List<int> enums;

        public List<Vector2> vector2s;
        public List<Vector3> vector3s;
        public List<Color> colors;

        public List<Object> objects;

        public List<UnityEngine.Events.UnityEvent> unityEvents;
        public List<AnimationCurve> animationCurves;

        public Method()
        {
            name = "";

            boolParameters = new List<Parameter>();
            intParameters = new List<Parameter>();
            floatParameters = new List<Parameter>();
            stringParameters = new List<Parameter>();
            objectParameters = new List<Parameter>();

            bools = new List<bool>();
            ints = new List<int>();
            floats = new List<float>();
            strings = new List<string>();

            enums = new List<int>();

            vector2s = new List<Vector2>();
            vector3s = new List<Vector3>();
            colors = new List<Color>();

            objects = new List<Object>();

            unityEvents = new List<UnityEngine.Events.UnityEvent>();
            animationCurves = new List<AnimationCurve>();
        }

        public Method(string name, params object[] parameters) : this()
        {
            this.name = name;

            foreach (var parameter in parameters)
            {
                if (parameter is bool @bool)
                    bools.Add(@bool);
                else if (parameter is int @int)
                    ints.Add(@int);
                else if (parameter is float @float)
                    floats.Add(@float);
                else if (parameter is string @string)
                    strings.Add(@string);

                else if (parameter.GetType().IsEnum)
                    enums.Add((int) parameter);

                else if (parameter is Vector2 vector2)
                    vector2s.Add(vector2);
                else if (parameter is Vector3 vector3)
                    vector3s.Add(vector3);
                else if (parameter is Color color)
                    colors.Add(color);

                else if (parameter is Object @object)
                    objects.Add(@object);

                else if (parameter is UnityEngine.Events.UnityEvent unityEvent)
                    unityEvents.Add(unityEvent);
                else if (parameter is AnimationCurve animationCurve)
                    animationCurves.Add(animationCurve);
            }
        }

        protected object[] GetParameters(IEnumerable<System.Type> types, SimpleData parameters)
        {
            var result = new List<object>();

            int boolsIndex = 0;
            int intsIndex = 0;
            int floatsIndex = 0;
            int stringsIndex = 0;

            int enumsIndex = 0;

            int vectors2Index = 0;
            int vectors3Index = 0;
            int colorsIndex = 0;

            int objectsIndex = 0;

            int unityEventsIndex = 0;
            int animationCurvesIndex = 0;

            foreach (var type in types)
            {
                if (type == typeof(bool))
                    AddBool(result, ref boolsIndex, parameters);
                else if (type == typeof(int))
                    AddInteger(result, ref intsIndex, parameters);
                else if (type == typeof(float))
                    AddFloat(result, ref floatsIndex, parameters);
                else if (type == typeof(string))
                    AddString(result, ref stringsIndex, parameters);

                else if (type.IsEnum)
                    result.Add(enums[enumsIndex++]);

                else if (type == typeof(Vector2))
                    result.Add(vector2s[vectors2Index++]);
                else if (type == typeof(Vector3))
                    result.Add(vector3s[vectors3Index++]);
                else if (type == typeof(Color))
                    result.Add(colors[colorsIndex++]);

                else if (typeof(Object).IsAssignableFrom(type))
                    AddObject(result, type, ref objectsIndex, parameters);

                else if (type == typeof(UnityEngine.Events.UnityEvent))
                    result.Add(unityEvents[unityEventsIndex++]);
                else if (type == typeof(AnimationCurve))
                    result.Add(animationCurves[animationCurvesIndex++]);
            }

            return result.ToArray();
        }

        void AddBool(List<object> result, ref int boolsIndex, SimpleData parameters)
        {
            if (parameters is not null)
            {
                foreach (var boolParameter in boolParameters)
                {
                    if (boolParameter.index == boolsIndex && parameters.TryGetBool(boolParameter.name, out var value))
                    {
                        boolsIndex++;
                        result.Add(value);
                        return;
                    }
                }
            }

            result.Add(bools[boolsIndex++]);
        }

        void AddInteger(List<object> result, ref int intsIndex, SimpleData parameters)
        {
            if (parameters is not null)
            {
                foreach (var intParameter in intParameters)
                {
                    if (intParameter.index == intsIndex && parameters.TryGetInteger(intParameter.name, out var value))
                    {
                        intsIndex++;
                        result.Add(value);
                        return;
                    }
                }
            }

            result.Add(ints[intsIndex++]);
        }

        void AddFloat(List<object> result, ref int floatsIndex, SimpleData parameters)
        {
            if (parameters is not null)
            {
                foreach (var floatParameter in floatParameters)
                {
                    if (floatParameter.index == floatsIndex && parameters.TryGetFloat(floatParameter.name, out var value))
                    {
                        floatsIndex++;
                        result.Add(value);
                        return;
                    }
                }
            }

            result.Add(floats[floatsIndex++]);
        }

        void AddString(List<object> result, ref int stringsIndex, SimpleData parameters)
        {
            if (parameters is not null)
            {
                foreach (var stringParameter in stringParameters)
                {
                    if (stringParameter.index == stringsIndex && parameters.TryGetString(stringParameter.name, out var value))
                    {
                        stringsIndex++;
                        result.Add(value);
                        return;
                    }
                }
            }
            result.Add(strings[stringsIndex++]);
        }

        void AddObject(List<object> result, System.Type type, ref int objectsIndex, SimpleData parameters)
        {
            if (parameters is not null)
            {
                foreach (var objectParameter in objectParameters)
                {
                    if (objectParameter.index == objectsIndex && parameters.TryGetObject(objectParameter.name, type, out var value))
                    {
                        objectsIndex++;
                        result.Add(value);
                        return;
                    }
                }
            }

            result.Add(objects[objectsIndex++]);
        }
    }
}
