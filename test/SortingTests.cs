using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Exceptions;

namespace Terraria.ModLoader
{
	[TestClass]
	public class SortingTests
	{

		[ClassInitialize]
		public static void ClassInit(TestContext context) {
			LanguageManager.Instance.SetLanguage("en-US");
		}

		private static LocalMod Make(string name, 
			ModSide side = ModSide.Both, string version = null,
			IEnumerable<string> refs = null, IEnumerable<string> weakRefs = null,
			IEnumerable<string> sortAfter = null, IEnumerable<string> sortBefore = null) {
			return new LocalMod(ModLocation.Local, new TmodFile(null, name), new BuildProperties {
				side = side,
				version = new Version(version ?? "1.0.0.0"),
				modReferences = refs?.Select(BuildProperties.ModReference.Parse).ToArray() ?? new BuildProperties.ModReference[0],
				weakReferences = weakRefs?.Select(BuildProperties.ModReference.Parse).ToArray() ?? new BuildProperties.ModReference[0],
				sortAfter = sortAfter?.ToArray() ?? new string[0],
				sortBefore = sortBefore?.ToArray() ?? new string[0]
			});
		}

		private static void AssertSetsEqual<T>(ICollection<T> set1, ICollection<T> set2) {
			var set = new HashSet<T>(set1);
			Assert.AreEqual(set1.Count, set2.Count);
			foreach (var e in set2)
				Assert.IsTrue(set.Contains(e), "Missing Element: " + set2);
		}

		private static void AssertModException(Action func, string[] mods, string msg) {
			try {
				func();
				Assert.Fail("Test method did not throw expected exception ModSortingException.");
			}
			catch (ModSortingException e) {
				AssertSetsEqual(e.errored.Select(m => m.Name).ToList(), mods);
				Assert.AreEqual(msg, e.Message.Trim());
			}
		}

		private static void Swap<T>(ref T a, ref T b) {
			var tmp = a;
			a = b;
			b = tmp;
		}

		private static void Reverse<T>(T[] arr, int pos, int len) {
			int end = pos + len - 1;
			for (int i = 0; i < len / 2; i++)
				Swap(ref arr[pos + i], ref arr[end - i]);
		}

		private static IEnumerable<int[]> Permutations(int n) {
			var arr = Enumerable.Range(0, n).ToArray();
			var b = Enumerable.Range(0, n).ToArray();
			var c = Enumerable.Repeat(0, n).ToArray();
			while (true) {
				yield return arr;

				int k = 1;
				while (c[k] == k) {
					c[k++] = 0;
					if (k == n) yield break;
				}
				c[k]++;
				Reverse(b, 1, k-1);
				Swap(ref arr[0], ref arr[b[k]]);
			}
		}

		private static List<LocalMod> AssertSortSatisfied(List<LocalMod> list) {
			var sorted = ModOrganizer.Sort(list);
			var indexMap = sorted.ToDictionary(m => m.Name, sorted.IndexOf);
			foreach (var mod in list) {
				int index = indexMap[mod.Name];
				foreach (var dep in mod.properties.sortAfter) {
					int i;
					if (indexMap.TryGetValue(dep, out i) && i > index)
						Assert.Fail(mod.Name + " sorted after " + dep);
				}
				foreach (var dep in mod.properties.sortBefore) {
					int i;
					if (indexMap.TryGetValue(dep, out i) && i < index)
						Assert.Fail(mod.Name + " sorted before " + dep);
				}
			}

			return sorted;
		}

		/*private static void AssertSortNameIndependent(List<LoadingMod> list) {
			var base_perm = list.Select(m => m.Name).ToList();
			var nameToIndex = base_perm.ToDictionary(m => m, base_perm.IndexOf);
			foreach (var perm in Permutations(list.Count)) {
				foreach (var m in list) {
					m.modFile.name = base_perm[perm[nameToIndex]]
				}
			}
		}*/

		//test missing dependencies
		[TestMethod]
		public void TestDependenciesExist() {
			//test A -> B
			var list1 = new List<LocalMod> {
				Make("A", refs: new[] {"B"}),
				Make("B"),
			};
			ModOrganizer.EnsureDependenciesExist(list1, false);

			//test A -> B (missing)
			var list2 = new List<LocalMod> {
				Make("A", refs: new[] {"B"})
			};
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list2, false),
				new[] {"A"},
				"Missing mod: B required by A");

			//test multi reference
			var list3 = new List<LocalMod> {
				Make("A", refs: new[] {"B"}),
				Make("B"),
				Make("C", refs: new[] {"A"})
			};
			ModOrganizer.EnsureDependenciesExist(list3, false);

