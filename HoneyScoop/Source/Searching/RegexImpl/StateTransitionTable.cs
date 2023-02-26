namespace HoneyScoop.Searching.RegexImpl; 

internal class StateTransitionTable {
	internal readonly struct State {
		internal readonly uint Id;

		internal State(ref uint id) {
			Id = id;
			id++;
		}

		internal bool Equals(State other) {
			return Id == other.Id;
		}

		public override bool Equals(object? obj) {
			return obj is State other && Equals(other);
		}

		public override int GetHashCode() {
			return (int)Id;
		}

		public override string ToString() {
			return $"{nameof(Id)}({Id})";
		}
	}

	internal readonly struct StateTransition {
		internal readonly byte TransitionValue;
		internal readonly bool IsEpsilon;
		internal readonly State StartState;
		internal readonly State EndState;

		internal StateTransition(State start, State end, byte transitionValue) {
			TransitionValue = transitionValue;
			IsEpsilon = false;
			StartState = start;
			EndState = end;
		}

		internal StateTransition(State start, State end) {
			TransitionValue = 0;
			IsEpsilon = true;
			StartState = start;
			EndState = end;
		}

		public override string ToString() {
			return $"{nameof(TransitionValue)}({TransitionValue}), {nameof(IsEpsilon)}({IsEpsilon}), {nameof(StartState)}({StartState}, {nameof(EndState)}({EndState})";
		}
	}
	
	/// <summary>
	/// The underlying table mapping States to transitions to other States
	/// </summary>
	internal Dictionary<State, List<StateTransition>> Table = new();
	internal State Start;
	internal State End;

	internal void AddTransition(StateTransition trans) {
		if(!Table.ContainsKey(trans.StartState)) {
			Table.Add(trans.StartState, new List<StateTransition>());
		}
		Table[trans.StartState].Add(trans);
	}
	
	internal void AddTransition(State from, State to, byte symbol) {
		if(!Table.ContainsKey(from)) {
			Table.Add(from, new List<StateTransition>());
		}
		Table[from].Add(new StateTransition(from, to, symbol));
	}

	internal void AddEpsilonTransition(State from, State to) {
		if(!Table.ContainsKey(from)) {
			Table.Add(from, new List<StateTransition>());
		}
		Table[from].Add(new StateTransition(from, to));
	}

	/// <summary>
	/// Takes a postfix list of tokens and outputs a State Transition Table that represents it
	/// </summary>
	/// <param name="tokens"></param>
	/// <returns></returns>
	internal static StateTransitionTable Build(List<RegexLexer.Token> tokens) {
		var stt = new StateTransitionTable();
		uint idCurr = 0;

		// var initState = new State(ref idCurr);
		// var finalState = new State(ref idCurr);
		
		var stateTransitions = new Stack<StateTransition>();
		// stateTransitions.Push(new StateTransition(initState, finalState));
		// stt.Start = initState;
		// stt.End = finalState;

		for(int i = 0; i < tokens.Count; i++) {
			switch(tokens[i].Type) {
				case RegexLexer.TokenType.Literal: {
					var start = new State(ref idCurr);
					var end = new State(ref idCurr);

					var trans = new StateTransition(start, end, tokens[i].LiteralValue);
					
					stateTransitions.Push(trans);
					stt.AddTransition(trans);

					break;
				}

				case RegexLexer.TokenType.UnaryOperator:
				case RegexLexer.TokenType.BinaryOperator: {
					switch(tokens[i].OpType) {
						case RegexLexer.OperatorType.Concat:
							StateTransition rhs = stateTransitions.Pop();
							StateTransition lhs = stateTransitions.Pop();
							var newTrans = new StateTransition(lhs.EndState, rhs.StartState);
							stateTransitions.Push(newTrans);
							stt.AddTransition(newTrans);
							break;
					}
					break;
				}
			}
		}

		// stt.AddTransition(stateTransitions.Peek());
		StateTransition overall = stateTransitions.Peek();
		stt.Start = overall.StartState;
		stt.End = overall.EndState;
		return stt;
	}
	
	public override string ToString() {
		string dictStr = string.Join(", ", Table.Select(
			kvp => {
				string listStr = string.Join(", ", kvp.Value.Select(val => $"({val.ToString()})"));
				return $"\n\t(({kvp.Key}): [{listStr}])";
			}));
		return $"{nameof(Table)}{{{dictStr}\n}}, {nameof(Start)}({Start}), {nameof(End)}({End})";
	}
}
