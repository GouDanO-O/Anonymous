namespace Core.Game.Pawn.Define
{
    public static class PawnConst
    {
        public const long InvalidPawnId = 0;

        // 移动
        public const float DefaultMoveSpeed = 2f; // 格/秒
        public const float DiagonalMultiplier = 1.414f;

        // 需求
        public const float MaxNeedValue = 100f;
        public const float MinNeedValue = 0f;
        public const float HungerDecayRate = 0.5f;
        public const float FatigueDecayRate = 0.3f;
        public const float MoodBaseDecayRate = 0.1f;
        public const float HungerCriticalThreshold = 20f;
        public const float FatigueCriticalThreshold = 15f;
        public const float MoodLowThreshold = 30f;

        // 漫游AI
        public const float WanderIntervalMin = 3f;
        public const float WanderIntervalMax = 8f;
        public const int WanderRadius = 15;
        public const int PathSearchRadius = 50;

        // 施工
        public const float ConstructionSpeed = 1.0f; // 每秒工作量
        public const float DemolishSpeed = 1.5f; // 拆除比建造快

        // 渲染 (Wall=10000, Roof=15000之间)
        public const int PawnSortOrderBase = 12000;
    }
}
