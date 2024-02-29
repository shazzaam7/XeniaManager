using System;
using System.IO;
using System.Net.Http;

// Imported
using Serilog;


namespace Xenia_Manager.Classes
{
    public class DownloadManager
    {
        public event EventHandler<int> ProgressChanged;

        /// <summary>
        /// Used for downloading Xenia builds
        /// </summary>
        /// <param name="url">URL where the build is stored</param>
        /// <param name="savePath">Where we want to save the build</param>
        /// <returns></returns>
        public async Task DownloadBuild(string url, string savePath)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        var bytesRead = 0L;
                        var buffer = new byte[4096];
                        var isMoreToRead = true;

                        using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                        {
                            do
                            {
                                var read = await stream.ReadAsync(buffer, 0, buffer.Length);
                                if (read == 0)
                                {
                                    isMoreToRead = false;
                                }
                                else
                                {
                                    await fileStream.WriteAsync(buffer, 0, read);
                                    bytesRead += read;
                                    var progress = (int)((bytesRead * 1.0 / totalBytes) * 100);
                                    OnProgressChanged(progress);
                                }
                            } while (isMoreToRead);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is used to update ProgressBar
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void OnProgressChanged(int progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }
    }
}
