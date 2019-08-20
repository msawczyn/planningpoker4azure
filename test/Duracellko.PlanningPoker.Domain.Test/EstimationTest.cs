using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimateTest
    {
        [TestMethod]
        public void Constructor_Null_ValueIsNull()
        {
            // Arrange
            double? value = null;

            // Act
            Estimate result = new Estimate(value);

            // Verify
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public void Constructor_Zero_ValueIsZero()
        {
            // Arrange
            double value = 0.0;

            // Act
            Estimate result = new Estimate(value);

            // Verify
            Assert.AreEqual<double?>(value, result.Value);
        }

        [TestMethod]
        public void Constructor_3point3_ValueIs3point3()
        {
            // Arrange
            double value = 3.3;

            // Act
            Estimate result = new Estimate(value);

            // Verify
            Assert.AreEqual<double?>(value, result.Value);
        }

        [TestMethod]
        public void Constructor_PositiveInfinity_ValueIsPositiveInfinity()
        {
            // Arrange
            double value = double.PositiveInfinity;

            // Act
            Estimate result = new Estimate(value);

            // Verify
            Assert.AreEqual<double?>(value, result.Value);
        }

        [TestMethod]
        public void Constructor_NotANumber_ValueIsNaN()
        {
            // Arrange
            double value = double.NaN;

            // Act
            Estimate result = new Estimate(value);

            // Verify
            Assert.AreEqual<double?>(value, result.Value);
        }

        [TestMethod]
        public void Equals_ZeroAndZero_ReturnsTrue()
        {
            // Arrange
            Estimate target = new Estimate(0.0);
            Estimate estimation2 = new Estimate(0.0);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_3point6And3point6_ReturnsTrue()
        {
            // Arrange
            Estimate target = new Estimate(3.6);
            Estimate estimation2 = new Estimate(3.6);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_6And2_ReturnsFalse()
        {
            // Arrange
            Estimate target = new Estimate(6);
            Estimate estimation2 = new Estimate(2);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_DiferentObjectTypes_ReturnsFalse()
        {
            // Arrange
            Estimate target = new Estimate(6);

            // Act
            bool result = target.Equals(2.0);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_Null_ReturnsFalse()
        {
            // Arrange
            Estimate target = new Estimate(6);

            // Act
            bool result = target.Equals(null);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_NullEstimates_ReturnsTrue()
        {
            // Arrange
            Estimate target = new Estimate();
            Estimate estimation2 = new Estimate(null);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_NullEstimateAndZero_ReturnsTrue()
        {
            // Arrange
            Estimate target = new Estimate();
            Estimate estimation2 = new Estimate(0.0);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_PositiveInfinityAndPositiveInfinity_ReturnsTrue()
        {
            // Arrange
            Estimate target = new Estimate(double.PositiveInfinity);
            Estimate estimation2 = new Estimate(double.PositiveInfinity);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Equals_ZeroAndPositiveInfinity_ReturnsFalse()
        {
            // Arrange
            Estimate target = new Estimate();
            Estimate estimation2 = new Estimate(double.PositiveInfinity);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_NaNAndNaN_ReturnsFalse()
        {
            // Arrange
            Estimate target = new Estimate(double.NaN);
            Estimate estimation2 = new Estimate(double.NaN);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Equals_ZeroAndNaN_ReturnsFalse()
        {
            // Arrange
            Estimate target = new Estimate();
            Estimate estimation2 = new Estimate(double.NaN);

            // Act
            bool result = target.Equals(estimation2);

            // Verify
            Assert.IsFalse(result);
        }
    }
}