			//test one missing reference
			var list4 = new List<LocalMod> {
				Make("A", refs: new[] {"B"}),
				Make("B", refs: new[] {"C"})
			};
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list4, false),
				new[] {"B"},
				"Missing mod: C required by B");

			//test weak reference (missing)
			var list5 = new List<LocalMod> {
				Make("A", weakRefs: new[] {"B"})
			};
			ModOrganizer.EnsureDependenciesExist(list5, false);
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list5, true),
				new[] {"A"},
				"Missing mod: B required by A");

			//test weak reference (found)
			var list6 = new List<LocalMod> {
				Make("A", weakRefs: new[] {"B"}),
				Make("B")
			};
			ModOrganizer.EnsureDependenciesExist(list6, true);

			//test strong (found) and weak (missing)
			var list7 = new List<LocalMod> {
				Make("A", refs: new[] {"B"}),
				Make("B", weakRefs: new[] {"C"})
			};
			ModOrganizer.EnsureDependenciesExist(list7, false);
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list7, true),
				new[] {"B"},
				"Missing mod: C required by B");

			//multi test case (missing)
			var list8 = new List<LocalMod> {
				Make("A", refs: new[] {"X"}),
				Make("B", refs: new[] {"Y"}),
				Make("C", refs: new[] {"D"}),
				Make("D", weakRefs: new[] {"E"}),
				Make("E", weakRefs: new[] {"Z"}),
				Make("F", weakRefs: new[] {"Z"})
			};
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list8, false),
				new[] {"A", "B"},
				"Missing mod: X required by A\r\n" +
				"Missing mod: Y required by B");
			AssertModException(
				() => ModOrganizer.EnsureDependenciesExist(list8, true),
				new[] {"A", "B", "E", "F"},
				"Missing mod: X required by A\r\n" +
				"Missing mod: Y required by B\r\n" +
				"Missing mod: Z required by E\r\n" +
				"Missing mod: Z required by F");

			//multi test case (found)
			var list9 = new List<LocalMod> {
				Make("A", refs: new[] {"C"}),
				Make("B", refs: new[] {"C"}),
				Make("C", refs: new[] {"D"}),
				Make("D", weakRefs: new[] {"E"}),
				Make("E", weakRefs: new[] {"F"}),
				Make("F")
			};
			ModOrganizer.EnsureDependenciesExist(list9, false);
			ModOrganizer.EnsureDependenciesExist(list9, true);
		}

		//test missing dependencies
		[TestMethod]
		public void TestVersionRequirements() {
			//test version on missing mod
			var list1 = new List<LocalMod> {
				Make("A", refs: new[] {"B@1.2"})
			};
			ModOrganizer.EnsureTargetVersionsMet(list1);

			//test passed version check
			var list2 = new List<LocalMod> {
				Make("A", refs: new[] {"B@1.2"}),
				Make("B", version: "1.2")
			};
			ModOrganizer.EnsureTargetVersionsMet(list2);

			//test failed version check
			var list3 = new List<LocalMod> {
				Make("A", refs: new[] {"B@1.2"}),
				Make("B")
			};
			AssertModException(
				() => ModOrganizer.EnsureTargetVersionsMet(list3),
				new[] { "A" },
				"A requires B version 1.2 or greater but version 1.0.0.0 is installed");

			// test major version mismatch
			var list3B = new List<LocalMod> {
				Make("A", refs: new[] {"B@0.9"}),
				Make("B")
			};
			AssertModException(
				() => ModOrganizer.EnsureTargetVersionsMet(list3B),
				new[] { "A" },
				"A targets B version 0.9 but you have a newer major version (1.0.0.0) which may not be compatible. A must be updated.");

			//test one pass, two fail version check
			var list4 = new List<LocalMod> {
				Make("A", version: "1.1"),
				Make("B", refs: new[] {"A@1.0"}),
				Make("C", refs: new[] {"A@1.2"}),
				Make("D", refs: new[] {"A@1.1.0.1"})
			};
			AssertModException(
				() => ModOrganizer.EnsureTargetVersionsMet(list4),
				new[] { "C", "D" },
				"C requires A version 1.2 or greater but version 1.1 is installed\r\n" +
				"D requires A version 1.1.0.1 or greater but version 1.1 is installed");
			
			//test weak version check (missing)
			var list5 = new List<LocalMod> {
				Make("A", weakRefs: new[] {"B@1.1"})
			};
			ModOrganizer.EnsureDependenciesExist(list5, false);
			ModOrganizer.EnsureTargetVersionsMet(list5);

			//test weak version check (too low)
			var list6 = new List<LocalMod> {
				Make("A", weakRefs: new[] {"B@1.1"}),
				Make("B")
			};
			AssertModException(
				() => ModOrganizer.EnsureTargetVersionsMet(list6),
				new[] { "A" },
				"A requires B version 1.1 or greater but version 1.0.0.0 is installed");
		}

		[TestMethod]
		public void TestSortOrder() {
			//general complex one way edge sort
			var list = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new [] {"A"}),
				Make("C", sortAfter: new [] {"A"}, sortBefore: new[] {"B"}),
				Make("D", sortAfter: new [] {"H"}),
				Make("E", sortAfter: new [] {"C"}),
				Make("F", sortBefore: new [] {"G"}),
				Make("G", sortAfter: new [] {"B", "C"}),
				Make("H", sortAfter: new [] {"G"}, sortBefore: new [] {"D"}),
			};
			AssertSortSatisfied(list);

			//mutually satisfiable cycle
			var list2 = new List<LocalMod> {
				Make("A", sortBefore: new [] {"B"}),
				Make("B", sortAfter: new [] {"A"})
			};
			AssertSortSatisfied(list2);

			//direct cycle
			var list3 = new List<LocalMod> {
				Make("A", sortAfter: new [] {"B"}),
				Make("B", sortAfter: new [] {"A"})
			};
			AssertModException(
				() => AssertSortSatisfied(list3),
				new[] { "A", "B" },
				"Dependency Cycle: A -> B -> A");

			//complex unsatisfiable sort
			var list4 = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new [] {"A"}),
				Make("C", sortBefore: new [] {"A", "D"}),
				Make("D", sortAfter: new [] {"I"}),
				Make("E", sortAfter: new [] {"C"}),
				Make("F", sortBefore: new [] {"A, I"}),
				Make("G", sortAfter: new [] {"B", "C"}),
				Make("H", sortBefore: new [] {"G", "F"}),
				Make("I", sortAfter: new [] {"G", "H"}, sortBefore: new[] {"C"})
			};
			AssertModException(
				() => AssertSortSatisfied(list4),
				new[] { "A", "B", "C", "G", "I" },
				"Dependency Cycle: A -> C -> I -> G -> B -> A\r\n" +
				"Dependency Cycle: C -> I -> G -> C");
		}

		[TestMethod]
		public void TestSidedSorts() {
			//basic B is a client mod
			var list = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"B"})
			};
			AssertModException(
				() => AssertSortSatisfied(list),
				new[] { "C" },
				"C indirectly depends on A via C -> B -> A\r\n"+
				"Some of these mods may not exist on both client and server. Add a direct sort entries or weak references.");
			
			//apply above advice
			var list2 = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"B", "A"})
			};
			AssertSortSatisfied(list2);

			//diamond pattern
			var list3 = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("D", sortAfter: new[] {"C", "B"}, side: ModSide.Client),
				Make("E", sortAfter: new[] {"D"})
			};
			AssertModException(
				() => AssertSortSatisfied(list3),
				new[] { "E" },
				"E indirectly depends on A via E -> D -> C -> A\r\n" +
				"E indirectly depends on A via E -> D -> B -> A\r\n" +
				"Some of these mods may not exist on both client and server. Add a direct sort entries or weak references.");

			//diamond pattern (fixed)
			var list4 = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("D", sortAfter: new[] {"C", "B"}, side: ModSide.Client),
				Make("E", sortAfter: new[] {"D", "A"})
			};
			AssertSortSatisfied(list4);
		}

		[TestMethod]
		public void TestSidedSortsMatch() {
			//diamond pattern
			var list1 = new List<LocalMod> {
				Make("A"),
				Make("B", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"A"}, side: ModSide.Client),
				Make("D", sortAfter: new[] {"C", "B"}, side: ModSide.Client),
				Make("E", sortAfter: new[] {"D", "A"})
			};
			var s1 = AssertSortSatisfied(list1).Where(m => m.properties.side == ModSide.Both).ToList();
			var s2 = AssertSortSatisfied(list1.Where(m => m.properties.side == ModSide.Both).ToList());
			Assert.IsTrue(Enumerable.SequenceEqual(s1, s2));

			//reverse the order
			var list2 = new List<LocalMod> {
				Make("E"),
				Make("D", sortAfter: new[] {"E"}, side: ModSide.Client),
				Make("C", sortAfter: new[] {"E"}, side: ModSide.Client),
				Make("B", sortAfter: new[] {"C", "D"}, side: ModSide.Client),
				Make("A", sortAfter: new[] {"B", "E"})
			};
			s1 = AssertSortSatisfied(list2).Where(m => m.properties.side == ModSide.Both).ToList();
			s2 = AssertSortSatisfied(list2.Where(m => m.properties.side == ModSide.Both).ToList());
			Assert.IsTrue(Enumerable.SequenceEqual(s1, s2));

			//mostly independent sort with random client only before/afters
			var list3 = new List<LocalMod> {
				Make("A"),
				Make("B", ModSide.Client, sortBefore: new [] {"A"}),
				Make("C"),
				Make("D", ModSide.Client, sortAfter: new [] {"F"}),
				Make("E", sortAfter: new [] {"G"}),
				Make("F", ModSide.Client, sortAfter: new [] {"E, G"}),
				Make("G"),
				Make("H"),
			};
			s1 = AssertSortSatisfied(list3).Where(m => m.properties.side == ModSide.Both).ToList();
			s2 = AssertSortSatisfied(list3.Where(m => m.properties.side == ModSide.Both).ToList());
			Assert.IsTrue(Enumerable.SequenceEqual(s1, s2));
		}
	}
}
