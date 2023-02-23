namespace HoneyScoop.Searching.RegexImpl;

// Regex with Finite Automata (Finite State Machines): https://deniskyashif.com/2019/02/17/implementing-a-regular-expression-engine/
//                                                     https://swtch.com/~rsc/regexp/regexp1.html

/// <summary>
/// Also known as NFA (Nondeterministic Finite Automata) and referred to as such in the codebase
/// </summary>
/// <typeparam name="T">The type of data that the NFA is matching</typeparam>
internal readonly struct FiniteStateMachine<T> where T: struct { // TODO: Make this capable of matching the '.' metacharacter. Hang on you can just do "a?" right? no?
	// TODO: Restructure into a State Transition Table (STT) (if appropriate which I think it is). Try and keep the API similar to how it currently is without harming the efficiency of the implementation of the STT. The API might require some changes lmao
	/// <summary>
	/// Reference type so that instances of State can be referenced all over
	/// <br /><br />
	/// A State object represents a state in a Finite State Machine, that has connections to other states represented by a tuple (State, T?)
	/// where the State is the connected state and the T? is the connection character or a null represents an ε-connection (empty/transparent connection)
	/// </summary>
	internal class State { // TODO: This is kinda expensive to construct - Maybe add a constructor that doesn't then also initialise `connections`
		private List<StateConnection> _connections; // TODO: Array or List?
		
		internal bool IsEnd => _connections.Count == 0;
		
		/// <summary>
		/// Specify a capacity to preallocate when creating the <c>List&lt;StateConnection&gt;</c> of connections (for optimisation purposes)
		/// </summary>
		/// <param name="preallocCapacity">Pre-allocated capacity of the internal list of connections</param>
		internal State(int preallocCapacity = 4) {
			this._connections = new List<StateConnection>(preallocCapacity);
		}

		internal State AddConnection(StateConnection connection) {
			this._connections.Add(connection);
			return this;
		}

		internal State AddEpsilonConnection(State other) {
			this._connections.Add(new StateConnection(other, default, true));
			return this;
		}
		
		internal State AddSymbolConnection(State other, T symbol) {
			this._connections.Add(new StateConnection(other, symbol, false));
			return this;
		}

		internal List<StateConnection> Connections() {
			return _connections;
		}
	}

	/// <summary>
	/// A StateConnection object represents a directional connection from one State to another - The <c>Next</c> state.
	/// If the connection is a ε-connection, <c>Transparent</c> is true, otherwise, <c>Symbol</c> is used
	/// </summary>
	internal readonly record struct StateConnection(State Next, T Symbol, bool Transparent);

	private readonly State _start;
	private readonly State _end;
	
	internal FiniteStateMachine(T? symbol) {
		_end = new State();
		_start = new State().AddConnection(new StateConnection(_end, symbol ?? default(T), !symbol.HasValue));
	}

	private FiniteStateMachine(State start, State end) {
		_start = start;
		_end = end;
	}

	/// <summary>
	/// Concatenation: <c>a.Concatenate(b) => ab</c>
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	internal FiniteStateMachine<T> Concatenate(FiniteStateMachine<T> other) {
		this._end.AddConnection(new StateConnection(other._start, default, true));
		return new FiniteStateMachine<T>(this._start, other._end);
	}

	/// <summary>
	/// Alternation: <c>a.Alternate(b) => a|b</c>
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	internal FiniteStateMachine<T> Alternate(FiniteStateMachine<T> other) {
		// Slight optimisation: Use this.Start instead of creating new start state. Should be functionally identical
		// TODO: Could optimise out the endState too actually and just have the end state be the end state of `this` or `other`
		var endState = new State();

		this._start.AddEpsilonConnection(other._start);
		
		this._end.AddEpsilonConnection(endState);
		other._end.AddEpsilonConnection(endState);
		
		return new FiniteStateMachine<T>(this._start, endState);
	}

	/// <summary>
	/// Empty Alternation: <c>a.AlternateEmpty() => a?</c>
	/// </summary>
	/// <returns></returns>
	internal FiniteStateMachine<T> AlternateEmpty() {
		this._start.AddEpsilonConnection(this._end);
		
		return this;
	}

	/// <summary>
	/// Loop Alternation: <c>a.AlternateLoop() => a*</c>
	/// </summary>
	/// <returns></returns>
	internal FiniteStateMachine<T> AlternateLoop() {
		// TODO: Test - This is different to implementations I've come across in research - But as far as I can think it is functionally identical?
		this._start.AddEpsilonConnection(this._end);
		this._end.AddEpsilonConnection(this._start);
		
		return this;
	}

	/// <summary>
	/// Once-Mandatory Loop Alternation: <c>a.AlternateLoopOnce() => a+</c>
	/// </summary>
	/// <returns></returns>
	internal FiniteStateMachine<T> AlternateLoopOnce() {
		this._end.AddEpsilonConnection(this._start);
		
		return this;
	}

	/// <summary>
	/// Ah shit this is actually quite complex... Deep object comparison
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	internal bool Equals(FiniteStateMachine<T> other) {
		// return Equals(Start, other.Start) && Equals(End, other.End);

		return false;
	}

	internal bool CompareState(State state) {
		var conns = state.Connections();
		
		return false;
	}

	public override bool Equals(object? obj) {
		return obj is FiniteStateMachine<T> other && Equals(other);
	}
}