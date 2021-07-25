using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace HookListPerformance
{
	static partial class Program
    {
		static long nanosecPerTick;
        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)1;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            if (Stopwatch.IsHighResolution)
                Console.WriteLine("Operations timed using the system's high-resolution performance counter.");
            else
                Console.WriteLine("Operations timed using the DateTime class.");

            long frequency = Stopwatch.Frequency;
            nanosecPerTick = (1000L * 1000L * 1000L) / frequency;
            Console.WriteLine("\t{0} ns / tick", nanosecPerTick);

            var total = new Stopwatch();
            total.Start();
            RunTest();
            Console.WriteLine("Took {0}ms", total.ElapsedTicks * 1000 / frequency);
        }

        static IEnumerable<int> Select(int n, int from, Random rnd) =>
            Enumerable.Range(0, from)
                .Select(x => new { Rnd = rnd.Next(), Item = x })
                .OrderBy(x => x.Rnd)
                .Take(n)
                .Select(x => x.Item)
                .OrderBy(x => x);

        static T[] Select<T>(int n, T[] from, Random rnd) =>
            Select(n, from.Length, rnd)
                .Select(i => from[i])
                .ToArray();

        static void RunTest()
        {
            Random rnd = new Random(0);
            // 100 globals total
            GlobalItem[] globals = Enumerable.Range(0, 100).Select(i => new GlobalItem { index = i, rng = (float)rnd.NextDouble() }).ToArray();
            // 10000 different items, each with 20 globals
            Item[] items = Enumerable.Range(0, 10000).Select(_ => new Item(Select(20, globals, rnd))).ToArray();

            // a random 20 global items implement this hook.
            int[] hookInds = Select(20, globals.Length, rnd).ToArray();

			var impls = new Implementation[] {
				new HandCoded(),
				new HandCodedUnpacked(),
				new HandCodedSpan(),
				new HandCodedVirtual(),
				new HandCodedNoInstStruct(),
				new StructEnumerator(),
				new StructEnumeratorUnpacked(),
				new StructEnumeratorSpan(),
				new StructEnumerator5Fields(),
				new StructEnumeratorInstancedField(),
				new Lambda(),
				new YieldEnumerator(),
			};

			// Call each hook on each item 2k times
			const int iterations = 2000;
			var times = impls.Select(_ => new long[iterations]).ToArray();

            for (int i = 0; i < iterations; i++)
            {
                float input = (float)rnd.NextDouble();
				float resultThisIteration = 0;
				for (int impl = 0; impl < impls.Length; impl++) {
					var inputCopy = input; //all impls get teh same input

					long start = Stopwatch.GetTimestamp();
					var result = impls[impl].HookDoEffect(input, items, hookInds);
					times[impl][i] = (Stopwatch.GetTimestamp() - start) * nanosecPerTick;

					if (impl == 0) {
						resultThisIteration = result;
					} else if (result != resultThisIteration) {
						throw new Exception("Test implementation has a bug");
					}
				}
            }

			var headings = impls.Select(impl => impl.GetType().Name).ToArray();
			var implPerfTimes = times.Select(ts => ts.Skip(iterations / 2).Average() / items.Length).ToArray();

			for (int impl = 0; impl < impls.Length; impl++) {
				var perf = implPerfTimes[impl];
				var s = $"{headings[impl],-34}{perf:0}ns/item";
				if (impl > 0) {
					var basePerf = implPerfTimes[0];
					s += $"{(perf-basePerf)/perf,8:+0.0%;-0.0%}";
				}
				Console.WriteLine(s);
			}

			var path = "results.csv";

			using StreamWriter writer = new StreamWriter(path);
			writer.WriteLine(string.Join(',', headings));
			for (var i = 0; i < iterations; i++)
				writer.WriteLine(string.Join(',', Enumerable.Range(0, impls.Length).Select(j => times[j][i])));
			

			Console.WriteLine("Wrote CSV: " + path);
		}
    }
}