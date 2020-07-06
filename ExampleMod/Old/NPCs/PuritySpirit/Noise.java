import java.awt.*;
import java.awt.image.*;
import java.io.*;
import java.util.*;
import javax.imageio.*;
public class Noise
{
	public static void main(String[] args) throws IOException
	{
		Random random = new Random();
		int width = 60;
		int height = 244;
		BufferedImage image = new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
		Color color = new Color(11, 102, 10);
		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				if (random.nextInt(3) == 0)
				{
					image.setRGB(x, y, color.getRGB());
				}
			}
		}
		ImageIO.write(image, "png", new File("Noise.png"));
	}
}