using System.Net.Mail;

namespace HoneyScoop.Searching; 

public struct FiniteStateMachine { // TODO: Make this capable of matching the '.' metacharacter
	/// <summary>
	/// Reference type so that instances of State can be referenced all over
	/// <br /><br />
	/// A State object represents a state in a Finite State Machine, that has connections to other states represented by a tuple (State, byte?)
	/// where the State is the connected state and the byte? is the connection character or a null represents an ε-connection (empty/transparent connection)
	/// </summary>
	public class State {
		private List<StateConnection> connections; // TODO: Array or List?
		
		public bool IsEnd => connections.Count == 0;
		
		/// <summary>
		/// Specify a capacity to preallocate when creating the <c>List&lt;StateConnection&gt;</c> of connections (for optimisation purposes)
		/// </summary>
		/// <param name="preallocCapacity"></param>
		public State(int preallocCapacity = 4) {
			this.connections = new List<StateConnection>(preallocCapacity);
		}

		public State AddConnection(StateConnection connection) {
			this.connections.Add(connection);
			return this;
		}

		public State AddEpsilonConnection(State other) {
			this.connections.Add(new StateConnection(other, 0, true));
			return this;
		}
		
		public State AddSymbolConnection(State other, byte symbol) {
			this.connections.Add(new StateConnection(other, symbol, false));
			return this;
		}
	}

	/// <summary>
	/// A StateConnection object represents a directional connection from one State to another - The <c>Next</c> state.
	/// If the connection is a ε-connection, <c>Transparent</c> is true, otherwise, <c>Symbol</c> is used
	/// </summary>
	public readonly record struct StateConnection(State Next, byte Symbol, bool Transparent) {}

	public readonly State Start;
	public readonly State End;
	
	public FiniteStateMachine(byte? symbol) {
		End = new State();
		Start = new State().AddConnection(new StateConnection(End, symbol ?? 0, !symbol.HasValue));
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
	private FiniteStateMachine Concatenate(FiniteStateMachine other) {
		this.End.AddConnection(new StateConnection(other.Start, 0, true));
		return new FiniteStateMachine(this.Start, other.End);
	}

	/// <summary>
	/// Alternation: <c>a.Alternate(b) => a|b</c>
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	private FiniteStateMachine Alternate(FiniteStateMachine other) {
		// Slight optimisation: Use this.Start instead of creating new start state. Should be functionally identical
		// TODO: Could optimise out the endState too actually and just have the end state be the end state of `this` or `other`
		var endState = new State();

		this.Start.AddEpsilonConnection(other.Start);
		
		this.End.AddEpsilonConnection(endState);
		other.End.AddEpsilonConnection(endState);
		
		return new FiniteStateMachine(this.Start, endState);
	}

	/// <summary>
	/// Empty Alternation: <c>a.AlternateEmpty() => a?</c>
	/// </summary>
	/// <returns></returns>
	private FiniteStateMachine AlternateEmpty() {
		this.Start.AddEpsilonConnection(this.End);
		
		return this;
	}

	/// <summary>
	/// Loop Alternation: <c>a.AlternateLoop() => a*</c>
	/// </summary>
	/// <returns></returns>
	private FiniteStateMachine AlternateLoop() {
		// TODO: Test - This is different to implementations I've come across in research - But as far as I can think it is functionally identical?
		this.Start.AddEpsilonConnection(this.End);
		this.End.AddEpsilonConnection(this.Start);
		
		return this;
	}

	/// <summary>
	/// Once-Mandatory Loop Alternation: <c>a.AlternateLoopOnce() => a+</c>
	/// </summary>
	/// <returns></returns>
	private FiniteStateMachine AlternateLoopOnce() {
		this.End.AddEpsilonConnection(this.Start);
		
		return this;
	}
}