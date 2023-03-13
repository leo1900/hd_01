
using BestHTTP;

namespace FrameWork.Asset
{
    public class DownloadingTask
    {
        public DownloadingTask(DownloadInfo info)
        {
            this.DownloadInfo = info;
        }

        // 任务数据
        public DownloadInfo DownloadInfo { private set; get; }

        // 下载器
        public BreakPointDownloader Downloader;

        // http head
        public HTTPRequest HeadRequest;
    }
}
