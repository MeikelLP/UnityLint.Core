using System;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Editor.UI
{
    public static class UIUtility
    {
        public static VisualElement GetField(object parentObject, FieldInfo fieldInfo)
        {
            var fieldValue = fieldInfo.GetValue(parentObject);
            if (fieldValue == default && !fieldInfo.FieldType.IsValueType)
            {
                fieldValue = Activator.CreateInstance(fieldInfo.FieldType);
            }

            var element = GetField(parentObject, fieldInfo, fieldValue);

            // ReSharper disable once UseIsOperator.2
            if(element.GetType().IsDerivedFromOpenGenericType(typeof(BaseField<>)))
            {
                element.AddToClassList("row");
                foreach (var child in element.Children())
                {
                    child.AddToClassList("col");
                }
            }

            return element;
        }

        private static VisualElement GetField(object parentObject, FieldInfo fieldInfo, object fieldValue)
        {
            VisualElement element;
            switch (fieldValue)
            {
                case int value:
                    element = GetField(parentObject, fieldInfo, value);
                    break;
                case string value:
                    element = GetField(parentObject, fieldInfo, value);
                    break;
                case float value:
                    element = GetField(parentObject, fieldInfo, value);
                    break;
                case double value:
                    element = GetField(parentObject, fieldInfo, value);
                    break;
                case long value:
                    element = GetField(parentObject, fieldInfo, value);
                    break;
                case bool value:
                    element = GetField(parentObject, fieldInfo, value);
                    break;
                case Enum value:
                    element = GetField(parentObject, fieldInfo, value);
                    break;
                default:
                    element = fieldInfo.FieldType.GetFields().Length > 0 ? GetField(fieldInfo, fieldValue) : null;

                    break;
            }

            return element;
        }

        private static VisualElement GetField(FieldInfo fieldInfo, object parentObject)
        {
            if (parentObject == null) return null;
            var fields = fieldInfo.FieldType.GetFields();

            var foldout = new Foldout
            {
                text = fieldInfo.Name
            };

            foreach (var field in fields)
            {
                var elem = GetField(parentObject, field);
                elem.AddToClassList("row");
                foldout.Add(elem);
            }

            return foldout;
        }

        private static BaseField<Enum> GetField(object parentObject, FieldInfo fieldInfo, Enum value)
        {
            var field = new EnumField(fieldInfo.Name, value)
            {
                value = value
            };
            return field.RegisterCallback(fieldInfo, parentObject);
        }

        public static BaseField<int> GetField(object parentObject, FieldInfo fieldInfo, int value)
        {
            var field = new IntegerField(fieldInfo.Name)
            {
                value = value
            };
            return field.RegisterCallback(fieldInfo, parentObject);
        }

        public static BaseField<string> GetField(object parentObject, FieldInfo fieldInfo, string value)
        {
            var field = new TextField(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback(fieldInfo, parentObject);
        }

        public static BaseField<float> GetField(object parentObject, FieldInfo fieldInfo, float value)
        {
            var field = new FloatField(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback(fieldInfo, parentObject);
        }

        public static BaseField<double> GetField(object parentObject, FieldInfo fieldInfo, double value)
        {
            var field = new DoubleField(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback<double>(fieldInfo, parentObject);
        }

        public static BaseField<long> GetField(object parentObject, FieldInfo fieldInfo, long value)
        {
            var field = new LongField(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback(fieldInfo, parentObject);
        }

        public static BaseField<bool> GetField(object parentObject, FieldInfo fieldInfo, bool value)
        {
            var field = new Toggle(fieldInfo.Name)
            {
                value = value
            };

            return field.RegisterCallback(fieldInfo, parentObject);
        }

        private static BaseField<T> RegisterCallback<T>(this BaseField<T> field, FieldInfo fieldInfo, object obj)
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

        public static bool IsDerivedFromOpenGenericType(this Type current, Type target)
        {
            var type = current;
            while ((type = type.BaseType) != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == target)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
