using Actors.DeliveryPriority;
using MathNet.Spatial.Euclidean;

namespace Tests.ActorTests
{
    public class DeliveryPathTests
    {
        [Fact]
        public void CheckParallel()
        {
            DeliveryPath deliveryPathA = new(
                Point2D.Origin,
                new Point2D(28, 10)
                );

            DeliveryPath deliveryPathB = new(
                new Point2D(30, 0),
                new Point2D(58, 10)
                );

            Assert.Null(deliveryPathA.ClosestCollisionPoint(deliveryPathB));
        }

        [Fact]
        public void CheckIncident()
        {
            DeliveryPath deliveryPathA = new(
                Point2D.Origin,
                new Point2D(28, 28)
                );

            DeliveryPath deliveryPathB = new(
                new Point2D(0, 28),
                new Point2D(28, 0)
                );

            Point2D? pc = deliveryPathA.ClosestCollisionPoint(deliveryPathB);
            Assert.NotNull(pc);

            // Arrotonda le coordinate X e Y a numeri interi
            int roundedX = (int)Math.Round(pc!.Value.X);
            int roundedY = (int)Math.Round(pc!.Value.Y);
            Point2D? roundedPoint = new Point2D(roundedX, roundedY);

            Assert.True(roundedPoint is not null && roundedPoint.Value.Equals(new Point2D(12, 12)));
        }

        [Fact]
        public void TestVector()
        {
            var vector = new Point2D(2, 2) - new Point2D(1, 1);
            Console.WriteLine(vector);
        }
    }
}