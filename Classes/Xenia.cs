using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Xenia_Manager.Classes
{
    public class Xenia
    {
        public int versionID;
        public event EventHandler<int> ProgressChanged;

        public Xenia(int versionID)
        {
            this.versionID = versionID;
        }

        public async Task DownloadFileAsync(string url, string savePath)
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

        protected virtual void OnProgressChanged(int progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }
    }
}
