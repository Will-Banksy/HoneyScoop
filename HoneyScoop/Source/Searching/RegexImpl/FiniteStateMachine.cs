namespace HoneyScoop.Searching.RegexImpl;

// Regex with Finite Automata (Finite State Machines): https://deniskyashif.com/2019/02/17/implementing-a-regular-expression-engine/
//                                                     https://swtch.com/~rsc/regexp/regexp1.html

/// <summary>
/// Also known as NFA (Nondeterministic Finite Automata) and referred to as such in the codebase
/// </summary>
/// <typeparam name="T">The type of data that the NFA is matching</typeparam>
internal readonly struct FiniteStateMachine<T> where T: struct {
	/// <summary>
	/// Reference type so that instances of State can be referenced all over
	/// <br /><br />
	/// A State object represents a state in a Finite State Machine, that has connections to other states represented by a tuple (State, T?)
	/// where the State is the connected state and the T? is the connection character or a null represents an ε-connection (empty/transparent connection)
	/// </summary>
	internal class State { // TODO: This is kinda expensive to construct - Maybe add a constructor that doesn't then also initialise `connections`
		private readonly int _uuid;
		internal readonly List<StateConnection> Connections; // TODO: Array or List?

		/// <summary>
		/// Specify a capacity to preallocate when creating the <c>List&lt;StateConnection&gt;</c> of connections (for optimisation purposes)
		/// </summary>
		/// <param name="uuid">The UUID assigned to this State</param>
		/// <param name="preallocCapacity">Pre-allocated capacity of the internal list of connections</param>
		internal State(ref int uuid, int preallocCapacity = 4) {
			_uuid = uuid;
			uuid++;
			this.Connections = new List<StateConnection>(preallocCapacity);
		}

		internal State AddConnection(StateConnection connection) {
			this.Connections.Add(connection);
			return this;
		}

		internal State AddEpsilonConnection(State other) {
			this.Connections.Add(new StateConnection(other, default, true, default));
			return this;
		}
		
		internal State AddSymbolConnection(State other, T symbol, bool matchesAny) {
			this.Connections.Add(new StateConnection(other, symbol, false, matchesAny));
			return this;
		}

		internal bool Equals(State other) {
			return _uuid == other._uuid;
		}

		public override bool Equals(object? obj) {
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != this.GetType()) return false;
			return Equals((State)obj);
		}

		public override int GetHashCode() {
			return _uuid;
		}

		public override string ToString() {
			// return $"State({_uuid})";
			return $"{_uuid}";
		}
	}

	/// <summary>
	/// A StateConnection object represents a directional connection from one State to another - The <c>Next</c> state.
	/// If the connection is a ε-connection, <c>Transparent</c> is true, otherwise, <c>Symbol</c> is used
	/// </summary>
	internal readonly record struct StateConnection(State Next, T Symbol, bool Transparent, bool Wildcard);

	internal readonly State Start;
	internal readonly State End;

	public FiniteStateMachine(ref int uuid) {
		End = new State(ref uuid);
		Start = new State(ref uuid).AddEpsilonConnection(End);
	}

	/// <summary>
	/// Constructs a new NFA with a connection between it's start and end having value <c>symbol</c> or being a transparent connection if <c>symbol</c> is null
	/// </summary>
	/// <param name="uuid"></param>
	/// <param name="symbol"></param>
	/// <param name="matchesAny">If true, the finite state machine will match any value</param>
	internal FiniteStateMachine(ref int uuid, T? symbol, bool matchesAny = false) {
		End = new State(ref uuid);
		Start = new State(ref uuid);
		if(symbol == null) {
			Start.AddConnection(new StateConnection(End, default, false, default));
		} else {
			Start.AddConnection(new StateConnection(End, symbol ?? default, false, matchesAny));
		}
	}

	private FiniteStateMachine(State start, State end) {
		Start = start;
		End = end;
	}

	/// <summary>
	/// Concatenation: <c>a.Concatenate(b) => ab</c>
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	internal FiniteStateMachine<T> Concatenate(FiniteStateMachine<T> other) {
		this.End.AddEpsilonConnection(other.Start);
		return new FiniteStateMachine<T>(this.Start, other.End);
	}

	/// <summary>
	/// Alternation: <c>a.Alternate(b) => a|b</c>
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	internal FiniteStateMachine<T> Alternate(ref int uuid, FiniteStateMachine<T> other) {
		// Slight optimisation: Use this.Start instead of creating new start state. Should be functionally identical
		// TODO: Could optimise out the endState too actually and just have the end state be the end state of `this` or `other`
		var endState = new State(ref uuid);

		this.Start.AddEpsilonConnection(other.Start);
		
		this.End.AddEpsilonConnection(endState);
		other.End.AddEpsilonConnection(endState);
		
		return new FiniteStateMachine<T>(this.Start, endState);
	}

	/// <summary>
	/// Empty Alternation: <c>a.AlternateEmpty() => a?</c>
	/// </summary>
	/// <returns></returns>
	internal FiniteStateMachine<T> AlternateEmpty() {
		this.Start.AddEpsilonConnection(this.End);
		
		return this;
	}

	/// <summary>
	/// Loop Alternation: <c>a.AlternateLoop() => a*</c>
	/// </summary>
	/// <returns></returns>
	internal FiniteStateMachine<T> AlternateLoop() {
		this.Start.AddEpsilonConnection(this.End);
		this.End.AddEpsilonConnection(this.Start);
		
		return this;
	}

	/// <summary>
	/// Once-Mandatory Loop Alternation: <c>a.AlternateLoopOnce() => a+</c>
	/// </summary>
	/// <returns></returns>
	internal FiniteStateMachine<T> AlternateLoopOnce() {
		this.End.AddEpsilonConnection(this.Start);
		
		return this;
	}

#if DEBUG
	/// <summary>
	/// Prints out the structure of the NFA, along with it's start and end states. The printed-out structure is valid mermaid diagram markup for easy visualisation
	/// </summary>
	private void Debug() {
		Console.WriteLine(" --- NFA DEBUG START --- ");
		
		Console.WriteLine($"Start: {Start}");
		Console.WriteLine($"End: {End}");
		
		Stack<State> toVisit = new();
		HashSet<State> visited = new();

		toVisit.Push(Start);

		while(toVisit.Count != 0) {
			State state = toVisit.Pop();
			visited.Add(state);
			for(int i = 0; i < state.Connections.Count; i++) {
				DebugPrintConnection(state, state.Connections[i]);
				if(!visited.Contains(state.Connections[i].Next)) {
					toVisit.Push(state.Connections[i].Next);
					visited.Add(state.Connections[i].Next);
				}
			}
		}
		
		Console.WriteLine(" --- NFA DEBUG END --- ");
	}

	private void DebugPrintConnection(State start, StateConnection conn) {
		// Console.WriteLine($"{start} -> ${conn.Next} : Transparent({conn.Transparent}), Symbol({conn.Symbol})");
		string connStr;
		if(conn.Transparent) {
			connStr = "ε";
		} else if(conn.Wildcard) {
			connStr = ".";
		} else {
			connStr = conn.Symbol.ToString() ?? "ERROR";
		}
		Console.WriteLine($"{start} -->|\"{connStr}\"| {conn.Next}");
	}
#endif
}