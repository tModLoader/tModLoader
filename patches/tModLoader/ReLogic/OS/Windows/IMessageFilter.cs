namespace ReLogic.OS.Windows;

public interface IMessageFilter
{
	bool PreFilterMessage(ref Message m);
}