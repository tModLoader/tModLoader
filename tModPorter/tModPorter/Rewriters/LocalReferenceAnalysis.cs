using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace tModPorter.Rewriters
{
	using BlockState = Dictionary<ILocalSymbol, LocalReferenceAnalysis.Ref<LocalReferenceAnalysis.Node>>;

	public class LocalReferenceAnalysis
	{
		internal class Ref<T>
		{
			public T Value;
		}

		internal class Node
		{
			IAssignmentOperation value;
			ImmutableArray<Ref<Node>> preds;

			public Node(IAssignmentOperation value) { this.value = value; }
			public Node(ImmutableArray<Ref<Node>> preds) { this.preds = preds; }

			public ImmutableArray<IAssignmentOperation> GetValues() => GetValues(new()).Distinct().ToImmutableArray();

			public ImmutableArray<IAssignmentOperation> GetValues(Stack<Node> stack) {
				if (stack.Contains(this))
					return ImmutableArray<IAssignmentOperation>.Empty;

				if (value != null)
					return ImmutableArray.Create(value);

				try {
					stack.Push(this);
					return preds.SelectMany(p => p.Value.GetValues(stack)).ToImmutableArray();
				}
				finally {
					stack.Pop();
				}
			}
		}

		internal static ImmutableDictionary<SyntaxNode, ImmutableArray<IAssignmentOperation>> Analyze(ControlFlowGraph cfg) {
			var reads = new List<(SyntaxNode syntax, Node node)>();
			var blockStates = cfg.Blocks.ToDictionary(b => b, b => new BlockState(SymbolEqualityComparer.Default));

			Ref<Node> GetNode(BasicBlock block, ILocalSymbol local) {
				var state = blockStates[block];

				if (!state.TryGetValue(local, out var r)) {
					state[local] = r = new();
					r.Value = new Node(block.Predecessors.Select(p => GetNode(p.Source, local)).ToImmutableArray());
				}

				return r;
			}

			foreach (var block in cfg.Blocks) {
				void Visit(IOperation op) {
					switch (op) {
						case IAssignmentOperation assign when assign is { Target: ILocalReferenceOperation { Local: var local }, Value: var value }:
							Visit(value);
							GetNode(block, local).Value = new Node(assign);
							break;
						case ILocalReferenceOperation { IsDeclaration: false, Local: var local, Syntax: var syntax }:
							reads.Add((syntax, GetNode(block, local).Value));
							break;
						default:
							foreach (var child in op.ChildOperations)
								Visit(child);
							break;
					}
				}

				foreach (var op in block.Operations) {
					Visit(op);
				}
			}

			return reads.ToImmutableDictionary(e => e.syntax, e => e.node.GetValues());
		}
	}
}