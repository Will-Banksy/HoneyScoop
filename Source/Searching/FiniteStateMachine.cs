namespace HoneyScoop.Searching; 

public struct FiniteStateMachine {
	/// <summary>
	/// Reference type so that instances of State can be referenced all over
	/// <br /><br />
	/// A State object represents a state in a Finite State Machine, that has connections to other states represented by a tuple (State, byte?)
	/// where the State is the connected state and the byte? is the connection character or a null represents an ε-connection (empty/transparent connection)
	/// </summary>
	public class State {
		public StateConnection[] Connections; // TODO: Array or List?

		public State(StateConnection[]? connections) {
			this.Connections = connections ?? new StateConnection[]{};
		}
	}

	/// <summary>
	/// A StateConnection object represents a directional connection from one State to another - The <c>Next</c> state.
	/// If the connection is a ε-connection, <c>Transparent</c> is true, otherwise, <c>Symbol</c> is used
	/// </summary>
	public readonly record struct StateConnection(State Next, byte Symbol, bool Transparent) {}

	public readonly State Start;
	public readonly State End; // This will probably need to be mutable
	
	public FiniteStateMachine(byte? symbol) {
		End = new State(null);
		Start = new State(new StateConnection[] {
			new StateConnection(End, symbol ?? 0, !symbol.HasValue)
		});
	}

	private FiniteStateMachine Concat(FiniteStateMachine other) {
		// TODO
		return this;
	}

	private FiniteStateMachine Union(FiniteStateMachine other) {
		// TODO
		return this;
	}

	private FiniteStateMachine Closure() {
		// TODO
		return this;
	}
}