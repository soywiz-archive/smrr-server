using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Containers.RedBlackTree;
using CSharpUtils.Extensions;
using System.Text.RegularExpressions;

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
            public string IndexName { get; protected set; }
			public int IndexId;
			internal SortingDirection SortingDirection = SortingDirection.Invalid;
			internal Dictionary<uint, UserScore> UserScoresByUserId;
			internal RedBlackTreeWithStats<UserScore> Tree;
			public int CappedCount = -1;

			public UserScoreIndex(int IndexId, string IndexName, SortingDirection SortingDirection)
			{
				this.IndexId = IndexId;
                this.IndexName = IndexName;
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

				if (this.CappedCount > -1)
				{
					while (this.UserScoresByUserId.Count > this.CappedCount)
					{
						// Not synchronized for performance!
						this.UserScoresByUserId.Remove(this.Tree.BackElement.UserId);
						this.Tree.RemoveBack();
					}
				}

				return UserScore;
			}

			public IEnumerable<UserScore> GetRange(int StartingPosition, int Count)
			{
				return Tree.All.Skip(StartingPosition).Take(Count);
			}

			public void RemoveAllItems()
			{
				Tree.Clear();
				UserScoresByUserId.Clear();
			}
		}

		protected List<UserScoreIndex> Indices = new List<UserScoreIndex>();
		protected Dictionary<string, int> IndiceIdsByName = new Dictionary<string, int>();

        public int IndicesCount
        {
            get
            {
                return Indices.Count;
            }
        }

        public long TotalNumberOfElements
        {
            get
            {
                long TotalCount = 0;
                foreach (var Index in Indices.ToArray())
                {
                    TotalCount += Index.Tree.Count;
                }
                return TotalCount;
            }
        }

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
					return Indices[IndiceIdsByName.GetOrCreate(IndexName, () =>
					{
						SortingDirection SortingDirection = SortingDirection.Invalid;
						var IndexNameMatch = new Regex(@"^([\+\-])[^:]+(:(\d+))?$", RegexOptions.Compiled).Match(IndexName);
						if (!IndexNameMatch.Success) throw(new Exception("Invalid Index Name '" + IndexName + "'"));
						var DirectionString = IndexNameMatch.Groups[1].Value;
						var CappedCountString = IndexNameMatch.Groups[3].Value;
						int CappedCount = -1;

						if (DirectionString == "+") SortingDirection = SortingDirection.Ascending;
						if (DirectionString == "-") SortingDirection = SortingDirection.Descending;
						if (CappedCountString.Length > 0)
						{
							CappedCount = int.Parse(CappedCountString);
						}

						if (SortingDirection == SortingDirection.Invalid) throw(new Exception("The index must be called '-IndexName' or '+IndexName' wether is a descending index or ascending index."));
                        var Index = new UserScoreIndex(Indices.Count, IndexName, SortingDirection);
						Index.CappedCount = CappedCount;
						//Index.Tree.CappedToNumberOfElements = CappedCount;
						Indices.Add(Index);

						Console.WriteLine(
							"Created Indexd: '{0}' :: IndexId({1}), Direction({2}), CappedCount({3})",
							IndexName,
							Index.IndexId,
							Enum.GetName(typeof(SortingDirection), SortingDirection),
							CappedCount
						);

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
