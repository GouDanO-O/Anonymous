#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Demos.RPGEditor
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Serialization;

    //
    // This class is used by the RPGEditorWindow to render an overview of all characters using the TableList attribute.
    // All characters are Unity objects though, so they are rendered in the inspector as single Unity object field,
    // which is not exactly what we want in our table. We want to show the members of the unity object.
    //
    // So in order to render the members of the Unity object, we'll create a class that wraps the Unity object
    // and displays the relevant members through properties, which works with the TableList, attribute.
    //
    //RPGEditorWindow使用这个类来通过tableelist属性渲染所有字符的概览。
    //所有的字符都是Unity对象，所以它们在inspector中被渲染为单一的Unity对象字段
    //这并不是我们想要的。我们想要显示unity对象的成员
    //所以为了渲染Unity对象的成员，我们将创建一个包装Unity对象的类
    //并通过属性显示相关成员，这与TableList属性一起工作
    public class CharacterTable
    {
        [FormerlySerializedAs("allCharecters")] [TableList(IsReadOnly = true, AlwaysExpanded = true)] [ShowInInspector]
        private readonly List<CharacterWrapper> allCharacters;

        public Character this[int index] => allCharacters[index].Character;

        public CharacterTable(IEnumerable<Character> characters)
        {
            allCharacters = characters.Select(x => new CharacterWrapper(x)).ToList();
        }

        private class CharacterWrapper
        {
            private Character character; //Character是一个ScriptableObject，将渲染一个unity对象
            // Character is a ScriptableObject and would render a unity object
            //字段，这不是我们想要的。
            // field if drawn in the inspector, which is not what we want.

            public Character Character => character;

            public CharacterWrapper(Character character)
            {
                this.character = character;
            }

            [TableColumnWidth(50, false)]
            [ShowInInspector]
            [PreviewField(45, ObjectFieldAlignment.Center)]
            public Texture Icon
            {
                get => character.Icon;
                set
                {
                    character.Icon = value;
                    EditorUtility.SetDirty(character);
                }
            }

            [TableColumnWidth(120)]
            [ShowInInspector]
            public string Name
            {
                get => character.Name;
                set
                {
                    character.Name = value;
                    EditorUtility.SetDirty(character);
                }
            }

            [ShowInInspector]
            [ProgressBar(0, 100)]
            public float Shooting
            {
                get => character.Skills.Shooting;
                set
                {
                    character.Skills.Shooting = value;
                    EditorUtility.SetDirty(character);
                }
            }

            [ShowInInspector]
            [ProgressBar(0, 100)]
            public float Melee
            {
                get => character.Skills.Melee;
                set
                {
                    character.Skills.Melee = value;
                    EditorUtility.SetDirty(character);
                }
            }

            [ShowInInspector]
            [ProgressBar(0, 100)]
            public float Social
            {
                get => character.Skills.Social;
                set
                {
                    character.Skills.Social = value;
                    EditorUtility.SetDirty(character);
                }
            }

            [ShowInInspector]
            [ProgressBar(0, 100)]
            public float Animals
            {
                get => character.Skills.Animals;
                set
                {
                    character.Skills.Animals = value;
                    EditorUtility.SetDirty(character);
                }
            }

            [ShowInInspector]
            [ProgressBar(0, 100)]
            public float Medicine
            {
                get => character.Skills.Medicine;
                set
                {
                    character.Skills.Medicine = value;
                    EditorUtility.SetDirty(character);
                }
            }

            [ShowInInspector]
            [ProgressBar(0, 100)]
            public float Crafting
            {
                get => character.Skills.Crafting;
                set
                {
                    character.Skills.Crafting = value;
                    EditorUtility.SetDirty(character);
                }
            }
        }
    }
}
#endif