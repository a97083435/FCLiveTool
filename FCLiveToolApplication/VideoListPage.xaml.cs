using Microsoft.Maui.Platform;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;

namespace FCLiveToolApplication;

public partial class VideoListPage : ContentPage
{
    public VideoListPage()
    {
        InitializeComponent();
    }
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        LoadVideos();
        DeviceDisplay.MainDisplayInfoChanged+=DeviceDisplay_MainDisplayInfoChanged;
    }

    private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
    {
#if ANDROID

        if (DeviceDisplay.Current.MainDisplayInfo.Orientation==DisplayOrientation.Landscape)
        {

        }
        else
        {

        }
#endif
    }

    public string AllVideoData;
    public int[] UseGroup;
    private void Question_Clicked(object sender, EventArgs e)
    {
        ImageButton button = sender as ImageButton;

        switch (button.StyleId)
        {
            case "VideoListQue":
                DisplayAlert("��������", "����ֱ��Դ�����������磬���ǲ���ֱ��������ݸ���������Ȩ����ϵ����ɾ��", "�ر�");
                break;
            case "VideosQue":
                DisplayAlert("������Ϣ", "��ܰ��ʾ������ĳЩM3U8��URLʹ�õ�����Ե�ַ�����Ǿ��Ե�ַ����APP�޷����ţ�������APP��bug����֪Ϥ", "�ر�");
                break;
        }
    }
    private void DownloadBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string buttonName = button.StyleId.Replace("DOWNB", "");
    }
    private void EditBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string buttonName = button.StyleId.Replace("EB", "");
    }
    private void DeleteBtn_Clicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        string buttonName = button.StyleId.Replace("DELB", "");
    }

    private async void VideosList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        VideoDataListRing.IsRunning = true;
        try
        {
            VideoList videoList = e.Item as VideoList;
            AllVideoData = await new HttpClient().GetStringAsync("https://fclivetool.com/api/APPGetVD?url="+videoList.SourceLink);

            VideoDetailList.ItemsSource= DoRegex(AllVideoData, videoList.RecommendReg);
        }
        catch (Exception)
        {
            await DisplayAlert("��ʾ��Ϣ", "��ȡ����ʧ�ܣ����Ժ����ԣ�", "ȷ��");

            /*
                        var link = videoList.SourceLink;
                        if (link.Contains("githubusercontent.com")||link.Contains("github.com"))
                        {
                            await DisplayAlert("��ʾ��Ϣ", "�����ڵĵ�������GitHub���ܲ�˳��", "ȷ��");
                        }
                        else
                        {
                            await DisplayAlert("��ʾ��Ϣ", "��ȡ����ʧ�ܣ����Ժ����ԣ�", "ȷ��");
                        }
            
             */
        }

        VideoDataListRing.IsRunning = false;
    }

    private void VideoDetailList_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        VideoDetailList detail = e.Item as VideoDetailList;

        VideoPrevPage.videoPrevPage.VideoWindow.Source=detail.SourceLink;
        VideoPrevPage.videoPrevPage.VideoWindow.Play();
        VideoPrevPage.videoPrevPage.NowPlayingTb.Text=detail.SourceName;
    }

    public async void LoadVideos()
    {
        try
        {
            string videodata = await new HttpClient().GetStringAsync("https://fclivetool.com/api/GetVList?pindex=1");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<VideoList>));

            VideosList.ItemsSource= (List<VideoList>)xmlSerializer.Deserialize(new StringReader(videodata));
        }
        catch (Exception)
        {
            await DisplayAlert("��ʾ��Ϣ", "��ȡ����ʧ�ܣ����Ժ����ԣ�", "ȷ��");
        }

        VideosListRing.IsRunning=false;
    }

    public List<VideoDetailList> DoRegex(string videodata, string recreg)
    {
        MatchCollection match = Regex.Matches(videodata, UseRegex(recreg));

        List<VideoDetailList> result = new List<VideoDetailList>();
        for (int i = 0; i<match.Count(); i++)
        {
            VideoDetailList videoDetail = new VideoDetailList()
            {
                //ID��̨�̨꣬����ֱ��Դ��ַ
                Id=i,
                LogoLink=match[i].Groups[UseGroup[0]].Value=="" ? "fclive_tvicon.png" : match[i].Groups[UseGroup[0]].Value,
                SourceName=match[i].Groups[UseGroup[1]].Value,
                SourceLink=match[i].Groups[UseGroup[2]].Value
            };
            //videoDetail.LogoLink=videoDetail.LogoLink=="" ? "fclive_tvicon.png" : videoDetail.LogoLink;

            result.Add(videoDetail);
        }

        return result;
    }
    public string UseRegex(string index)
    {
        switch (index)
        {
            case "1":
                UseGroup =new int[] { 1, 2, 3 };
                return @"(?:.*?tvg-logo=""([^""]*)"")?(?:.*?tvg-name=""([^""]*)"")?.*\r?\n?((http|https)://\S+\.m3u8(\?(.*?))?(?=\n|,))";
            //1.2Ϊ1�Ĳ�����M3U8��׺�İ汾
            case "1.2":
                UseGroup =new int[] { 1, 2, 3 };
                return @"(?:.*?tvg-logo=""([^""]*)"")?(?:.*?tvg-name=""([^""]*)"")?.*\r?\n?((http|https)://\S+(.*?)(?=\n|,))";
            //ֻ��2��������ƥ��̨����ƥ��̨��
            case "2":
                UseGroup =new int[] { 2, 1, 3 };
                return @"(?:.*?tvg-name=""([^""]*)"")(?:.*?tvg-logo=""([^""]*)"")?.*\r?\n?((http|https)://\S+\.m3u8(\?(.*?))?(?=\n|,))";
            //2.2Ϊ2�Ĳ�����M3U8��׺�İ汾
            case "2.2":
                UseGroup =new int[] { 2, 1, 3 };
                return @"(?:.*?tvg-name=""([^""]*)"")(?:.*?tvg-logo=""([^""]*)"")?.*\r?\n?((http|https)://\S+(.*?)(?=\n|,))";
            case "3":
                UseGroup =new int[] { 3, 5, 8 };
                return @"((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+\.m3u8(\?(.*?))?(?=\n|,)))";
            //3.2Ϊ3�Ĳ�����M3U8��׺�İ汾
            case "3.2":
                UseGroup =new int[] { 3, 5, 8 };
                return @"((tvg-logo=""([^""]*)"")(.*?))?,(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n|,)))";
            case "4":
                UseGroup =new int[] { 3, 5, 8 };
                return @",?((tvg-logo=""([^""]*)"")(.*?)),(.+?)(,)?(\n)?(?=((http|https)://\S+(.*?)(?=\n|,)))";
            case "5":
                UseGroup =new int[] { 2, 5, 7 };
                return @"(((http|https)://\S+)(,))?(.*?)(,)((http|https)://\S+(?=\n|,|\s{1}))";
            default:
                return "";
        }

    }

}