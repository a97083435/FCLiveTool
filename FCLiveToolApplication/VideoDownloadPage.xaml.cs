using CommunityToolkit.Maui.Storage;

namespace FCLiveToolApplication;

public partial class VideoDownloadPage : ContentPage
{
	public VideoDownloadPage()
	{
		InitializeComponent();
	}
    //public long ReceiveSize = 0;
    //public long AllFilesize = 0;
    //public double DownloadProcess;
    //public int ThreadNum = 1;
    //public List<ThreadInfo> threadinfos;
    public List<VideoManager> DownloadTaskList=new List<VideoManager>();
    public List<DownloadVideoFileList> DownloadFileLists=new List<DownloadVideoFileList>();

    private void SelectLocalM3U8FileBtn_Clicked(object sender, EventArgs e)
    {

    }

    private void SelectLocalM3U8FolderBtn_Clicked(object sender, EventArgs e)
    {

    }

    private async void M3U8AnalysisBtn_Clicked(object sender, EventArgs e)
    {
        if(M3U8SourceRBtn1.IsChecked)
        {
            if (string.IsNullOrWhiteSpace(M3U8SourceURLTb.Text))
            {
                await DisplayAlert("��ʾ��Ϣ", "������ֱ��ԴM3U8��ַ��", "ȷ��");
                return;
            }
            if (!M3U8SourceURLTb.Text.Contains("://"))
            {
                await DisplayAlert("��ʾ��Ϣ", "��������ݲ�����URL�淶��", "ȷ��");
                return;
            }

            VideoAnalysisList videoAnalysisList=new VideoAnalysisList();         
            string readresult = await new VideoManager().DownloadAndReadM3U8FileForDownloadTS(videoAnalysisList, new string[] { M3U8SourceURLTb.Text });
            if (readresult != "")
            {
                await DisplayAlert("��ʾ��Ϣ", readresult, "ȷ��");
                return;
            }

            List<VideoAnalysisList> videoAnalysisLists= new List<VideoAnalysisList>();
            if(VideoAnalysisList.ItemsSource!=null)
            {
                videoAnalysisLists=VideoAnalysisList.ItemsSource.Cast<VideoAnalysisList>().ToList();

                var titem = videoAnalysisLists.FirstOrDefault(p => p.FullURL == videoAnalysisList.FullURL);
                if (titem!=null)
                {
                    videoAnalysisLists.Remove(titem);
                }

            }
            videoAnalysisLists.Add(videoAnalysisList);


            VideoAnalysisList.ItemsSource = videoAnalysisLists;

        }
        else if (M3U8SourceRBtn2.IsChecked)
        {

        }


    }

    private void M3U8SourceRBtn_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        RadioButton entry = sender as RadioButton;
        
