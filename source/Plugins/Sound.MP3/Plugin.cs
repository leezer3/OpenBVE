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

using System.IO;
using OpenBveApi.Hosts;
using OpenBveApi.Sounds;

namespace Plugin
{
	public partial class Plugin : SoundInterface
	{

		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public override void Load(HostInterface host)
		{
			// CurrentHost = host;
		}

		/// <summary>Checks whether the plugin can load the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <returns>Whether the plugin can load the specified sound.</returns>
		public override bool CanLoadSound(string path)
		{
			if (string.IsNullOrEmpty(path) || !File.Exists(path))
			{
				return false;
			}

			using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					byte[] magicNumber = reader.ReadBytes(3);

					// without an ID3 tag or with an ID3v1 tag
					if (magicNumber[0] == 0xFF && magicNumber[1] == 0xFB)
					{
						return true;
					}

					// with an ID3v2 tag
					if (magicNumber[0] == 0x49 && magicNumber[1] == 0x44 && magicNumber[2] == 0x33)
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>Loads the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool LoadSound(string path, out Sound sound)
		{
			sound = LoadFromFile(path);
			return true;
		}
	}
}
