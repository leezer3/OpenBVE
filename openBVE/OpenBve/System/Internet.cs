using System;
using System.IO;
using System.Net;
using System.Threading;

namespace OpenBve {
	/// <summary>Provides methods for accessing the internet.</summary>
	internal static class Internet {
		
		private static string UserAgent;
		static Internet() {
			string[] agents = new string[] {
				"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:9.0.1) Gecko/20100101 Firefox/9.0.1",
				"Mozilla/5.0 (Windows NT 6.1; WOW64; rv:10.0.2) Gecko/20100101 Firefox/10.0.2",
				"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.11 (KHTML, like Gecko) Chrome/17.0.963.56 Safari/535.11",
				"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_3) AppleWebKit/534.53.11 (KHTML, like Gecko) Version/5.1.3 Safari/534.53.10",
				"Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)",
				"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.77 Safari/535.7",
				"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_7_2) AppleWebKit/534.52.7 (KHTML, like Gecko) Version/5.1.2 Safari/534.52.7",
				"Mozilla/5.0 (Windows NT 5.1; rv:9.0.1) Gecko/20100101 Firefox/9.0.1",
				"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_6_8) AppleWebKit/534.52.7 (KHTML, like Gecko) Version/5.1.2 Safari/534.52.7",
				"Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.75 Safari/535.7"
			};
			int index = Program.RandomNumberGenerator.Next(0, agents.Length);
			UserAgent = agents[index];
		}
		
		/// <summary>Adds some user agent and referer to the web client.</summary>
		/// <param name="client">The web client.</param>
		/// <param name="url">The URL to be accessed.</param>
		private static void AddWebClientHeaders(WebClient client, string url) {
			try {
				client.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);
			} catch { }
			if (url.StartsWith("http://")) {
				int index = url.IndexOf('/', 7);
				if (index >= 7) {
					string referer = url.Substring(0, index + 1);
					try {
						client.Headers.Add(HttpRequestHeader.Referer, referer);
					} catch { }
				}
			}
		}
		
		/// <summary>Adds the user-specified proxy server to the client.</summary>
		/// <param name="client">The web client.</param>
		private static void AddWebClientProxy(WebClient client) {
			if (Interface.CurrentOptions.ProxyUrl.Length != 0) {
				WebProxy proxy = new WebProxy(Interface.CurrentOptions.ProxyUrl);
				proxy.Credentials = new NetworkCredential(Interface.CurrentOptions.ProxyUserName, Interface.CurrentOptions.ProxyPassword);
				client.Proxy = proxy;
			}
		}
		
		/// <summary>Downloads data from the specified URL.</summary>
		/// <param name="url">The URL.</param>
		/// <returns>The data.</returns>
		internal static byte[] DownloadBytesFromUrl(string url) {
			byte[] bytes;
			using (WebClient client = new WebClient()) {
				AddWebClientHeaders(client, url);
				AddWebClientProxy(client);
				bytes = client.DownloadData(url);
			}
			return bytes;
		}
		
		/// <summary>Downloads data from the specified URL.</summary>
		/// <param name="url">The URL.</param>
		/// <param name="bytes">Receives the data.</param>
		/// <param name="size">Accumulates the size of the downloaded data in an interlocked operation. If the operation fails, all accumulated size is subtracted again.</param>
		/// <returns>Whether the operation was successful.</returns>
		internal static bool TryDownloadBytesFromUrl(string url, out byte[] bytes, ref int size) {
			int count = 0;
			try {
				using (WebClient client = new WebClient()) {
					AddWebClientHeaders(client, url);
					AddWebClientProxy(client);
					using (Stream stream = client.OpenRead(url)) {
						const int chunkSize = 65536;
						bytes = new byte[chunkSize];
						while (true) {
							if (count + chunkSize >= bytes.Length) {
								Array.Resize<byte>(ref bytes, bytes.Length << 1);
							}
							int read = stream.Read(bytes, count, chunkSize);
							if (read != 0) {
								count += read;
								Interlocked.Add(ref size, read);
							} else {
								break;
							}
						}
					}
				}
				Array.Resize<byte>(ref bytes, count);
				return true;
			} catch {
				Interlocked.Add(ref size, -count);
				bytes = null;
				return false;
			}
		}
		
		/// <summary>Downloads bytes from the specified URL and saves them to a file.</summary>
		/// <param name="url">The URL.</param>
		/// <param name="file">The file name.</param>
		/// <param name="days">If the file already exists and was modified during the last so and so days, the download will be bypassed.</param>
		/// <param name="callback">The function to execute once the data has been saved to the file, or a null reference. The argument in the callback function is of type System.String and contains the file name.</param>
		internal static void DownloadAndSaveAsynchronous(string url, string file, double days, ParameterizedThreadStart callback) {
			bool download;
			if (File.Exists(file)) {
				try {
					DateTime lastWrite = File.GetLastWriteTime(file);
					TimeSpan span = DateTime.Now - lastWrite;
					download = span.TotalDays > days;
				} catch {
					download = true;
				}
			} else {
				download = true;
			}
			if (download) {
				ThreadStart start = new ThreadStart(
					() => {
						try {
							byte[] bytes = DownloadBytesFromUrl(url);
							string directory = Path.GetDirectoryName(file);
							try {
								Directory.CreateDirectory(directory);
								File.WriteAllBytes(file, bytes);
							} catch { }
							if (callback != null) {
								callback.Invoke(file);
							}
						} catch { }
					}
				);
				Thread thread = new Thread(start);
				thread.IsBackground = true;
				thread.Start();
			} else if (callback != null) {
				callback.Invoke(file);
			}
		}
		
	}
}