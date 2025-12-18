/****************************************************************************
 * Copyright (c) 2015 - 2022 liangxiegame UNDER MIT License
 * 
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 *
 * 感谢 于大进 提供反馈
 ****************************************************************************/

using System.Collections.Generic;

namespace GDFrameworkExtend.PoolKit
{
    /// <summary>
    /// 存储 List 对象池,用于优化减少 new 调用次数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ListPool<T>
    {
        /// <summary>
        /// 栈对象：存储多个List
        /// </summary>
        static Stack<List<T>> mListStack = new Stack<List<T>>(8);

        /// <summary>
        /// 出栈：获取某个List对象
        /// </summary>
        /// <returns></returns>
        public static List<T> Get()
        {
            if (mListStack.Count == 0)
            {
                return new List<T>(8);
            }

            return mListStack.Pop();
        }

        /// <summary>
        /// 入栈：将List对象添加到栈中
        /// </summary>
        /// <param name="toRelease"></param>
        public static void Release(List<T> toRelease)
        {
            if (mListStack.Contains(toRelease))
            {
                throw new System.InvalidOperationException ("重复回收 List，The List is released even though it is in the pool");
            }

            toRelease.Clear();
            mListStack.Push(toRelease);
        }
    }

    public static class ListPoolExtensions

    {
        public static void Release2Pool<T>(this List<T> self)
        {
            ListPool<T>.Release(self);
        }
    }
}