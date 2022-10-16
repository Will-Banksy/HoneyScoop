// My conventions TODO Revisit
// PascalCase for classes/structs/records and functions, delegates, namespaces and public member variables
// camelCase for private member variables, method parameters, local variables
// Prefix interfaces with I

// NOTE: For performance-sensitive areas of code, and honestly in general:
//           Use direct assignment instead of properties
//           Prefer structs over classes when a lot are being created/used
//           Always use stringbuilder when doing much string concatenation
//           Use for loops instead of foreach
//           Don't use LINQ
//           Remember memory locality
//           Don't create reference objects unless they are really needed (creating reference objects is expensive and gives the GC work)

// Regex with Finite Automata (Finite State Machines): https://deniskyashif.com/2019/02/17/implementing-a-regular-expression-engine/
//                                                     https://swtch.com/~rsc/regexp/regexp1.html

// NOTE: Use Span for slices (https://learn.microsoft.com/en-us/dotnet/api/system.span-1?view=netcore-3.0)

namespace HoneyScoop {
	class MainClass {
		public static void Main(string[] args) {
			// Handle arguments, create HoneyScoop instances
		}
	}
}
