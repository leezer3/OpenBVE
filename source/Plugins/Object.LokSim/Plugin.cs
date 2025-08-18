//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using System.Text;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace Plugin
{
	public class Plugin : ObjectInterface
	{
		internal static HostInterface currentHost;

		internal static string LoksimPackageFolder;

		public override string[] SupportedAnimatedObjectExtensions => new[] { ".l3dgrp" };
		public override string[] SupportedStaticObjectExtensions => new[] { ".l3dobj" };

		public override void Load(HostInterface host, FileSystem fileSystem)
		{
			currentHost = host;
			LoksimPackageFolder = fileSystem.LoksimPackageInstallationDirectory;
		}


		public override bool CanLoadObject(string path)
		{
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return false;
			}
			if (path.ToLowerInvariant().EndsWith(".l3dobj", StringComparison.InvariantCultureIgnoreCase) || path.ToLowerInvariant().EndsWith(".l3dgrp", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}

		public override bool LoadObject(string path, Encoding textEncoding, out UnifiedObject unifiedObject)
		{
			try
			{
				if (path.ToLowerInvariant().EndsWith(".l3dobj", StringComparison.InvariantCultureIgnoreCase))
				{
					unifiedObject = Ls3DObjectParser.ReadObject(path, Vector3.Zero);
				}
				else
				{
					unifiedObject = Ls3DGrpParser.ReadObject(path, textEncoding, Vector3.Zero);
				}
			}
			catch
			{
				unifiedObject = null;
				return false;
			}

			return true;
		}
	}
}
