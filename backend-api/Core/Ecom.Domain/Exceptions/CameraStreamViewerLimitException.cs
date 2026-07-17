namespace Ecom.Domain.Exceptions;

public class CameraStreamViewerLimitException : Exception
{
	public CameraStreamViewerLimitException(string message, int currentViewers, int maxViewers)
		: base(message)
	{
		CurrentViewers = currentViewers;
		MaxViewers = maxViewers;
	}

	public int CurrentViewers { get; }
	public int MaxViewers { get; }
}

