using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceModel;
using OpenBveApi.Runtime;
using OpenBveApi.Interop;

namespace TrainManager.SafetySystems
{
    [Guid("1388460c-fc46-46f0-9a3a-98624f6304bd")]
    public interface IAtsPlugin
    {
	    void setPluginFile(string fileName);

	    bool load(VehicleSpecs specs, InitializationModes mode);

	    void unload();

	    void beginJump(InitializationModes mode);

	    ElapseProxy elapse(ElapseProxy proxyData);

	    void setReverser(int reverser);

	    void setPowerNotch(int powerNotch);

	    void setBrake(int brakeNotch);

	    void keyDown(int key);

	    void keyUp(int key);

	    void hornBlow(int type);

	    void doorChange(int oldState, int newState);

	    void setSignal(int aspect);

	    void setBeacon(BeaconData beacon);
    };

	public class Win32CallbackHandler : IAtsPluginCallback
	{
		public string lastError = string.Empty;
		public bool Unload = false;

		public void ReportError(string Error)
		{
			this.lastError = Error;
		}
	}

	/// <summary>Represents a Win32 plugin proxied through WinPluginProxy</summary>
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("c570f27c-0a86-4d9b-a568-4d4b217caf7b")]
    internal class Win32ProxyPlugin : IAtsPlugin
	{
		internal bool externalCrashed = false;
        private int win32Dll_instances = 0;
        private readonly Process hostProcess;
        private readonly IAtsPluginProxy pipeProxy;
        private readonly object syncLock = new object();

	    public Win32CallbackHandler callback;

        internal Win32ProxyPlugin()
        {
            lock (syncLock)
            {
                if (win32Dll_instances == 0)
                {
                    hostProcess = new Process();
                    var handle = Process.GetCurrentProcess().MainWindowHandle;
                    hostProcess.StartInfo.FileName = @"Win32PluginProxy.exe";
                    hostProcess.Start();
                    Shared.eventHostReady.WaitOne();
                    pipeProxy = getPipeProxy();
                    SetForegroundWindow(handle.ToInt32());
                }
                win32Dll_instances++;
            }
        }
        ~Win32ProxyPlugin()
        {
            lock (syncLock)
            {
                win32Dll_instances--;
                if (win32Dll_instances == 0)
                {
                    Shared.eventHostShouldStop.Set();
                    hostProcess.WaitForExit();
                }
            }
        }

        [DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);

        public IAtsPluginProxy getPipeProxy()
        {
			callback = new Win32CallbackHandler();
			DuplexChannelFactory<IAtsPluginProxy> pipeFactory = new DuplexChannelFactory<IAtsPluginProxy>(new InstanceContext(callback), new NetNamedPipeBinding(), new EndpointAddress(Shared.endpointAddress));

            return pipeFactory.CreateChannel();
        }

	    public void setPluginFile(string fileName)
	    {
		    pipeProxy.SetPluginFile(fileName);
	    }

	    public bool load(VehicleSpecs specs, InitializationModes mode)
	    {
		    return pipeProxy.Load(specs, mode);
	    }

	    public void unload()
	    {
		    pipeProxy.Unload();
	    }

	    public void beginJump(InitializationModes mode)
	    {
		    pipeProxy.BeginJump(mode);
	    }

	    public ElapseProxy elapse(ElapseProxy proxyData)
	    {
		    try
		    {
			    return pipeProxy.Elapse(proxyData);
		    }
		    catch
		    {
			    externalCrashed = true;
		    }

		    return proxyData;
	    }

	    public void setReverser(int reverser)
	    {
		    pipeProxy.SetReverser(reverser);
	    }

	    public void setPowerNotch(int powerNotch)
	    {
		    pipeProxy.SetPowerNotch(powerNotch);
	    }

	    public void setBrake(int brakeNotch)
	    {
		    pipeProxy.SetBrake(brakeNotch);
	    }

	    public void keyDown(int key)
	    {
		    pipeProxy.KeyDown(key);
	    }

	    public void keyUp(int key)
	    {
		    pipeProxy.KeyUp(key);
	    }

	    public void hornBlow(int type)
	    {
		    pipeProxy.HornBlow(type);
	    }

	    public void doorChange(int oldState, int newState)
	    {
		    pipeProxy.DoorChange(oldState, newState);
	    }

	    public void setSignal(int aspect)
	    {
		    pipeProxy.SetSignal(aspect);
	    }

	    public void setBeacon(BeaconData beacon)
	    {
		    pipeProxy.SetBeacon(beacon);
	    }
    }
}
