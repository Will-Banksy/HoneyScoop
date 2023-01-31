using System.Collections.Generic;
using HoneyScoop.FileHandling;
using HoneyScoop.Searching.RegexImpl;

namespace HoneyScoop.Searching;

/// <summary>
/// The class that instruments searching a block of data for file signatures
/// </summary>
internal class SignatureSearcher {
	/// <summary>
	/// Cache to speed up constructing the same FiniteStateMachine multiple times
	/// </summary>
	private Dictionary<Signature, FiniteStateMachine<byte>> _signatureCache = new();
}