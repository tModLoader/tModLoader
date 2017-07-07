using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Terraria.ModLoader.Setup
{
	//generate the patch output
	//generate a failure file
	public class Patcher
	{
		public enum Operation
		{
			DELETE, INSERT, EQUAL
		}

		public class Diff
		{
			public Operation op;
			public string text;
			
			public Diff(Operation op, string text) {
				this.op = op;
				this.text = text;
			}

			public override string ToString() => 
				(op == Operation.EQUAL ? ' ' :
				op == Operation.INSERT ? '+' : '-') + text;
		}

		public enum Mode
		{
			EXACT, OFFSET, FUZZY
		}

		public class Patch
		{
			public List<Diff> diffs = new List<Diff>();
			public int start1;
			public int start2;
			public int length1;
			public int length2;

			public Patch() {}

			public Patch(Patch patch) {
				diffs = new List<Diff>(patch.diffs.Select(d => new Diff(d.op, d.text)));
				start1 = patch.start1;
				start2 = patch.start2;
				length1 = patch.length1;
				length2 = patch.length2;
			}

			public string Header => $"@@ -{start1 + 1},{length1} +{start2 + 1},{length2} @@";

			public IEnumerable<string> ContextLines => diffs.Where(d => d.op != Operation.INSERT).Select(d => d.text);
			public IEnumerable<string> PatchedLines => diffs.Where(d => d.op != Operation.DELETE).Select(d => d.text);

			public override string ToString() => Header + Environment.NewLine +
										string.Join(Environment.NewLine, diffs);
		}

		public class Result
		{
			public Patch patch;
			public bool success;
			public Mode mode;

			public Patch appliedPatch;

			public int offset;
			public bool offsetWarning;

			public float fuzzyQuality;
			public Patch fuzzyDiff;
		}

		//patch extended with implementation fields
		private class Patch2 : Patch
		{
			public Result result;
			public string lmContext;
			public string lmPatched;
			public string[] wmContext;
			public string[] wmPatched;

			public void Fail() {
				result = new Result {patch = this, success = false};
			}

			public void SucceedExact(int at) {
				result = new Result {
					patch = this,
					success = true,
					mode = Mode.EXACT,
					appliedPatch = new Patch(this) {
						start1 = start1 + at - start2,
						start2 = at
					}
				};
			}

			public void SucceedOffset(Patcher patcher, int at, int offset) {
				result = new Result {
					patch = this,
					success = true,
					mode = Mode.OFFSET,
					appliedPatch = new Patch(this) {
						start1 = start1 + at - start2,
						start2 = at
					},
					offset = offset,//note that offset is different to at - start2, because offset is relative to the applied position of the last patch
					offsetWarning = offset > OffsetWarnDistance(length1, patcher.textLines.Count)
				};
			}

			public void SucceedFuzzy(Patcher patcher, int offset, float score, Patch appliedPatch, Patch fuzzyDiff) {
				result = new Result {
					patch = this,
					success = true,
					mode = Mode.FUZZY,
					offset = offset,
					offsetWarning = offset > OffsetWarnDistance(length1, patcher.textLines.Count),
					fuzzyQuality = score,
					appliedPatch = appliedPatch,
					fuzzyDiff = fuzzyDiff
				};
			}

			public void LinesToChars(Patcher patcher) {
				lmContext = new string(ContextLines.Select(patcher.AddLine).ToArray());
				lmPatched = new string(PatchedLines.Select(patcher.AddLine).ToArray());
			}

			public void WordsToChars(Patcher patcher) {
				wmContext = ContextLines.Select(patcher.WordsToChars).ToArray();
				wmPatched = PatchedLines.Select(patcher.WordsToChars).ToArray();
			}
		}

		//the offset distance which constitutes a warning for a patch
		//currently either 10% of file length, or 10x patch length, whichever is longer
		public static int OffsetWarnDistance(int patchLength, int fileLength) => Math.Max(patchLength * 10, fileLength / 10);

		private List<Patch2> patches;
		private List<string> textLines;
		private bool applied;

		private List<string> charToLine = new List<string>();
		private Dictionary<string, char> lineToChar = new Dictionary<string, char>();
		private string lmText;

		private int nextWord = 255;
		private Dictionary<string, char> wordToChar = new Dictionary<string, char>();
		private List<string> wmLines;

		public Patcher(string patchText, IEnumerable<string> lines) {
			patches = ParsePatch(patchText);
			textLines = new List<string>(lines);
		}

		private static List<Patch2> ParsePatch(string patchText) {
			var lines = patchText.Split('\n');

			var patches = new List<Patch2>();
			Patch2 patch = null;
			int delta = 0;
			for (int i = 2; i < lines.Length; i++) {
				var line = lines[i].TrimEnd('\r');
				if (line.Length == 0)
					continue;

				switch (line[0]) {
					case '@':
						var m = DiffTask.HunkOffsetRegex.Match(line);
						if (!m.Success)
							throw new ArgumentException($"Invalid patch line {i}:{line}");

						patch = new Patch2 {
							start1 = int.Parse(m.Groups[1].Value) - 1,
							length1 = int.Parse(m.Groups[2].Value),
							length2 = int.Parse(m.Groups[4].Value),
						};
						patch.start2 = patch.start1 + delta;
						delta += patch.length2 - patch.length1;
						patches.Add(patch);
						break;
					case ' ':
						patch.diffs.Add(new Diff(Operation.EQUAL, line.Substring(1)));
						break;
					case '+':
						patch.diffs.Add(new Diff(Operation.INSERT, line.Substring(1)));
						break;
					case '-':
						patch.diffs.Add(new Diff(Operation.DELETE, line.Substring(1)));
						break;
					default:
						throw new ArgumentException($"Invalid patch line {i}:{line}");
				}
			}
			
			//verify patch files
			foreach (var p in patches) {
				if (p.length1 != p.ContextLines.Count())
					throw new ArgumentException($"Context length doesn't match contents: {p.Header}");
				if (p.length2 != p.PatchedLines.Count())
					throw new ArgumentException($"Patched length doesn't match contents: {p.Header}");
			}

			return patches;
		}

		public void Apply(Mode mode) {
			if (applied)
				throw new Exception("Already Applied");

			applied = true;

			//we maintain delta as the offset of the last patch (applied location - expected location)
			//this way if a line is inserted, and all patches are offset by 1, only the first patch is reported as offset
			int delta = 0;
			foreach (var patch in patches) {
				if (ApplyExact(patch, delta))
					continue;
				if (mode >= Mode.OFFSET && ApplyOffset(patch, ref delta))
					continue;
				if (mode >= Mode.FUZZY && ApplyFuzzy(patch, ref delta))
					continue;

				patch.Fail();
			}
		}

		public string[] ResultLines() => textLines.ToArray();
		public IEnumerable<Result> Results() => patches.Select(p => p.result);

		private bool ApplyExact(Patch2 patch, int delta) {
			int loc = patch.start2 + delta;
			if (loc + patch.length1 > textLines.Count)
				return false;

			int i = loc;
			foreach (var line in patch.ContextLines)
				if (textLines[i++] != line)
					return false;

			ApplyExactAt(loc, patch);
			patch.SucceedExact(loc);
			return true;
		}

		private bool ApplyOffset(Patch2 patch, ref int delta) {
			if (lmText == null)
				LinesToChars();

			int loc = patch.start2 + delta;
			if (loc < 0) loc = 0;
			else if (loc >= textLines.Count) loc = textLines.Count - 1;

			int forward = lmText.IndexOf(patch.lmContext, loc, StringComparison.Ordinal);
			int reverse = lmText.LastIndexOf(patch.lmContext, loc + patch.length1, StringComparison.Ordinal);
			if (forward < 0 && reverse < 0)
				return false;

			int found = reverse < 0 || forward >= 0 && (forward - loc) < (loc - reverse) ? forward : reverse;
			ApplyExactAt(found, patch);
			
			delta = found - patch.start2;
			patch.SucceedOffset(this, found, found - loc);

			return true;
		}

		private void ApplyExactAt(int loc, Patch2 patch) {
			if (!patch.ContextLines.SequenceEqual(textLines.GetRange(loc, patch.length1)))
				throw new Exception("Patch engine failure");

			textLines.RemoveRange(loc, patch.length1);
			textLines.InsertRange(loc, patch.PatchedLines);

			//update the lineModeText
			if (lmText != null)
				lmText = lmText.Remove(loc) + patch.lmPatched + lmText.Substring(loc + patch.length1);

			//update the wordModeLines
			if (wmLines != null) {
				wmLines.RemoveRange(loc, patch.length1);
				wmLines.InsertRange(loc, patch.wmPatched);
			}
		}

		private void LinesToChars() {
			charToLine.Add("\0");//lets avoid the 0 char

			foreach (var patch in patches)
				patch.LinesToChars(this);

			lmText = new string(textLines.Select(AddLine).ToArray());
		}

		private char AddLine(string line) {
			char c;
			if (!lineToChar.TryGetValue(line, out c)) {
				lineToChar.Add(line, c = (char)charToLine.Count);
				charToLine.Add(line);
			}

			return c;
		}

		private void WordsToChars() {
			foreach (var patch in patches)
				patch.WordsToChars(this);

			wmLines = textLines.Select(WordsToChars).ToList();
		}

		private char[] buf = new char[4096];
		private string WordsToChars(string line) {
			int b = 0;

			for (int i = 0, len; i < line.Length; i += len) {
				char c = line[i];
				len = 1;
				if (char.IsLetter(c)) while (i+len < line.Length && char.IsLetterOrDigit(line, i+len)) len++;
				else if (char.IsDigit(c)) while (i + len < line.Length && char.IsDigit(line, i+len)) len++;

				if (len > 1) {
					var word = line.Substring(i, len);
					if (!wordToChar.TryGetValue(word, out c))
						wordToChar[word] = c = (char)nextWord++;
				}

				if (b > buf.Length) Array.Resize(ref buf, buf.Length * 2);
				buf[b++] = c;
			}

			return new string(buf, 0, b);
		}

		//a match below quality 0.5 is unacceptably bad
		private const float MinScore = 0.5f;
		private bool ApplyFuzzy(Patch2 patch, ref int delta) {
			if (wmLines == null)
				WordsToChars();
			
			int loc = patch.start2 + delta;
			var pattern = patch.wmContext;

			float bestScore = MinScore;
			int[] bestMatch = null;

			var mmForward = new MatchMatrix(pattern, wmLines);
			float score = mmForward.Initialize(loc);
			if (score >= bestScore) {
				bestScore = score;
				bestMatch = mmForward.Path();
			}

			var mmReverse = new MatchMatrix(pattern, wmLines);
			mmReverse.Initialize(loc);

			int warnDist = OffsetWarnDistance(patch.length1, textLines.Count);
			for (int i = 0; mmForward.CanStepForward && mmReverse.CanStepBackward; i++) {
				//within the warning range it's a straight up fight
				//past the warning range, quality is reduced by 10% per warning range
				float penalty = i < warnDist ? 0 : 0.1f * i / warnDist;

				score = mmForward.StepForward() - penalty;
				if (score > bestScore) {
					bestScore = score;
					bestMatch = mmForward.Path();
				}

				score = mmReverse.StepBackward() - penalty;
				if (score > bestScore) {
					bestScore = score;
					bestMatch = mmReverse.Path();
				}

				if (bestScore + penalty > 1f)
					break;
			}

			if (bestMatch == null)
				return false;

			//copy the patch
			var ndiffs = patch.diffs.Select(d => new Diff(d.op, d.text)).ToList();
			var npatch = new Patch2 {
				diffs = ndiffs,
				start1 = patch.start1 + bestMatch[0] - patch.start2,
				start2 = bestMatch[0]
			};

			//build a patch-patch while we're at it
			var pdiff = new Patch();

			//keep operations, but replace lines with lines in source text
			//skipped patch lines (-1) are deleted
			//skipped source lines (increasing offset) are inserted
			//an insertion rule is deleted, if neither of the surrounding lines are kept context (Mode == EQUAL)
			int j = 0; //index in original patch diff list
			for (int i = 0; i < patch.length1; i++) {
				var mloc = bestMatch[i];

				if (mloc >= 0) { //remap
					int ploc = i > 0 ? bestMatch[i - 1] : -1;
					if (ploc >= 0) {//insert extra lines into patch
						for (int l = ploc + 1; l < mloc; l++) {
							bool equal = j > 0 && ndiffs[j - 1].op == Operation.EQUAL ||
										 j < ndiffs.Count && ndiffs[j].op == Operation.EQUAL;
							var diff = new Diff(equal ? Operation.EQUAL : Operation.DELETE, textLines[l]);
							ndiffs.Insert(j++, diff);
							pdiff.diffs.Add(new Diff(Operation.INSERT, diff.ToString()));
						}
					}
				}

				//add insert lines
				while (ndiffs[j].op == Operation.INSERT) {
					pdiff.diffs.Add(new Diff(Operation.EQUAL, ndiffs[j].ToString()));
					j++;
				}

				if (mloc < 0) {//deleted patch line
					pdiff.diffs.Add(new Diff(Operation.DELETE, ndiffs[j].ToString()));
					ndiffs.RemoveAt(j);
				}
				else if (ndiffs[j].text != textLines[mloc]) { //change line content
					pdiff.diffs.Add(new Diff(Operation.DELETE, ndiffs[j].ToString()));
					ndiffs[j].text = textLines[mloc];
					pdiff.diffs.Add(new Diff(Operation.INSERT, ndiffs[j].ToString()));
					j++;
				}
				else {
					// perfect match
					pdiff.diffs.Add(new Diff(Operation.EQUAL, ndiffs[j].ToString()));
					j++;
				}
			}
			
			//add trailing insert lines to patch diff
			for (; j < ndiffs.Count; j++) {
				pdiff.diffs.Add(new Diff(Operation.EQUAL, ndiffs[j].ToString()));
				j++;
			}

			//finish our new patch
			npatch.length1 = npatch.ContextLines.Count();
			npatch.length2 = npatch.PatchedLines.Count();
			npatch.LinesToChars(this);
			npatch.WordsToChars(this);

			//finish the patch diff
			pdiff.diffs.Insert(0, new Diff(Operation.DELETE, patch.Header));
			pdiff.diffs.Insert(1, new Diff(Operation.INSERT, npatch.Header));
			pdiff.length1 = pdiff.ContextLines.Count();
			pdiff.length2 = pdiff.PatchedLines.Count();

			ApplyExactAt(npatch.start2, npatch);
			patch.SucceedFuzzy(this, npatch.start2 - loc, bestScore, npatch, pdiff);
			return true;
		}

		private const int MaxLineDistance = 5;
		private class MatchNodes
		{
			//score of this match (1.0 = perfect, 0.0 = no match)
			public float score;
			//sum of the match scores in the best path up to this node
			public float sum;
			//offset index of the next node in the path
			public int next;
		}

		//contains match entries for consecutive characters of a pattern and the search text starting at line offset loc
		private class StraightMatch
		{
			public readonly MatchNodes[] nodes;

			public StraightMatch(int patternLength) {
				nodes = new MatchNodes[patternLength];
				for (int i = 0; i < patternLength; i++)
					nodes[i] = new MatchNodes();
			}

			public void Update(int loc, string[] pattern, List<string> search) {
				for (int i = 0; i < pattern.Length; i++) {
					int l = i + loc;
					if (l < 0 || l >= search.Count)
						nodes[i].score = 0;
					else
						nodes[i].score = MatchLines(pattern[i], search[l]);
				}
			}
		}

		//returns the pattern distance between two successive nodes in a path with offsets i and j
		//if i == j, then the line offsets are the same and the pattern distance is 1 line
		//if j > i, then the offset increased by j-i in successive pattern lines and the pattern distance is 1 line
		//if j < i, then i-j patch lines were skipped between nodes, and the distance is 1+i-j
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int OffsetsToPatternDistance(int i, int j) => j >= i ? 1 : 1 + i - j;

		private class MatchMatrix
		{
			//center line offset for this match matrix
			private int loc;
			private readonly string[] pattern;
			private readonly List<string> search;
			//consecutive matches for pattern offset from loc by up to MaxLineDistance
			//first entry is for pattern starting at loc in text, last entry is offset +MaxLineDistance
			private readonly StraightMatch[] matches;
			//offset index of first node in best path
			private int firstNode;

			public MatchMatrix(string[] pattern, List<string> search) {
				this.pattern = pattern;
				this.search = search;

				matches = new StraightMatch[MaxLineDistance+1];
				for (int i = 0; i <= MaxLineDistance; i++)
					matches[i] = new StraightMatch(pattern.Length);
			}

			public float Initialize(int loc) {
				this.loc = loc;

				for (int i = 0; i <= MaxLineDistance; i++)
					matches[i].Update(loc + i, pattern, search);

				return Recalculate();
			}
			
			public bool CanStepForward => loc < search.Count - pattern.Length + MaxLineDistance;
			public bool CanStepBackward => loc > -MaxLineDistance;

			public float StepForward() {
				if (!CanStepForward)
					return 0;

				loc++;

				var reuse = matches[0];
				for (int i = 1; i <= MaxLineDistance; i++)
					matches[i - 1] = matches[i];

				matches[MaxLineDistance] = reuse;
				reuse.Update(loc + MaxLineDistance, pattern, search);

				return Recalculate();
			}

			public float StepBackward() {
				if (!CanStepBackward)
					return 0;

				loc--;

				var reuse = matches[MaxLineDistance];
				for (int i = MaxLineDistance; i > 0; i--)
					matches[i] = matches[i - 1];

				matches[0] = reuse;
				reuse.Update(loc, pattern, search);

				return Recalculate();
			}

			//calculates the best path through the match matrix
			//all paths must start with the first line of pattern matched to the line at loc (0 offset)
			private float Recalculate() {
				//tail nodes have sum = score
				for (int j = 0; j <= MaxLineDistance; j++) {
					var node = matches[j].nodes[pattern.Length - 1];
					node.sum = node.score;
					node.next = -1;//no next
				}

				//calculate best paths for all nodes excluding head
				for (int i = pattern.Length - 2; i >= 0; i--)
					for (int j = 0; j <= MaxLineDistance; j++) {
						//for each node
						var node = matches[j].nodes[i];
						int maxk = -1;
						float maxsum = 0;
						for (int k = 0; k <= MaxLineDistance; k++) {
							int l = i + OffsetsToPatternDistance(j, k);
							if (l >= pattern.Length) continue;

							float sum = matches[k].nodes[l].sum;
							if (k > j) sum -= 0.5f * (k - j); //penalty for skipping lines in search text

							if (sum > maxsum) {
								maxk = k;
								maxsum = sum;
							}
						}

						node.sum = maxsum + node.score;
						node.next = maxk;
					}

				//find starting node
				{
					firstNode = 0;
					float maxsum = matches[0].nodes[0].sum;
					for (int k = 1; k <= MaxLineDistance; k++) {
						float sum = matches[k].nodes[0].sum;
						if (sum > maxsum) {
							firstNode = k;
							maxsum = sum;
						}
					}
				}

				//return best path value
				return matches[firstNode].nodes[0].sum / pattern.Length;
			}

			public int[] Path() {
				var path = new int[pattern.Length];

				int offset = firstNode; //offset of current node
				var node = matches[firstNode].nodes[0];
				path[0] = loc + offset;

				int i = 0; //index in pattern of current node
				while (node.next >= 0) {
					int delta = OffsetsToPatternDistance(offset, node.next);
					while (delta-- > 1) //skipped pattern lines
						path[++i] = -1;
					
					offset = node.next;
					node = matches[offset].nodes[++i];
					path[i] = loc + i + offset;
				}

				while (++i < path.Length)//trailing lines with no match
					path[i] = -1;

				return path;
			}

			public string Visualise() {
				var path = Path();
				var sb = new StringBuilder();
				for (int j = 0; j <= MaxLineDistance; j++) {
					sb.Append(j).Append(':');
					var line = matches[j];
					for (int i = 0; i < pattern.Length; i++) {
						bool inPath = path[i] > 0 && path[i] == loc + i + j;
						sb.Append(inPath ? '[' : ' ');
						int score = (int) Math.Round(line.nodes[i].score * 100);
						sb.Append(score == 100 ? "%%" : score.ToString("D2"));
						sb.Append(inPath ? ']' : ' ');
					}
					sb.AppendLine();
				}
				return sb.ToString();
			}
		}

		//assumes the lines are in word to char mode
		//return 0.0 poor match to 1.0 perfect match
		//uses LevenshtienDistance. A distance with half the maximum number of errors is considered a 0.0 scored match
		public static float MatchLines(string s, string t) {
			int d = LevenshteinDistance(s, t);
			if (d == 0)
				return 1f;//perfect match

			int max = Math.Max(s.Length, t.Length) / 2;
			return Math.Max(0f, 1f - d / (float)max);
		}

		//https://en.wikipedia.org/wiki/Levenshtein_distance
		public static int LevenshteinDistance(string s, string t) {
			// degenerate cases
			if (s == t) return 0;
			if (s.Length == 0) return t.Length;
			if (t.Length == 0) return s.Length;

			// create two work vectors of integer distances
			//previous
			int[] v0 = new int[t.Length + 1];
			//current
			int[] v1 = new int[t.Length + 1];

			// initialize v1 (the current row of distances)
			// this row is A[0][i]: edit distance for an empty s
			// the distance is just the number of characters to delete from t
			for (int i = 0; i < v1.Length; i++)
				v1[i] = i;

			for (int i = 0; i < s.Length; i++) {
				// swap v1 to v0, reuse old v0 as new v1
				var tmp = v0;
				v0 = v1;
				v1 = tmp;

				// calculate v1 (current row distances) from the previous row v0

				// first element of v1 is A[i+1][0]
				//   edit distance is delete (i+1) chars from s to match empty t
				v1[0] = i + 1;

				// use formula to fill in the rest of the row
				for (int j = 0; j < t.Length; j++) {
					int del = v1[j] + 1;
					int ins = v0[j + 1] + 1;
					int subs = v0[j] + (s[i] == t[j] ? 0 : 1);
					v1[j + 1] = Math.Min(del, Math.Min(ins, subs));
				}
			}

			return v0[t.Length];
		}
	}
}
