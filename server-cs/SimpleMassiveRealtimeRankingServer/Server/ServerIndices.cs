﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Containers.RedBlackTree;
using CSharpUtils.Extensions;

namespace SimpleMassiveRealtimeRankingServer.Server
{
	public class ServerIndices
	{
		public enum SortingDirection : int
		{
			Invalid = 0,
			Ascending = +1,
			Descending = -1,
		}

		public class UserScore
		{
			public uint UserId;
			public uint ScoreTimeStamp;
			public int ScoreValue;

			public override string ToString()
			{
				return String.Format(
					"User(UserId:{0}, ScoreTimeStamp:{1}, ScoreValue:{2})",
					UserId, ScoreTimeStamp, ScoreValue
				);
			}

			public bool ShouldUpdate(uint ScoreTimeStamp, int ScoreValue)
			{
				if (ScoreTimeStamp < this.ScoreTimeStamp)
				{
					return false;
				}
				if (ScoreValue < this.ScoreValue)
				{
					return false;
				}
				return true;
			}

			public void SetScore(uint ScoreTimeStamp, int ScoreValue)
			{
				this.ScoreTimeStamp = ScoreTimeStamp;
				this.ScoreValue = ScoreValue;
			}
		}

		public class UserScoreIndex : IComparer<UserScore>
		{
			public int IndexId;
			internal SortingDirection SortingDirection = SortingDirection.Invalid;
			internal Dictionary<uint, UserScore> UserScoresByUserId;
			internal RedBlackTreeWithStats<UserScore> Tree;

			public UserScoreIndex(int IndexId, SortingDirection SortingDirection)
			{
				this.IndexId = IndexId;
				this.SortingDirection = SortingDirection;
				this.Tree = new RedBlackTreeWithStats<UserScore>(this);
				this.UserScoresByUserId = new Dictionary<uint, UserScore>();
			}

			/// <summary>
			/// First it sorts by -Score.
			/// Then it sorts by -Timestamp.
			/// Then it sorts by +UserId.
			/// </summary>
			/// <param name="A"></param>
			/// <param name="B"></param>
			/// <returns></returns>
			int IComparer<UserScore>.Compare(UserScore A, UserScore B)
			{
				int Result;

				Result = A.ScoreValue.CompareTo(B.ScoreValue);
				Result *= (int)SortingDirection;
				if (Result == 0)
				{
					Result = A.ScoreTimeStamp.CompareTo(B.ScoreTimeStamp);
					if (Result == 0)
					{
						Result = A.UserId.CompareTo(B.UserId);
					}
				}

				return Result;
			}

			public UserScore GetUserScore(uint UserId)
			{
				return this.UserScoresByUserId[UserId];
			}

			public UserScore UpdateUserScore(uint UserId, uint ScoreTimeStamp, int ScoreValue)
			{
				bool IsNew = false;

				var UserScore = this.UserScoresByUserId.GetOrCreate(UserId, () => {
					var NewUserScore = new UserScore() {
						UserId = UserId,
						ScoreTimeStamp = ScoreTimeStamp,
						ScoreValue = ScoreValue,
					};
					Tree.Add(NewUserScore);
					IsNew = true;
					return NewUserScore;
				});

				if (!IsNew && UserScore.ShouldUpdate(ScoreTimeStamp, ScoreValue))
				{
					this.Tree.Remove(UserScore);
					{
						UserScore.SetScore(ScoreTimeStamp, ScoreValue);
					}
					this.Tree.Add(UserScore);
				}

				return UserScore;
			}

			public IEnumerable<UserScore> GetRange(int StartingPosition, int Count)
			{
				return Tree.All.Skip(StartingPosition).Take(Count);
			}
		}

		internal List<UserScoreIndex> Indices = new List<UserScoreIndex>();
		internal Dictionary<string, int> IndicesByName = new Dictionary<string, int>();

		public UserScoreIndex this[int IndexId]
		{
			get
			{
				return this.Indices[IndexId];
			}
		}

		public UserScoreIndex this[string IndexName]
		{
			get
			{
				lock (this)
				{
					return Indices[IndicesByName.GetOrCreate(IndexName, () =>
					{
						SortingDirection SortingDirection = SortingDirection.Invalid;
						if (IndexName[0] == '+') SortingDirection = SortingDirection.Ascending;
						if (IndexName[0] == '-') SortingDirection = SortingDirection.Descending;
						if (SortingDirection == SortingDirection.Invalid) throw(new Exception("The index must be called '-IndexName' or '+IndexName' wether is a descending index or ascending index."));
						var Index = new UserScoreIndex(Indices.Count, SortingDirection);
						Indices.Add(Index);
						return Index.IndexId;
					})];
				}
			}
		}

		public IEnumerable<UserScore> GetRange(int IndexId, int StartingPosition, int Count)
		{
			return this.Indices[IndexId].GetRange(StartingPosition, Count);
		}
	}
}