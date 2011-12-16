using SimpleMassiveRealtimeRankingServer.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
//using CSharpUtils.Json;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServerTests
{
	public class ServerIndicesTest
	{
		[TestClass]
		public class ServerIndicesTest_SetIndex
		{
			ServerIndices ServerIndices;
			ServerIndices.UserScoreIndex DescendingIndex;
			ServerIndices.UserScoreIndex AscendingIndex;

			[TestInitialize]
			public void Initialize()
			{
				this.ServerIndices = new ServerIndices();
				DescendingIndex = ServerIndices["-TestIndex"];
				AscendingIndex = ServerIndices["+TestIndex"];
				AscendingIndex.UpdateUserScore(UserId: 2, ScoreTimeStamp: 1000, ScoreValue: 100);
				DescendingIndex.UpdateUserScore(UserId: 2, ScoreTimeStamp: 1000, ScoreValue: 100);
			}

			[TestMethod]
			public void SameScoreAndTimeStampOrdersById()
			{
				AscendingIndex.UpdateUserScore(UserId: 0, ScoreTimeStamp: 1000, ScoreValue: 100);
				AscendingIndex.UpdateUserScore(UserId: 1, ScoreTimeStamp: 1000, ScoreValue: 100);
				Assert.AreEqual(
					"0,1,2",
					String.Join(",", AscendingIndex.GetRange(StartingPosition: 0, Count: 1000).Select(Item => Item.UserId))
				);
			}

			[TestMethod]
			public void SameTimeStampOrdersByScoreOnDescendingIndex()
			{
				DescendingIndex.UpdateUserScore(UserId: 4, ScoreTimeStamp: 1000, ScoreValue: 40);
				DescendingIndex.UpdateUserScore(UserId: 3, ScoreTimeStamp: 1000, ScoreValue: 200);
				Assert.AreEqual(
					"3,2,4",
					String.Join(",", DescendingIndex.GetRange(StartingPosition: 0, Count: 1000).Select(Item => Item.UserId))
				);
			}

			[TestMethod]
			public void SameTimeStampOrdersByScoreOnAscendingIndex()
			{
				AscendingIndex.UpdateUserScore(UserId: 4, ScoreTimeStamp: 1000, ScoreValue: 40);
				AscendingIndex.UpdateUserScore(UserId: 3, ScoreTimeStamp: 1000, ScoreValue: 200);
				Assert.AreEqual(
					"4,2,3",
					String.Join(",", AscendingIndex.GetRange(StartingPosition: 0, Count: 1000).Select(Item => Item.UserId))
				);
			}

			[TestMethod]
			public void SameScoreOrdersByTimestampAscending()
			{
				DescendingIndex.UpdateUserScore(UserId: 4, ScoreTimeStamp: 999, ScoreValue: 100);
				DescendingIndex.UpdateUserScore(UserId: 3, ScoreTimeStamp: 1001, ScoreValue: 100);
				Assert.AreEqual(
					"4,2,3",
					String.Join(",", DescendingIndex.GetRange(StartingPosition: 0, Count: 1000).Select(Item => Item.UserId))
				);
			}
		}

		[TestClass]
		public class ServerIndicesTest_UpdateUser
		{
			const int TestUserId = 777;
			ServerIndices ServerIndices;
			ServerIndices.UserScoreIndex DescendingIndex;


			[TestInitialize]
			public void Initialize()
			{
				this.ServerIndices = new ServerIndices();
				DescendingIndex = ServerIndices["-TestIndex"];
				DescendingIndex.UpdateUserScore(UserId: TestUserId, ScoreTimeStamp: 1000, ScoreValue: 100);
			}

			[TestMethod]
			public void UpdateOnHigherTimestampAndScore()
			{
				DescendingIndex.UpdateUserScore(UserId: TestUserId, ScoreTimeStamp: 1100, ScoreValue: 200);
				Assert.AreEqual(
					"User(UserId:777, ScoreTimeStamp:1100, ScoreValue:200)",
					DescendingIndex.GetUserScore(UserId: TestUserId).ToString()
				);
			}

			[TestMethod]
			public void DontUpdateOnLowerTimestamp()
			{
				DescendingIndex.UpdateUserScore(UserId: TestUserId, ScoreTimeStamp: 900, ScoreValue: 300);
				Assert.AreEqual(
					"User(UserId:777, ScoreTimeStamp:1000, ScoreValue:100)",
					DescendingIndex.GetUserScore(UserId: TestUserId).ToString()
				);
			}

			[TestMethod]
			public void DontUpdateOnLowerScore()
			{
				DescendingIndex.UpdateUserScore(UserId: TestUserId, ScoreTimeStamp: 1200, ScoreValue: 50);
				Assert.AreEqual(
					"User(UserId:777, ScoreTimeStamp:1000, ScoreValue:100)",
					DescendingIndex.GetUserScore(UserId: TestUserId).ToString()
				);
			}

			[TestMethod]
			public void UpdateOnSameTimeStamp()
			{
				DescendingIndex.UpdateUserScore(UserId: TestUserId, ScoreTimeStamp: 1000, ScoreValue: 150);
				Assert.AreEqual(
					"User(UserId:777, ScoreTimeStamp:1000, ScoreValue:150)",
					DescendingIndex.GetUserScore(UserId: TestUserId).ToString()
				);
			}

			[TestMethod]
			public void ExceedCappedCollection()
			{
				var CappedIndex = ServerIndices["-TestIndex@0:2"];
				CappedIndex.UpdateUserScore(UserId: 1, ScoreTimeStamp: 1000, ScoreValue: 100);
				CappedIndex.UpdateUserScore(UserId: 2, ScoreTimeStamp: 1000, ScoreValue: 200);
				CappedIndex.UpdateUserScore(UserId: 3, ScoreTimeStamp: 1000, ScoreValue: 300);
				CappedIndex.UpdateUserScore(UserId: 4, ScoreTimeStamp: 1000, ScoreValue: 150);
				Assert.AreEqual(
					"3,2",
					String.Join(",", CappedIndex.GetRange(0, 1000).Select(Item => Item.UserId))
				);
			}
		}
	}
}
