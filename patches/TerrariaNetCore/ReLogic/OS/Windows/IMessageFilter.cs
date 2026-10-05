namespace ReLogic.OS.Windows;

#if NETCORE
public interface IMessageFilter
{
	bool PreFilterMessage(ref Message m);
}
#endif