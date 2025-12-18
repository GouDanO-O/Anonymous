using GDFrameworkExtend.PoolKit;

namespace GDFrameworkExtend.Data
{
    /// <summary>
    /// 临时游戏数据--池
    /// 仅当前存档中持续存在,会被玩家的行为影响而产生影响
    /// 需要实现Allocate
    /// </summary>
    public abstract class TemporaryDataPoolable : TemporaryData, IPoolable, IPoolType
    {
        public bool IsRecycled { get; set; }

        /// <summary>
        /// 注销(用来回收)
        /// </summary>
        public abstract void OnRecycled();

        /// <summary>
        /// 注销时执行
        /// </summary>
        public abstract void Recycle2Cache();
    }
}