namespace Core.Game.Item.Define
{
    public static class ItemConst
    {
        public const long InvalidItemId = 0;
        public const long ItemIdStart = 1_000_000;

        // 渲染层级 (Floor=5000, Wall=10000 之间)
        public const int ItemSortOrderBase = 7500;
    }
}
