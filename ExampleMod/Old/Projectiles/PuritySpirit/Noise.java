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
		int width = 40;
		int height = 80;
		BufferedImage image = new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB);
		Color[] colors = new Color[]
		{
			new Color(128, 255, 128),
			new Color(128, 255, 128),
			new Color(70, 230, 69),
			new Color(21, 204, 20)
		};
		for(int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				Color color = colors[random.nextInt(colors.length)];
				image.setRGB(x, y, color.getRGB());
			}
		}
		ImageIO.write(image, "png", new File("Noise.png"));
	}
}