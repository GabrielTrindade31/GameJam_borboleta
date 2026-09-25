using UnityEngine;

namespace ButterflyStep.EditorTools
{
    public static partial class GameBuilder
    {
        private static TemporalCondition Spring => TemporalCondition.In(Season.Primavera);
        private static TemporalCondition Summer => TemporalCondition.In(Season.Verao);
        private static TemporalCondition Autumn => TemporalCondition.In(Season.Outono);
        private static TemporalCondition Winter => TemporalCondition.In(Season.Inverno);

        private static void PlaceExtras(int level)
        {
            switch (level)
            {
                case 1:
                    CheckpointAt(16.5f, 0f);
                    CheckpointAt(22f, 7f);
                    Pollen(0, -2.3f, 2.2f);
                    Pollen(1, 12.5f, 7.1f);
                    Pollen(2, 28.8f, 12.1f, Autumn);
                    Pollen(3, 40f, 13.2f);
                    Pollen(4, 58f, 12.8f, Summer);
                    break;
                case 2:
                    CheckpointAt(12.5f, 0f);
                    CheckpointAt(32f, 0f);
                    Pollen(0, -11.5f, 5.7f);
                    Pollen(1, 25f, 0.6f, Winter);
                    Pollen(2, 1f, 3.3f, Spring);
                    Pollen(3, 34f, 0.9f, Spring);
                    Pollen(4, 44.5f, 0.9f);
                    break;
                case 3:
                    CheckpointAt(4.6f, 7f);
                    CheckpointAt(25.5f, 0f);
                    Pollen(0, -5.5f, 2.4f);
                    Pollen(1, 3f, 6.4f);
                    Pollen(2, 14.5f, 7.8f, Spring);
                    Pollen(3, 22f, 0.9f);
                    Pollen(4, 38.6f, 10.8f, Autumn);
                    break;
                case 4:
                    CheckpointAt(9.8f, 0f);
                    CheckpointAt(25f, -7f);
                    Pollen(0, 5.5f, 0.8f);
                    Pollen(1, 15f, 3.3f);
                    Pollen(2, 20.5f, 3.3f, Winter);
                    Pollen(3, 24.6f, -6.2f);
                    Pollen(4, 31.6f, -6.2f, Summer);
                    break;
                case 5:
                    CheckpointAt(21f, 0f);
                    CheckpointAt(48f, 7f);
                    Pollen(0, 4.5f, 5.4f);
                    Pollen(1, 12.75f, 3.5f, Autumn);
                    Pollen(2, 33f, 1.9f);
                    Pollen(3, 44.8f, 8.1f);
                    Pollen(4, 70.5f, 10.6f);
                    break;
                case 6:
                    CheckpointAt(10.9f, 0f);
                    CheckpointAt(28f, 0f);
                    Pollen(0, 10.9f, 3.2f, Spring);
                    Pollen(1, 17f, 0.6f, Winter);
                    Pollen(2, 32.5f, 5.3f, Winter);
                    Pollen(3, 42f, 2.9f);
                    Pollen(4, 49.6f, 6.2f);
                    break;
                case 7:
                    CheckpointAt(25.5f, 0f);
                    Pollen(0, -5f, 3.3f);
                    Pollen(1, 14.2f, 0.4f, Summer);
                    Pollen(2, 17f, 3.3f);
                    Pollen(3, 31.2f, 8.6f);
                    Pollen(4, 44.5f, 9f, Winter);
                    break;
                case 8:
                    CheckpointAt(14f, 0f);
                    CheckpointAt(28.5f, 0f);
                    CheckpointAt(40f, 0f);
                    Pollen(0, 7.5f, 0.9f, Spring);
                    Pollen(1, 21f, 0.5f, Winter);
                    Pollen(2, 34.5f, 5.8f, Winter);
                    Pollen(3, 46f, 0.9f, Autumn);
                    Pollen(4, 53.8f, 4.2f, Autumn);
                    break;
                case 9:
                    CheckpointAt(15f, 0f);
                    CheckpointAt(30f, 0f);
                    Pollen(0, -4f, 3.3f);
                    Pollen(1, 18.5f, 0.9f, Spring);
                    Pollen(2, 25.5f, 0.9f);
                    Pollen(3, 40f, 3.3f);
                    Pollen(4, 55f, 0.9f, Summer);
                    break;
                case 10:
                    CheckpointAt(-5f, 0f);
                    Pollen(0, 9f, 3.7f);
                    Pollen(1, 27f, 3.7f);
                    Pollen(2, 18f, 6.3f);
                    Pollen(3, 4f, 0.9f, Winter);
                    Pollen(4, 33f, 0.9f, Spring);
                    break;
            }
        }
    }
}
