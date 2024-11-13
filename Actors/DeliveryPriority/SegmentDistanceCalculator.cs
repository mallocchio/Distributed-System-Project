using MathNet.Spatial.Euclidean;

namespace Actors.DeliveryPriority
{
    internal class SegmentDistanceCalculator
    {
        private readonly LineSegment2D _segmentA;
        private readonly LineSegment2D _segmentB;

        public SegmentDistanceCalculator(LineSegment2D segmentA, LineSegment2D segmentB)
        {
            _segmentA = segmentA;
            _segmentB = segmentB;
        }
    }
}