        if(entry.StyleId=="M3U8SourceRBtn1")
        {
            M3U8SourceURLTb.IsVisible = true;
            LocalM3U8SelectPanel.IsVisible=false;
        }
        else if (entry.StyleId=="M3U8SourceRBtn2")
        {
            M3U8SourceURLTb.IsVisible = false;
            LocalM3U8SelectPanel.IsVisible=true;
        }

    }

    private async void M3U8DownloadBtn_Clicked(object sender, EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(SaveDownloadFolderTb.Text))
        {
            await DisplayAlert("��ʾ��Ϣ", "����ѡ�������ļ�Ҫ�����λ�ã�", "ȷ��");
            return; 
        }
        if(!Directory.Exists(SaveDownloadFolderTb.Text))
        {
            await DisplayAlert("��ʾ��Ϣ", "��ǰ�����ļ�����λ�õ�Ŀ¼�����ڣ�������ѡ��", "ȷ��");
            return;
        }
        if (VideoAnalysisList.ItemsSource is null)
        {
            await DisplayAlert("��ʾ��Ϣ", "��ǰ�б�Ϊ�գ����Ȼ�ȡһ��M3U8ֱ��Դ��", "ȷ��");
            return;
        }

        var tlist = VideoAnalysisList.ItemsSource.Cast<VideoAnalysisList>().Where(p=>p.IsSelected).ToList();
        for(int i = 0; i < tlist.Count; i++)
        {
            var tobj = tlist[i];
            VideoManager vmanager = new VideoManager();
            if (DownloadFileLists.Where(p => p.M3U8FullLink==tobj.FullURL&&p.CurrentActiveObject.isContinueDownloadStream).Count()>0)
            {
                await DisplayAlert("��ʾ��Ϣ", "�㵱ǰ�����������M3U8ֱ������"+tobj.FileName+" �������ظ���������", "ȷ��");
                continue;
            }
            DownloadFileLists.Add(new DownloadVideoFileList() { SaveFilePath=SaveDownloadFolderTb.Text, CurrentVALIfm=tobj, CurrentActiveObject=vmanager });

            new Thread(async () =>
            {
                string r = await vmanager.DownloadM3U8Stream(tobj, SaveDownloadFolderTb.Text + "\\",true);
                if (r != "")
                {
                    MainThread.BeginInvokeOnMainThread(()=>
                    {
                        DisplayAlert("��ʾ��Ϣ",tobj.FileName+"\n"+ r, "ȷ��");
                    });

                }
            }).Start();
        }

        DownloadVideoFileList.ItemsSource = DownloadFileLists;

    }

    private void VideoAnalysisListCB_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if(VideoAnalysisList.ItemsSource is null)
        {
            return;
        }

        var tlist= VideoAnalysisList.ItemsSource.Cast<VideoAnalysisList>().ToList();
        tlist.ForEach(p => { p.IsSelected=e.Value; });
        VideoAnalysisList.ItemsSource=tlist;
    }

    /*
         public async Task<string> DownloadM3U8Stream(List<M3U8_TS_PARM> mlist,string savepath,string filename)
        {
            threadinfos= new List<ThreadInfo>();
            int FileIndex = 0;

            foreach (var m in  mlist)
            {
                string url = m.FullURL;
                using (HttpClient httpClient = new HttpClient())
                {
                    int statusCode;
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(@"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
                    HttpResponseMessage response = null;

                    try
                    {
                        response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

                        statusCode=(int)response.StatusCode;
                        if (!response.IsSuccessStatusCode)
                        {
                            return "����ʧ�ܣ�"+"HTTP������룺"+statusCode;
                        }

                        AllFilesize = response.Content.Headers.ContentLength??-1;
                        if (AllFilesize<=0)
                        {
                            return "�޷��� ContentLength �л�ȡ��Ч���ļ���С��";
                        }

                    }
                    catch (Exception)
                    {
                        return "�������쳣��";
                    }


                    List<Task> taskList = new List<Task>();
                    int FinishTaskCount = 0;
                    ReceiveSize=0;

                    //�˴�Ҫ����������ȡM3U8����

                    long pieceSize = (long)AllFilesize / ThreadNum + (long)AllFilesize % ThreadNum;
                    for (int i = 0; i < ThreadNum; i++)
                    {
                        ThreadInfo currentThread = new ThreadInfo();
                        currentThread.ThreadId = i;
                        currentThread.ThreadStatus = false;

                        currentThread.TmpFileName = string.Format($"{savepath}TMP{FileIndex}_{filename}.tmp");
                        currentThread.Url = url;
                        currentThread.FileName = filename+".mp4";

                        long startPosition = (i * pieceSize);
                        currentThread.StartPosition = startPosition == 0 ? 0 : startPosition + 1;
                        currentThread.FileSize = startPosition + pieceSize;

                        threadinfos.Add(currentThread);

                        taskList.Add(Task.Factory.StartNew(async () =>
                        {
                            string r = await ReceiveHttp(currentThread);
                            if (r!="")
                            {
                                await DisplayAlert("��ʾ��Ϣ", filename+"\n"+ r, "ȷ��");
                            }

                            FinishTaskCount++;
                        }));
                        FileIndex++;
                    }


                    while (true)
                    {
                        if (FinishTaskCount==taskList.Count)
                        {
                            break;
                        }
                    }    

                }
            }




            MergeFile(savepath+filename+".mp4");
            threadinfos.Clear();

            return "";
        }
        public async Task<string> ReceiveHttp(object thread)
        {
            FileStream fs = null;
            Stream ns = null;
            try
            {
                ThreadInfo currentThread = (ThreadInfo)thread;

                //���������ļ��Ѵ��ڵ��ж�
                if (!File.Exists(currentThread.FileName))
                {
                    fs = new FileStream(currentThread.TmpFileName, FileMode.Create);


                    using (HttpClient httpClient = new HttpClient())
                    {
                        int statusCode;
                        httpClient.DefaultRequestHeaders.Add("Accept-Ranges", "bytes");
                        httpClient.DefaultRequestHeaders.Add("Range", "bytes="+currentThread.StartPosition+"-"+(currentThread.FileSize));
                        HttpResponseMessage response = null;

                        try
                        {
                            response = await httpClient.GetAsync(currentThread.Url);

                            statusCode=(int)response.StatusCode;
                            if (!response.IsSuccessStatusCode)
                            {
                                return "����ʧ�ܣ�"+"HTTP������룺"+statusCode;
                            }

                        }
                        catch (Exception)
                        {
                            return "�������쳣��";
                        }


                       ns = await response.Content.ReadAsStreamAsync();
                       ns.CopyTo(fs);
                    }

                    ReceiveSize += ns.Length;
                    double percent = (double)ReceiveSize / (double)AllFilesize * 100;

                    DownloadProcess=percent;

                    //byte[] buffer = new byte[ns.Length];
                    //int readSize = ns.Read(buffer, 0, buffer.Length);
                    //while (readSize > 0)
                    //{
                    //    fs.Write(buffer, 0, readSize);

                    //    buffer = new byte[buffer.Length];

                    //    readSize = ns.Read(buffer, 0, buffer.Length);

                    //    ReceiveSize += readSize;
                    //    double percent = (double)ReceiveSize / (double)AllFilesize * 100;

                    //    DownloadProcess=percent;
                    //}
                }
                currentThread.ThreadStatus = true;
            }
            catch 
            {
                return "����ʱ�����쳣";
            }
            finally
            {
                fs?.Close();
                ns?.Close();
            }

            return "";
        }

        public class ThreadInfo
        {
            public int ThreadId { get; set; }
            public bool ThreadStatus { get; set; }
            public long StartPosition { get; set; }
            public long FileSize { get; set; }
            public string Url { get; set; }
            public string TmpFileName { get; set; }
            public string FileName { get; set; }
            public int Times { get; set; }
        }

        private void MergeFile(string filepath)
    {
        string downFileNamePath = filepath;
        int length = 0;
        using (FileStream fs = new FileStream(downFileNamePath, FileMode.Create))
        {
            foreach (var item in threadinfos.OrderBy(o => o.ThreadId))
            {
                if (!File.Exists(item.TmpFileName)) continue;
                var tempFile = item.TmpFileName;

                try
                {
                    using (FileStream tempStream = new FileStream(tempFile, FileMode.Open))
                    {
                        byte[] buffer = new byte[tempStream.Length];

                        while ((length = tempStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fs.Write(buffer, 0, length);
                        }
                        tempStream.Flush();
                    }

                }
                catch
                {

                }

                try
                {
                    File.Delete(item.TmpFileName);
                }
                catch (Exception)
                {

                }

            }
        }

    }
     */

    private async void SelectSaveDownloadFolderBtn_Clicked(object sender, EventArgs e)
    {
        var folderPicker = await FolderPicker.PickAsync(FileSystem.AppDataDirectory, CancellationToken.None);

        if (folderPicker.IsSuccessful)
        {
            SaveDownloadFolderTb.Text=folderPicker.Folder.Path;
        }
        else
        {
            await DisplayAlert("��ʾ��Ϣ", "����ȡ���˲�����", "ȷ��");
        }
    }

    private void DownloadFileListCB_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (DownloadVideoFileList.ItemsSource is null)
        {
            return;
        }

        var tlist = DownloadVideoFileList.ItemsSource.Cast<DownloadVideoFileList>().ToList();
        tlist.ForEach(p => { p.IsSelected = e.Value; });
        DownloadVideoFileList.ItemsSource = tlist;
    }

    private async void DownloadFileStopBtn_Clicked(object sender, EventArgs e)
    {
        if (DownloadVideoFileList.ItemsSource is null)
        {
            await DisplayAlert("��ʾ��Ϣ", "��ǰ�б�Ϊ�գ�", "ȷ��");
            return;
        }

        var tlist=DownloadVideoFileList.ItemsSource.Cast<DownloadVideoFileList>().Where(p => p.IsSelected).ToList();
        if(tlist.Count<1)
        {
            await DisplayAlert("��ʾ��Ϣ", "�������ٹ�ѡһ��Ҫֹͣ������", "ȷ��");
            return;
        }

        tlist.ForEach(p =>
        {
            var tl= DownloadFileLists.Where(p2 => p2 == p).FirstOrDefault();
            tl.CurrentActiveObject.isContinueDownloadStream = false;
            //DownloadFileLists.Remove(tl);
        });
        DownloadFileLists.RemoveAll(p => !p.CurrentActiveObject.isContinueDownloadStream || p.CurrentActiveObject.isEndList);
        

        DownloadVideoFileList.ItemsSource = DownloadFileLists;
        await DisplayAlert("��ʾ��Ϣ", "��ֹͣѡ��������", "ȷ��");

    }
}