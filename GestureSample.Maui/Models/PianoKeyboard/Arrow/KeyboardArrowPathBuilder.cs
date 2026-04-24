namespace GestureSample.Maui.Models
{
    internal static class KeyboardArrowPathBuilder
    {
        public static string BuildPathData(
            ArrowType arrowType,
            Direction direction,
            int aboveKeyNumber,
            double columnWidth,
            double arrowStart,
            double arrowEnd,
            double arrowEdgeX)
        {
            if (arrowType == ArrowType.Rounded)
            {
                double roundedStart = columnWidth / 2 + ((aboveKeyNumber == 1 || direction == Direction.Right) ? 0 : columnWidth);
                double arcEnd = roundedStart + (direction == Direction.Right ? 20 : -20);
                double roundedEnd = arcEnd + (direction == Direction.Right ? 20 : -20);
                double roundedEdgeX = roundedEnd + (direction == Direction.Right ? -10 : 10);
                int clockwise = direction == Direction.Right ? 1 : 0;

                return string.Format(
                    "M {0},30 A 20,20 0 0 {4} {3},10 L {1},10 L {2},0 M {1},10 L {2},20",
                    roundedStart,
                    roundedEnd,
                    roundedEdgeX,
                    arcEnd,
                    clockwise);
            }

            return string.Format(
                "M {0},50 L {0},15 L {1},15 L {2},2 M {1},15 L {2},28",
                arrowStart,
                arrowEnd,
                arrowEdgeX);
        }
    }
}
