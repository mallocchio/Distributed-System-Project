using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace Actors.DeliveryPriority
{
    // Tratta che il drone vuole percorrere, si tratta
    // di uno spostamento da un punto A ad un punto B ad una certa
    // velocità. 
    // 
    // Permette di calcolare i conflitti e 
    // di ottenere la distanza temporale da un certo punto.
    public class DeliveryPath
    {
        public Point2D StartPoint { get; }
        public Point2D EndPoint { get; }
        
        // Velocità del drone misurata in unità spaziali al secondo.
        public float Speed { get; }
        internal const float SideDistance = 3.0f;
        internal const float SafetyDistance = 10.0f;

        public DeliveryPath(Point2D startPoint, Point2D endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            Speed = 8.3f;
        }

        // Calcola, se esiste, il punto di conflitto più 
        // vicino al mio punto di partenza. 
        public Point2D? ClosestCollisionPoint(DeliveryPath p)
        {
            var direction = p.PathSegment().Direction.Normalize() * SideDistance;
            var perpendicular = p.PathSegment().Direction.Orthogonal.Normalize() * SideDistance;

            // vertici del poligono
            Point2D bottomLeft = p.StartPoint - direction + perpendicular;
            Point2D bottomRight = p.StartPoint - direction - perpendicular; 
            Point2D topLeft = p.EndPoint + direction + perpendicular;
            Point2D topRight = p.EndPoint + direction - perpendicular;

            // controlla se il punto di partenza è già incluso nel poligono
            if (new Polygon2D(bottomLeft, bottomRight, topRight, topLeft).EnclosesPoint(StartPoint)) 
            { 
                return new Point2D(StartPoint.X, StartPoint.Y);
            }

            // se non è incluso, costruisce i 4 segmenti e calcola le intersezioni
            LineSegment2D[] sideSegments =
            {
                new LineSegment2D(bottomLeft, bottomRight),
                new LineSegment2D(bottomRight, topRight),
                new LineSegment2D(topRight, topLeft),
                new LineSegment2D(topLeft, bottomLeft),
            };

            Point2D? collisionPoint = null;

            foreach(var s in sideSegments)
            {
                Point2D intersect;
                if (PathSegment().TryIntersect(s, out intersect, Angle.FromDegrees(0)))
                {
                    if (
                        collisionPoint is null ||

                        StartPoint.DistanceTo(intersect) <
                        StartPoint.DistanceTo(collisionPoint.Value)
                        )
                    {
                        collisionPoint = intersect;
                    }
                }
            }

            return collisionPoint;
        }

        public LineSegment2D PathSegment()
        {
            return new LineSegment2D(StartPoint, EndPoint);
        }

        // Calcola quanto tempo ci metto a raggiungere
        // un certo punto a partire dal mio punto di partenza.
        public TimeSpan TimeDistance(Point2D p)
        {
            return p!=StartPoint ? TimeSpan.FromSeconds(
                new Line2D(StartPoint, p).Length / Speed
                ) : TimeSpan.Zero;
        }

        public TimeSpan ExpectedDuration()
        {
            return TimeDistance(EndPoint);
        }

        public override string? ToString()
        {
            return "\n{"
                + $"\n\tStart: {StartPoint}, "
                + $"\n\tEnd: {EndPoint}, "
                + $"\n\tExpectedTime: {ExpectedDuration()}, "
                + "\n}";
        }
    }
}
