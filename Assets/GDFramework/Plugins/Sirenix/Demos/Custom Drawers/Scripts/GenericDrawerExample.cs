#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos
{
    using UnityEngine;
    using OdinInspector;
    using Serialization;
    using System.Collections.Generic;

#if UNITY_EDITOR
    using Editor;
    using Sirenix.Utilities.Editor;
    using UnityEditor;

#endif

    // Example demonstrating how drawers can be implemented with generic constraints.
    [TypeInfoBox(
        "This examples demonstates how a custom drawer can defined to be generic." +
        "\nThis allows a single drawer implementation, to deal with a wide array of values.")]
    public class GenericDrawerExample : SerializedMonoBehaviour
    {
        [OdinSerialize] public MyGenericClass<int, int> A = new(); // Drawn with struct drawer

        [OdinSerialize] public MyGenericClass<Vector3, Quaternion> B = new(); // Drawn with struct drawer

        [OdinSerialize]
        public MyGenericClass<int, GameObject> C = new(); // Drawn with generic parameter extraction drawer

        [OdinSerialize] public MyGenericClass<string, List<string>> D = new(); // Drawn with strong list drawer

        [OdinSerialize]
        public MyGenericClass<string, string>
            E = new(); // Drawn with default drawers, as none of the generic drawers beneath apply

        public List<MyClass> F = new(); // Drawn with the custom list drawer
    }

    // Generic class with any two generic types.
    public class MyGenericClass<T1, T2>
    {
        public T1 First;
        public T2 Second;
    }

#if UNITY_EDITOR

    public class MyGenericClassDrawer_Struct<T1, T2> : OdinValueDrawer<MyGenericClass<T1, T2>>
        where T1 : struct
        where T2 : struct
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            SirenixEditorGUI.DrawSolidRect(EditorGUILayout.GetControlRect(), Color.red);
            CallNextDrawer(label);
        }
    }

    public class MyGenericClassDrawer_StrongList<TElement, TList> : OdinValueDrawer<MyGenericClass<TElement, TList>>
        where TList : IList<TElement>
        where TElement : class
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            SirenixEditorGUI.DrawSolidRect(EditorGUILayout.GetControlRect(), Color.blue);
            CallNextDrawer(label);
        }
    }

    // Note how it is possible to give a generic parameter as the drawn type; Odin will look at the constraints on the parameter to determine where it applies
    public class MyGenericClassDrawer_GenericParameterExtraction<TValue, TUnityObject> : OdinValueDrawer<TValue>
        where TValue : MyGenericClass<int, TUnityObject>
        where TUnityObject : Object
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            SirenixEditorGUI.DrawSolidRect(EditorGUILayout.GetControlRect(), Color.green);
            CallNextDrawer(label);
        }
    }

    [DrawerPriority(0, 0, 2)]
    public class MyClassListDrawer<TList, TElement> : OdinValueDrawer<TList>
        where TList : IList<TElement>
        where TElement : MyClass
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            SirenixEditorGUI.DrawSolidRect(EditorGUILayout.GetControlRect(), new Color(1, 0.5f, 0));
            CallNextDrawer(label);
        }
    }

#endif
}
#endif