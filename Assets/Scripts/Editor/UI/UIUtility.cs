using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.UI
{
    public static class UIUtility
    {
        public static VisualElement GetField(object obj, FieldInfo fieldInfo)
        {
            VisualElement element;
            var val = fieldInfo.GetValue(obj);
            switch (val)
            {
                case int value:
                    element = GetField(obj, fieldInfo, value);
                    break;
                case string value:
                    element = GetField(obj, fieldInfo, value);
                    break;
                case float value:
                    element = GetField(obj, fieldInfo, value);
                    break;
                case double value:
                    element = GetField(obj, fieldInfo, value);
                    break;
                case long value:
                    element = GetField(obj, fieldInfo, value);
                    break;
                case bool value:
                    element = GetField(obj, fieldInfo, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return element;
        }

        public static VisualElement GetField(object obj, FieldInfo fieldInfo, int value)
        {
            var field = new IntegerField(fieldInfo.Name)
            {
                value = value
            };
            return field.RegisterCallback<int>(fieldInfo, obj);
        }

        public static VisualElement GetField(object obj, FieldInfo fieldInfo, string value)
        {

            var field = new TextField(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback<string>(fieldInfo, obj);
        }

        public static VisualElement GetField(object obj, FieldInfo fieldInfo, float value)
        {
            var field = new FloatField(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback<float>(fieldInfo, obj);
        }

        public static VisualElement GetField(object obj, FieldInfo fieldInfo, double value)
        {
            var field = new DoubleField(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback<double>(fieldInfo, obj);
        }

        public static VisualElement GetField(object obj, FieldInfo fieldInfo, long value)
        {
            var field = new LongField(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback<long>(fieldInfo, obj);
        }

        public static VisualElement GetField(object obj, FieldInfo fieldInfo, bool value)
        {
            var field = new Toggle(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback<bool>(fieldInfo, obj);
        }

        private static VisualElement RegisterCallback<T>(this VisualElement field, FieldInfo fieldInfo, object obj)
        {
            field.RegisterCallback<ChangeEvent<T>, (FieldInfo FieldInfo, object Object)>(
                GenericCallback<T>(),
                (fieldInfo, obj)
            );
            return field;
        }

        private static EventCallback<ChangeEvent<T>, (FieldInfo FieldInfo, object Object)> GenericCallback<T>()
        {
            return (evt, input) => { input.FieldInfo.SetValue(input.Object, evt.newValue); };
        }
    }
}
