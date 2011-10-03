using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Containers.RedBlackTree;
using CSharpUtils.Extensions;

namespace CSharpUtilsSandBox.Server
{
	public class ServerIndices
	{
		public enum SortingDirection : int
		{
			Ascending = +1,
			Descending = -1,
		}

		public class User
		{
			public struct Score
			{
				public uint TimeStamp;
				public int Value;
			}

			public void SetScore(uint ScoreId, uint ScoreTimeStamp, int ScoreValue)
			{
				Score Score = this.Scores.GetOrCreate(UserId);
				Score.TimeStamp = ScoreTimeStamp;
				Score.Value = ScoreValue;
			}

			public uint UserId;
			public Dictionary<uint, Score> Scores = new Dictionary<uint, Score>();
		}

		public class Index : IComparer<User>
		{
			SortingDirection SortingDirection;
			uint ScoreIndexToSort;
			RedBlackTreeWithStats<User> Tree;

			public Index(SortingDirection SortingDirection, uint ScoreIndexToSort)
			{
				this.SortingDirection = SortingDirection;
				this.ScoreIndexToSort = ScoreIndexToSort;
				this.Tree = new RedBlackTreeWithStats<User>(this);
			}

			/// <summary>
			/// First it sorts by -Score.
			/// Then it sorts by -Timestamp.
			/// Then it sorts by +UserId.
			/// </summary>
			/// <param name="A"></param>
			/// <param name="B"></param>
			/// <returns></returns>
			int IComparer<User>.Compare(User A, User B)
			{
				var ScoreA = A.Scores[ScoreIndexToSort];
				var ScoreB = B.Scores[ScoreIndexToSort];

				var ValueA = ScoreA.Value;
				var ValueB = ScoreB.Value;

				return ValueA.CompareTo(ValueB);
			}

			internal void SetInfo(SortingDirection SortingDirection, uint ScoreIndexToSort)
			{
				if ((this.SortingDirection != SortingDirection) || (this.ScoreIndexToSort != ScoreIndexToSort))
				{
					this.SortingDirection = SortingDirection;
					this.ScoreIndexToSort = ScoreIndexToSort;
					this.Tree = new RedBlackTreeWithStats<User>(this);
				}
			}
		}

		internal Dictionary<uint, User> Users = new Dictionary<uint, User>();
		internal Dictionary<uint, Index> Indices = new Dictionary<uint, Index>();

		public void UpdateUserScore(uint UserId, uint ScoreId, uint ScoreTimeStamp, int ScoreValue)
		{
			this.Users.GetOrCreate(UserId).SetScore(ScoreId, ScoreTimeStamp, ScoreValue);
		}

		public void SetIndex(uint IndexId, SortingDirection SortingDirection, uint ScoreIndexToSort)
		{
			var Index = this.Indices.GetOrCreate(IndexId, () => { return new Index(SortingDirection, ScoreIndexToSort); });
			Index.SetInfo(SortingDirection, ScoreIndexToSort);
		}
	}
}
