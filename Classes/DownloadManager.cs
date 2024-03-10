using System;
using System.IO;
using System.Net.Http;
using System.Windows.Controls;
using System.IO.Compression;
using System.Threading.Tasks;

// Imported
using Serilog;


namespace Xenia_Manager.Classes
{
    public class DownloadManager
    {
        /// <summary>
        /// ProgressBar element
        /// </summary>
        private readonly ProgressBar _progressBar;
        /// <summary>
        /// URL of the download
        /// </summary>
        private readonly string _downloadUrl;
        /// <summary>
        /// Where the download is stored
        /// </summary>
        private readonly string _downloadPath;

        public DownloadManager(ProgressBar? progressBar, string downloadUrl, string downloadPath)
        {
            _progressBar = progressBar;
            _downloadUrl = downloadUrl;
            _downloadPath = downloadPath;
        }

        /// <summary>
        /// Used for downloading Xenia builds and extracting them
        /// </summary>
        /// <param name="url">URL where the build is stored</param>
        /// <param name="savePath">Where we want to save the build</param>
        /// <returns></returns>
        public async Task DownloadAndExtractAsync()
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();
                    var totalBytes = response.Content.Headers.ContentLength ?? -1;
                    var downloadedBytes = 0L;

                    using (var fileStream = new FileStream(_downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var buffer = new byte[8192];
                        var bytesRead = 0;
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                downloadedBytes += bytesRead;
                                if (totalBytes > 0)
                                {
                                    var progress = (int)Math.Round((double)downloadedBytes / totalBytes * 100);
                                    UpdateProgressBar(progress);
                                }
                            }
                        }
                    }
                }
                Log.Information("Download completed. Extracting.");

                ZipFile.ExtractToDirectory(_downloadPath, Path.GetDirectoryName(_downloadPath) + @"\Xenia\", true);
                Log.Information("Extraction done. Deleting the zip file.");

                File.Delete(_downloadPath);
                Log.Information("Deleting the zip file done.");
            }
        }

        /// <summary>
        /// This is used to update ProgressBar
        /// </summary>
        /// <param name="progress"></param>
        private void UpdateProgressBar(int progress)
        {
            if (_progressBar != null)
            {
                if (_progressBar.Dispatcher.CheckAccess())
                {
                    _progressBar.Value = progress;
                }
                else
                {
                    _progressBar.Dispatcher.Invoke(() => _progressBar.Value = progress);
                }
            }
        }
    }
}
