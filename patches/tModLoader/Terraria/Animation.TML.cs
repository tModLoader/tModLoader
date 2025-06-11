using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria;

public partial class Animation
{
	public record AnimationFrameData(int frameRate, int[] frames);
	public static int AnimationCount { get; private set; } = 5;

	public static Dictionary<int, AnimationFrameData> AnimationFrameDatas = new();

	public static void Unload()
	{
		AnimationCount = 5;
		AnimationFrameDatas.Clear();
	}

	public static int RegisterTemporaryAnimation(int frameRate, int[] frames)
	{
		int animationType = AnimationCount++;
		AnimationFrameDatas[animationType] = new AnimationFrameData(frameRate, frames);
		return animationType;
	}
}
