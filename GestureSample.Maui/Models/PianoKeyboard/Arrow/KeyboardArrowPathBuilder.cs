namespace GestureSample.Maui.Models
{
    internal static class KeyboardArrowPathBuilder
    {
        public static string BuildPathData(
            ArrowType arrowType,
            ArrowMovementMode movementMode,
            Direction direction,
            int aboveKeyNumber,
            double columnWidth,
            double arrowStart,
            double arrowEnd,
            double arrowEdgeX)
        {
            if (movementMode == ArrowMovementMode.OneByOne)
                return BuildOrdinalHoldPath(direction, aboveKeyNumber, columnWidth);

            if (movementMode == ArrowMovementMode.JumpThroughMiddle)
                return BuildJumpThroughMiddlePath(arrowStart, arrowEnd, arrowEdgeX);

            if (arrowType == ArrowType.Rounded)
            {
                double roundedStart = GetRoundedStartX(direction, aboveKeyNumber, columnWidth);
                double arcEnd = roundedStart + (direction == Direction.Right ? 20 : -20);
                double roundedEnd = arcEnd + (direction == Direction.Right ? 20 : -20);
                double roundedEdgeX = roundedEnd + (direction == Direction.Right ? -10 : 10);
                int clockwise = direction == Direction.Right ? 1 : 0;

                return $"M {F(roundedStart)},30 A 20,20 0 0 {clockwise} {F(arcEnd)},10 L {F(roundedEnd)},10 L {F(roundedEdgeX)},0 M {F(roundedEnd)},10 L {F(roundedEdgeX)},20";
            }

            return movementMode switch
            {
                ArrowMovementMode.Arpeggio => BuildWavyPath(direction, arrowStart, arrowEnd, arrowEdgeX),
                ArrowMovementMode.Splited => BuildSplitPath(arrowStart, arrowEnd, arrowEdgeX, 0.44, 0.62, includeCenterBar: false),
                ArrowMovementMode.MiddleSplited => BuildSplitPath(arrowStart, arrowEnd, arrowEdgeX, 0.48, 0.52, includeCenterBar: true),
                _ => BuildStraightPath(arrowStart, arrowEnd, arrowEdgeX)
            };
        }

        public static double GetRoundedLabelCenterX(Direction direction, int aboveKeyNumber, double columnWidth)
        {
            return GetRoundedStartX(direction, aboveKeyNumber, columnWidth);
        }

        private static string BuildStraightPath(double arrowStart, double arrowEnd, double arrowEdgeX)
        {
            return $"M {F(arrowStart)},50 L {F(arrowStart)},15 L {F(arrowEnd)},15 L {F(arrowEdgeX)},2 M {F(arrowEnd)},15 L {F(arrowEdgeX)},28";
        }

        private static string BuildWavyPath(Direction direction, double arrowStart, double arrowEnd, double arrowEdgeX)
        {
            double distance = arrowEnd - arrowStart;
            double waveWidth = distance / 6.0;
            double x = arrowStart;
            string data = $"M {F(arrowStart)},50 L {F(arrowStart)},15";

            for (int i = 0; i < 6; i++)
            {
                double x1 = x + waveWidth * 0.33;
                double x2 = x + waveWidth * 0.66;
                double x3 = x + waveWidth;
                double y1 = i % 2 == 0 ? 6 : 24;
                double y2 = i % 2 == 0 ? 24 : 6;
                data += $" C {F(x1)},{F(y1)} {F(x2)},{F(y2)} {F(x3)},15";
                x = x3;
            }

            data += $" L {F(arrowEdgeX)},2 M {F(arrowEnd)},15 L {F(arrowEdgeX)},28";
            return data;
        }

        private static string BuildSplitPath(
            double arrowStart,
            double arrowEnd,
            double arrowEdgeX,
            double firstBreakRatio,
            double secondBreakRatio,
            bool includeCenterBar)
        {
            double firstBreak = Lerp(arrowStart, arrowEnd, firstBreakRatio);
            double secondBreak = Lerp(arrowStart, arrowEnd, secondBreakRatio);
            double center = Lerp(arrowStart, arrowEnd, 0.5);
            string data = $"M {F(arrowStart)},50 L {F(arrowStart)},15 L {F(firstBreak)},15 M {F(secondBreak)},15 L {F(arrowEnd)},15 L {F(arrowEdgeX)},2 M {F(arrowEnd)},15 L {F(arrowEdgeX)},28";

            if (includeCenterBar)
                data += $" M {F(center)},2 L {F(center)},28";

            return data;
        }

        private static string BuildOrdinalHoldPath(Direction direction, int aboveKeyNumber, double columnWidth)
        {
            double roundedStart = GetRoundedStartX(direction, aboveKeyNumber, columnWidth);
            double firstArcEnd = roundedStart + (direction == Direction.Right ? 18 : -18);
            double firstEnd = firstArcEnd + (direction == Direction.Right ? 18 : -18);
            double secondStart = roundedStart + (direction == Direction.Right ? -18 : 18);
            double secondArcEnd = secondStart + (direction == Direction.Right ? 18 : -18);
            double secondEnd = secondArcEnd + (direction == Direction.Right ? 18 : -18);
            double arrowEdgeX = firstEnd + (direction == Direction.Right ? -10 : 10);
            int clockwise = direction == Direction.Right ? 1 : 0;

            return $"M {F(secondStart)},30 A 18,18 0 0 {clockwise} {F(secondArcEnd)},10 L {F(secondEnd)},10 M {F(roundedStart)},30 A 18,18 0 0 {clockwise} {F(firstArcEnd)},10 L {F(firstEnd)},10 L {F(arrowEdgeX)},0 M {F(firstEnd)},10 L {F(arrowEdgeX)},20";
        }

        private static string BuildJumpThroughMiddlePath(double arrowStart, double arrowEnd, double arrowEdgeX)
        {
            double middle = Lerp(arrowStart, arrowEnd, 0.5);
            double firstControl = Lerp(arrowStart, middle, 0.5);
            double secondControl = Lerp(middle, arrowEnd, 0.5);
            double middleDashStart = middle + (arrowEnd >= arrowStart ? -7 : 7);
            double middleDashEnd = middle + (arrowEnd >= arrowStart ? 7 : -7);
            double peakY = 0;
            double baselineY = 15;

            return $"M {F(arrowStart)},50 L {F(arrowStart)},{F(baselineY)} Q {F(firstControl)},{F(peakY)} {F(middle)},{F(baselineY)} M {F(middleDashStart)},{F(baselineY)} L {F(middleDashEnd)},{F(baselineY)} M {F(middle)},{F(baselineY)} Q {F(secondControl)},{F(peakY)} {F(arrowEnd)},{F(baselineY)} L {F(arrowEdgeX)},2 M {F(arrowEnd)},{F(baselineY)} L {F(arrowEdgeX)},28";
        }

        private static double GetRoundedStartX(Direction direction, int aboveKeyNumber, double columnWidth)
        {
            return columnWidth / 2 + ((aboveKeyNumber == 1 || direction == Direction.Right) ? 0 : columnWidth);
        }

        private static double Lerp(double start, double end, double ratio)
        {
            return start + ((end - start) * ratio);
        }

        private static string F(double value)
        {
            return Math.Round(value, 1).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
