// ReSharper disable InconsistentNaming
namespace OpenBveApi.Hosts
{
	/// <summary>The host platform</summary>
	public enum HostPlatform
	{
		/// <summary>Microsoft Windows and compatabiles</summary>
		MicrosoftWindows = 0,
		/// <summary>Linux</summary>
		GNULinux = 1,
		/// <summary>Mac OS-X</summary>
		AppleOSX = 2,
		/// <summary>FreeBSD</summary>
		FreeBSD = 3,
		/// <summary>Emulated Windows</summary>
		WINE = 4

	}
}
