using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Duracellko.PlanningPoker.Domain.Test
{
    [TestClass]
    public class EstimateResultTest
    {
        [TestMethod]
        public void Constructor_ScrumMasterAndMember_MembersHasNoEstimate()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);

            // Act
            EstimateResult result = new EstimateResult(new Member[] { master, member });

            // Verify
            KeyValuePair<Member, Estimate>[] expectedResult = new KeyValuePair<Member, Estimate>[]
            {
                new KeyValuePair<Member, Estimate>(member, null),
                new KeyValuePair<Member, Estimate>(master, null),
            };
            CollectionAssert.AreEquivalent(expectedResult, result.ToList());
        }

        [TestMethod]
        public void Constructor_EmptyCollection_EmptyCollection()
        {
            // Arrange
            IEnumerable<Member> members = Enumerable.Empty<Member>();

            // Act
            EstimateResult result = new EstimateResult(members);

            // Verify
            KeyValuePair<Member, Estimate>[] expectedResult = Array.Empty<KeyValuePair<Member, Estimate>>();
            CollectionAssert.AreEquivalent(expectedResult, result.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Null_ArgumentNullException()
        {
            // Act
            EstimateResult result = new EstimateResult(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_DuplicateMember_InvalidOperationException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);

            // Act
            EstimateResult result = new EstimateResult(new Member[] { master, member, master });
        }

        [TestMethod]
        public void IndexerSet_SetScrumMasterEstimate_EstimateOfScrumMasterIsSet()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            EstimateResult target = new EstimateResult(new Member[] { master, member });
            Estimate estimate = new Estimate();

            // Act
            target[master] = estimate;

            // Verify
            KeyValuePair<Member, Estimate>[] expectedResult = new KeyValuePair<Member, Estimate>[]
            {
                new KeyValuePair<Member, Estimate>(member, null),
                new KeyValuePair<Member, Estimate>(master, estimate),
            };
            CollectionAssert.AreEquivalent(expectedResult, target.ToList());
        }

        [TestMethod]
        public void IndexerSet_SetMemberEstimate_EstimateOfMemberIsSet()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            EstimateResult target = new EstimateResult(new Member[] { master, member });
            Estimate estimate = new Estimate();

            // Act
            target[member] = estimate;

            // Verify
            KeyValuePair<Member, Estimate>[] expectedResult = new KeyValuePair<Member, Estimate>[]
            {
                new KeyValuePair<Member, Estimate>(master, null),
                new KeyValuePair<Member, Estimate>(member, estimate),
            };
            CollectionAssert.AreEquivalent(expectedResult, target.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void IndexerSet_MemberNotInResult_KeyNotFoundException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            EstimateResult target = new EstimateResult(new Member[] { master });
            Estimate estimate = new Estimate();

            // Act
            target[member] = estimate;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void IndexerSet_IsReadOnly_InvalidOperationException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            EstimateResult target = new EstimateResult(new Member[] { master, member });
            Estimate estimate = new Estimate();

            // Act
            target.SetReadOnly();
            target[member] = estimate;
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void IndexerGet_MemberNotInResult_KeyNotFoundException()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            EstimateResult target = new EstimateResult(new Member[] { master });

            // Act
            Estimate estimate = target[member];
        }

        [TestMethod]
        public void ContainsMember_MemberIsInResult_ReturnsTrue()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            EstimateResult target = new EstimateResult(new Member[] { master, member });

            // Act
            bool result = target.ContainsMember(member);

            // Verify
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainsMember_MemberIsNotInResult_ReturnsFalse()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            EstimateResult target = new EstimateResult(new Member[] { master });

            // Act
            bool result = target.ContainsMember(member);

            // Verify
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Count_InitializesBy2Members_Returns2()
        {
            // Arrange
            ScrumTeam team = new ScrumTeam("test team");
            ScrumMaster master = team.SetScrumMaster("master");
            Member member = (Member)team.Join("member", false);
            EstimateResult target = new EstimateResult(new Member[] { master, member });

            // Act
            int result = target.Count;

            // Verify
            Assert.AreEqual<int>(2, result);
        }

        [TestMethod]
        public void SetReadOnly_Execute_SetsIsReadOnly()
        {
            // Arrange
            EstimateResult target = new EstimateResult(Enumerable.Empty<Member>());

            // Act
            target.SetReadOnly();

            // Verify
            Assert.IsTrue(target.IsReadOnly);
        }

        [TestMethod]
        public void SetReadOnly_GetAfterConstruction_ReturnsFalse()
        {
            // Arrange
            EstimateResult target = new EstimateResult(Enumerable.Empty<Member>());

            // Act
            bool result = target.IsReadOnly;

            // Verify
            Assert.IsFalse(result);
        }
    }
}
