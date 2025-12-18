/****************************************************************************
 * Copyright (c) 2015 ~ 2022 liangxiegame UNDER MIT LICENSE
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

#if UNITY_EDITOR

using System.Linq;
using System.Text;
using GDFrameworkExtend.FluentAPI;
using UnityEngine;

namespace GDFrameworkExtend.CodeGenKit
{
    public class BindSearchHelper
    {
        public static void Search(CodeGenTask task)
        {
            var bindGroupTransforms = task.GameObject.GetComponentsInChildren<IBindGroup>(true)
                .Select(g => g.As<Component>().transform)
                .Where(t => t != task.GameObject.transform);

            var binds = task.GameObject.GetComponentsInChildren<IBindOld>(true)
                .Where(b => b.Transform != task.GameObject.transform);


            foreach (var bind in binds)
            {
                if (bindGroupTransforms.Any(g => bind.Transform.IsChildOf(g) && bind.Transform != g))
                {
                }
                else
                {
                    task.BindInfos.Add(new BindInfo()
                    {
                        TypeName = bind.TypeName,
                        MemberName = bind.Transform.gameObject.name,
                        BindScript = bind,
                        PathToRoot = PathToParent(bind.Transform, task.GameObject.name),
                    });
                }
            }
        }

        public static string PathToParent(Transform trans, string parentName)
        {
            var retValue = new StringBuilder(trans.name);

            while (trans.parent != null)
            {
                if (trans.parent.name.Equals(parentName))
                {
                    break;
                }

                retValue.AddPrefix("/").AddPrefix(trans.parent.name);

                trans = trans.parent;
            }

            return retValue.ToString();
        }
    }
}
#endif