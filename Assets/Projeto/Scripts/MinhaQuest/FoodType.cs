namespace NexaQuest.MinhaQuest
{
    public enum FoodType { Apple, Sandwich, NexaKing }
    public enum FeedingResult { Consumed, Empty, FullEnergy, Invalid }
    public static class FoodCatalog
    {
        public static bool Valid(FoodType type) => type >= FoodType.Apple && type <= FoodType.NexaKing;
        public static int Price(FoodType type) => type == FoodType.Apple ? 5 : type == FoodType.Sandwich ? 15 : 40;
        public static int Energy(FoodType type) => type == FoodType.Apple ? 10 : type == FoodType.Sandwich ? 25 : 50;
    }
}
