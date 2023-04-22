namespace HoneyScoop.Util;

internal readonly struct IntRange {
	internal readonly int Start;
	internal readonly int End;

	internal IntRange(int start, int end) {
		Start = start;
		End = end;
		if(Start > End) {
			throw new ArgumentException();
		}
	}

	internal bool Contains(IntRange other) {
		return other.Start >= this.Start && other.Start < this.End && other.End <= this.End && other.End > this.Start;
	}

	internal bool Intersects(IntRange other) {
		return (other.Start > this.Start && other.Start < this.End) || (other.End < this.End && other.End > this.Start);
	}

	internal bool IntersectsOrContiguous(IntRange other) {
		return (other.Start > this.Start && other.Start <= this.End + 1) || (other.End < this.End && other.End >= this.Start - 1);
	}

	internal IntRange Merge(IntRange other) {
		return new IntRange(Int32.Min(this.Start, other.Start), Int32.Max(this.End, other.End));
	}

	/// <summary>
	/// This method adds <see cref="newRange"/> to <see cref="ranges"/>, or, if newRange intersects any ranges in the list,
	/// it is merged with it.
	/// </summary>
	/// <param name="ranges">The list of ranges that is modified by this method</param>
	/// <param name="newRange"></param>
	internal static void AddMerging(List<IntRange> ranges, IntRange newRange) {
		for(int i = 0; i < ranges.Count; i++) {
			bool canMerge = ranges[i].IntersectsOrContiguous(newRange);
			if(canMerge) {
				ranges[i] = ranges[i].Merge(newRange);
				return;
			}
		}
		ranges.Add(newRange);
	}

	/// <summary>
	/// Returns true if the contents of <see cref="other"/> are fully contained by <see cref="ranges"/>
	/// </summary>
	/// <param name="ranges"></param>
	/// <param name="other"></param>
	internal static bool FullyContainedBy(List<IntRange> ranges, IntRange other) {
		for(int i = 0; i < ranges.Count; i++) {
			if(ranges[i].Contains(other)) {
				return true;
			}
		}

		return false;
	}
}