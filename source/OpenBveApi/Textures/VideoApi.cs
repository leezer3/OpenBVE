using System;
using System.Runtime.InteropServices;

namespace OpenBveApi.Textures
{
	/// <summary>Provides API bindings for libmpv video playback.</summary>
	public static class VideoApi
	{
		private const string MpvDll = "libmpv-2.dll";

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr LoadLibraryW(string lpLibFileName);

		[DllImport("libdl", EntryPoint = "dlopen")]
		private static extern IntPtr dlopen(string filename, int flags);

		static VideoApi()
		{
			try
			{
				string arch = IntPtr.Size == 8 ? "x64" : "x86";
				string baseDir = AppDomain.CurrentDomain.BaseDirectory;
				bool isWindows = System.IO.Path.DirectorySeparatorChar == '\\';

				if (isWindows)
				{
					string path = System.IO.Path.Combine(baseDir, arch, MpvDll);
					if (System.IO.File.Exists(path))
					{
						LoadLibraryW(path);
					}
				}
				else
				{
					// Unix/macOS: Map to .so or .dylib
					string libName = "libmpv.so";
					if (System.IO.File.Exists(System.IO.Path.Combine(baseDir, arch, "libmpv.dylib")))
					{
						libName = "libmpv.dylib";
					}
					string path = System.IO.Path.Combine(baseDir, arch, libName);
					if (System.IO.File.Exists(path))
					{
						dlopen(path, 2); // RTLD_NOW = 2
					}
				}
			}
			catch
			{
			}
		}

