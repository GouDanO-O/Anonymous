#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using Sirenix.Utilities.Editor;

#endif

    // Example demonstrating how use context objects in custom drawers.
    [InfoBox(
        "As of Odin 2.0, all drawers are now instanced per property. This means that the previous context system is now unnecessary as you can just make fields directly in the drawer.")]
    public class InstancedDrawerExample : MonoBehaviour
    {
        [InstancedDrawerExample] public int Field;
    }

    // The attribute used by the InstancedDrawerExampleAttributeDrawer.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InstancedDrawerExampleAttribute : Attribute
    {
    }

#if UNITY_EDITOR

    // Place the drawer script file in an Editor folder.
    public class InstancedDrawerExampleAttributeDrawer : OdinAttributeDrawer<InstancedDrawerExampleAttribute>
    {
        private int counter;
        private bool counterEnabled;

        // The new Initialize method is called when the drawer is first instanciated.
        protected override void Initialize()
        {
            counter = 0;
            counterEnabled = false;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Count the frames.
            if (Event.current.type == EventType.Layout && counterEnabled)
            {
                counter++;
                GUIHelper.RequestRepaint();
            }

            // Draw the current frame count, and a start stop button.
            SirenixEditorGUI.BeginBox();
            {
                GUILayout.Label("Frame Count: " + counter);

                if (GUILayout.Button(counterEnabled ? "Stop" : "Start")) counterEnabled = !counterEnabled;
            }
            SirenixEditorGUI.EndBox();

            // Continue the drawer chain.
            CallNextDrawer(label);
        }
    }

#endif
}
#endif