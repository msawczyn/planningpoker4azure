using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Azure.Test
{
    [TestClass]
    public class InitializationListTest
    {
        [TestMethod]
        public void IsEmpty_NewInstance_True()
        {
            // Arrange
            InitializationList target = new InitializationList();

            // Act
            bool result = target.IsEmpty;

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Values_NewInstance_Null()
        {
            // Arrange
            InitializationList target = new InitializationList();

            // Act
            IList<string> result = target.Values;

            // Verify
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ContainsOrNotInit_ExistingValue_ReturnsTrue()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1", "team2" });

            // Act
            bool result = target.ContainsOrNotInit("team2");

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainsOrNotInit_NonexistingValue_ReturnsFalse()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1", "team2" });

            // Act
            bool result = target.ContainsOrNotInit("team3");

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ContainsOrNotInit_NotInitialized_ReturnsTrue()
        {
            // Arrange
            InitializationList target = new InitializationList();

            // Act
            bool result = target.ContainsOrNotInit("team2");

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainsOrNotInit_Empty_ReturnsFalse()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Clear();

            // Act
            bool result = target.ContainsOrNotInit("team3");

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Setup_2ValuesAndNotInitialized_ValuesAreSet()
        {
            // Arrange
            InitializationList target = new InitializationList();

            // Act
            target.Setup(new string[] { "team1", "team2" });

            // Verify
            Assert.IsFalse(target.IsEmpty);
            Assert.IsNotNull(target.Values);
            CollectionAssert.AreEquivalent(new string[] { "team1", "team2" }, target.Values.ToList());
        }

        [TestMethod]
        public void Setup_2ValuesAndNotInitialized_ReturnsTrue()
        {
            // Arrange
            InitializationList target = new InitializationList();

            // Act
            bool result = target.Setup(new string[] { "team1", "team2" });

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Setup_2ValuesAndInitializedAlready_ValuesAreNotSet()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1" });

            // Act
            target.Setup(new string[] { "team3", "team4" });

            // Verify
            Assert.IsFalse(target.IsEmpty);
            Assert.IsNotNull(target.Values);
            CollectionAssert.AreEquivalent(new string[] { "team1" }, target.Values.ToList());
        }

        [TestMethod]
        public void Setup_2ValuesAndInitializedAlready_ReturnsFalse()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1" });

            // Act
            bool result = target.Setup(new string[] { "team3", "team4" });

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Setup_2ValuesAndClearedAlready_ValuesAreNotSet()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Clear();

            // Act
            target.Setup(new string[] { "team3", "team4" });

            // Verify
            Assert.IsTrue(target.IsEmpty);
            Assert.IsNotNull(target.Values);
            CollectionAssert.AreEquivalent(Array.Empty<string>(), target.Values.ToList());
        }

        [TestMethod]
        public void Setup_2ValuesAndClearedAlready_ReturnsFalse()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Clear();

            // Act
            bool result = target.Setup(new string[] { "team3", "team4" });

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Setup_Null_ArgumentNullException()
        {
            // Arrange
            InitializationList target = new InitializationList();

            // Act
            target.Setup(null);
        }

        [TestMethod]
        public void Remove_ExistingValue_ValueNotInCollection()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1", "team2" });

            // Act
            target.Remove("team2");

            // Verify
            CollectionAssert.AreEquivalent(new string[] { "team1" }, target.Values.ToList());
            Assert.IsFalse(target.IsEmpty);
        }

        [TestMethod]
        public void Remove_OnlyValue_CollectionIsEmpty()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team2", "team2" });

            // Act
            target.Remove("team2");

            // Verify
            CollectionAssert.AreEquivalent(Array.Empty<string>(), target.Values.ToList());
            Assert.IsTrue(target.IsEmpty);
        }

        [TestMethod]
        public void Remove_ExistingValue_ReturnsTrue()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1", "team2" });

            // Act
            bool result = target.Remove("team2");

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Remove_NonexistingValue_CollectionIsSame()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1", "team2" });

            // Act
            target.Remove("team3");

            // Verify
            CollectionAssert.AreEquivalent(new string[] { "team1", "team2" }, target.Values.ToList());
            Assert.IsFalse(target.IsEmpty);
        }

        [TestMethod]
        public void Remove_NonexistingValue_ReturnsFalse()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1", "team2" });

            // Act
            bool result = target.Remove("team3");

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Clear_AfterInitialization_IsEmpty()
        {
            // Arrange
            InitializationList target = new InitializationList();
            target.Setup(new string[] { "team1", "team2" });

            // Act
            target.Clear();

            // Verify
            Assert.IsTrue(target.IsEmpty);
            CollectionAssert.AreEquivalent(Array.Empty<string>(), target.Values.ToList());
        }

        [TestMethod]
        public void Clear_NoInitialization_IsEmpty()
        {
            // Arrange
            InitializationList target = new InitializationList();

            // Act
            target.Clear();

            // Verify
            Assert.IsTrue(target.IsEmpty);
            CollectionAssert.AreEquivalent(Array.Empty<string>(), target.Values.ToList());
        }
    }
}