		/// <summary>Creates an mpv instance.</summary>
		/// <returns>A pointer to the mpv handle.</returns>
		[DllImport(MpvDll, EntryPoint = "mpv_create", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr mpv_create();

		/// <summary>Initializes the mpv instance.</summary>
		/// <param name="handle">The mpv handle.</param>
		/// <returns>An integer status code.</returns>
		[DllImport(MpvDll, EntryPoint = "mpv_initialize", CallingConvention = CallingConvention.Cdecl)]
		public static extern int mpv_initialize(IntPtr handle);

		/// <summary>Destroys and terminates the mpv instance.</summary>
		/// <param name="handle">The mpv handle.</param>
		[DllImport(MpvDll, EntryPoint = "mpv_terminate_destroy", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mpv_terminate_destroy(IntPtr handle);

		/// <summary>Executes a command on the mpv instance.</summary>
		/// <param name="handle">The mpv handle.</param>
		/// <param name="args">The command arguments.</param>
		/// <returns>An integer status code.</returns>
		[DllImport(MpvDll, EntryPoint = "mpv_command", CallingConvention = CallingConvention.Cdecl)]
		public static extern int mpv_command(IntPtr handle, IntPtr[] args);

		/// <summary>Sets an option string on the mpv instance.</summary>
		/// <param name="handle">The mpv handle.</param>
		/// <param name="name">The option name.</param>
		/// <param name="value">The option value.</param>
		/// <returns>An integer status code.</returns>
		[DllImport(MpvDll, EntryPoint = "mpv_set_option_string", CallingConvention = CallingConvention.Cdecl)]
		public static extern int mpv_set_option_string(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string value);

		/// <summary>Gets a property value from the mpv instance.</summary>
		/// <param name="handle">The mpv handle.</param>
		/// <param name="name">The property name.</param>
		/// <returns>A pointer to the property value string.</returns>
		[DllImport(MpvDll, EntryPoint = "mpv_get_property_string", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr mpv_get_property_string(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string name);

		/// <summary>Frees data allocated by mpv.</summary>
		/// <param name="data">The pointer to the data to free.</param>
		[DllImport(MpvDll, EntryPoint = "mpv_free", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mpv_free(IntPtr data);

		/// <summary>Invalid render parameter type.</summary>
		public const int MPV_RENDER_PARAM_TYPE_INVALID = 0;
		/// <summary>API type render parameter.</summary>
		public const int MPV_RENDER_PARAM_API_TYPE = 1;
		/// <summary>OpenGL init parameters render parameter.</summary>
		public const int MPV_RENDER_PARAM_OPENGL_INIT_PARAMS = 2;

		/// <summary>OpenGL render API type string.</summary>
		public const string MPV_RENDER_API_TYPE_OPENGL = "opengl";

		/// <summary>Represents an mpv render parameter.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct mpv_render_param
		{
			/// <summary>The parameter type.</summary>
			public int type;
			/// <summary>The parameter data pointer.</summary>
			public IntPtr data;
		}

		/// <summary>Represents OpenGL initialization parameters for mpv.</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct mpv_opengl_init_params
		{
			/// <summary>Pointer to the get_proc_address callback.</summary>
			public IntPtr get_proc_address;
			/// <summary>Pointer to the callback context.</summary>
			public IntPtr get_proc_address_ctx;
			/// <summary>Pointer to extra fields.</summary>
			public IntPtr extra_fields;
		}

		/// <summary>Delegate callback to retrieve OpenGL procedure addresses.</summary>
		/// <param name="ctx">The callback context.</param>
		/// <param name="name">The procedure name.</param>
		/// <returns>The address of the procedure.</returns>
		public delegate IntPtr GetProcAddressCallback(IntPtr ctx, [MarshalAs(UnmanagedType.LPStr)] string name);

		/// <summary>Creates an mpv render context.</summary>
		/// <param name="res">The resulting render context pointer.</param>
		/// <param name="mpv">The mpv instance handle.</param>
		/// <param name="params_list">The parameter list.</param>
		/// <returns>An integer status code.</returns>
		[DllImport(MpvDll, EntryPoint = "mpv_render_context_create", CallingConvention = CallingConvention.Cdecl)]
		public static extern int mpv_render_context_create(out IntPtr res, IntPtr mpv, mpv_render_param[] params_list);

		/// <summary>Frees the mpv render context.</summary>
		/// <param name="ctx">The render context pointer.</param>
		[DllImport(MpvDll, EntryPoint = "mpv_render_context_free", CallingConvention = CallingConvention.Cdecl)]
		public static extern void mpv_render_context_free(IntPtr ctx);

		/// <summary>Renders a frame using the context.</summary>
		/// <param name="ctx">The render context pointer.</param>
		/// <param name="params_list">The render parameter list.</param>
		/// <returns>An integer status code.</returns>
		[DllImport(MpvDll, EntryPoint = "mpv_render_context_render", CallingConvention = CallingConvention.Cdecl)]
		public static extern int mpv_render_context_render(IntPtr ctx, mpv_render_param[] params_list);

		/// <summary>Checks/updates the render context state.</summary>
		/// <param name="ctx">The render context pointer.</param>
		/// <returns>Flags indicating update status.</returns>
		[DllImport(MpvDll, EntryPoint = "mpv_render_context_update", CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong mpv_render_context_update(IntPtr ctx);

		// Helper to load DLL on Windows
		[DllImport("opengl32.dll", CharSet = CharSet.Ansi)]
		private static extern IntPtr wglGetProcAddress(string name);

		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string name);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		/// <summary>Retrieves OpenGL procedure address on Windows.</summary>
		/// <param name="ctx">The context pointer.</param>
		/// <param name="name">The procedure name.</param>
		/// <returns>The address of the procedure.</returns>
		public static IntPtr GetProcAddressOpenGL(IntPtr ctx, string name)
		{
			try
			{
				IntPtr proc = wglGetProcAddress(name);
				if (proc == IntPtr.Zero)
				{
					IntPtr hModule = GetModuleHandle("opengl32.dll");
					proc = GetProcAddress(hModule, name);
				}
				return proc;
			}
			catch
			{
				return IntPtr.Zero;
			}
		}
	}
}
