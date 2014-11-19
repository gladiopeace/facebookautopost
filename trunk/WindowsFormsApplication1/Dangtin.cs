using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Facebook;
using System.Net;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Web;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Dynamic;

namespace FacebookTest
{
    public partial class Dangtin : Form
    {
        public bool isLogIn = false;
        public string user;
        public string token;
        public string comment_id = null;

        // Download Videos
        string URLWebPage = "";
        string URLToDownload;
        string FileName;
        string graphSearch = "https://graph.facebook.com/v2.0/{0}/videos?access_token={1}";
        string linkVideo = "https://www.facebook.com/photo.php?v={0}";
        string previousPage = "";
        string nextPage = "";
        Boolean isDownload = false;
        JObject jResultsSearch;

        WebClient Downloader;

        public Dangtin()
        {
            InitializeComponent();
        }
        public void setInfor(string username)
        {
            FacebookClient fb = new FacebookClient(AppSettings.Default.AccessToken);
            if (username.ToUpper().Equals("ME"))
            {
                gvComment.Rows.Clear();
                dynamic friendList = fb.Get("/me/feed");//friends groups

                int count = (int)friendList.data.Count;
                int dem = 1;
                for (int i = 0; i < count; i++)
                {
                    if (!string.IsNullOrEmpty(friendList.data[i].message))
                    {
                        gvComment.Rows.Add(dem.ToString(), friendList.data[i].message, friendList.data[i].id);
                        dem++;
                    }
                }

                dynamic myInfor = fb.Get("/me");
                info.Text = myInfor.name + "'s Information";
                image.ImageLocation = String.Format("http://graph.facebook.com/{0}/picture", myInfor.id);
                name.Text = myInfor.name;

                try
                {
                    location.Text = myInfor.location.name;
                }
                catch (System.Exception ex)
                {
                    location.Text = null;
                }

                try
                {
                    homeTown.Text = myInfor.hometown.name;
                }
                catch (System.Exception ex)
                {
                    homeTown.Text = null;
                }

                try
                {
                    link.Text = myInfor.link;
                }
                catch (System.Exception ex)
                {
                    link.Text = null;
                }
            }
            else
            {
                dynamic friendsList = fb.Get("/me/friends?fields=name, location, hometown, link");


                int count = (int)friendsList.data.Count;

                for (int i = 0; i < count; i++)
                {
                    if (friendsList.data[i].name.Equals(username))
                        count = i;
                }

                info.Text = friendsList.data[count].name + "'s Information";
                image.ImageLocation = String.Format("http://graph.facebook.com/{0}/picture", friendsList.data[count].id);
                name.Text = friendsList.data[count].name;

                try
                {
                    location.Text = friendsList.data[count].location.name;
                }
                catch (System.Exception ex)
                {
                    location.Text = null;
                }

                try
                {
                    homeTown.Text = friendsList.data[count].hometown.name;
                }
                catch (System.Exception ex)
                {
                    homeTown.Text = null;
                }

                try
                {
                    link.Text = friendsList.data[count].link;
                }
                catch (System.Exception ex)
                {
                    link.Text = null;
                }
            }
        }




        private void logIn_Click(object sender, EventArgs e)
        {
            if (!InternetConnection.IsConnectedToInternet())
            {
                MessageBox.Show("Máy tinh của bạn chưa được kết nối tới Internet.\n Vui lòng kiểm tra và thử lại sau.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (!isLogIn)
                {
                    LogInWithConfirmPermissions logIn = new LogInWithConfirmPermissions(this);
                    logIn.ShowDialog();
                    txtToken.Text = token;
                }
                else
                {
                    setInfor("me");
                    user = "me";

                }
            }
        }

        private void browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "PNG Images|*.png|JPG Images|*.jpg";

            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK && openFile.FileName.ToString().Length != 0)
            {
                picturePath.Text = openFile.FileName;
            }
        }

        private void post_Click(object sender, EventArgs e)
        {
            string strPathFile = "Log.txt";
            try
            {
                if (!InternetConnection.IsConnectedToInternet())
                {
                    MessageBox.Show("Máy tinh của bạn chưa được kết nối tới Internet.\n Vui lòng kiểm tra và thử lại sau.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(AppSettings.Default.AccessToken))
                {
                    MessageBox.Show("Bạn chưa đăng nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logIn.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtToken.Text))
                {
                    txtToken.Text = AppSettings.Default.AccessToken;
                }
                if (string.IsNullOrEmpty(status.Text))
                {
                    MessageBox.Show("Bạn phải nhập nội dung cần đăng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    status.Focus();
                    return;
                }
                // post as share link
                if (picturePath.Text.Length == 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    FacebookClient fbClient = new FacebookClient(txtToken.Text.Trim());
                    Dictionary<string, object> postArgs = new Dictionary<string, object>();
                    postArgs["message"] = status.Text;
                    if (linkText.Text.Length > 0)
                        postArgs["link"] = linkText.Text;
                    fbClient.Post(String.Format("{0}/feed", user), postArgs);
                    setInfor(user);
                    Cursor.Current = Cursors.Default;
                }
                else
                {
                    Cursor.Current = Cursors.WaitCursor;
                    FacebookClient fbClient = new FacebookClient(txtToken.Text.Trim());

                    var imgStream = File.OpenRead(picturePath.Text);
                    string mess = status.Text + "\n" + linkText.Text;
                    fbClient.Post(String.Format("{0}/photos", user), new
                    {
                        message = mess,
                        file = new FacebookMediaStream
                        {
                            ContentType = "image/jpg",
                            FileName = Path.GetFileName(picturePath.Text)
                        }.SetValue(imgStream)
                    });

                    setInfor(user);
                    Cursor.Current = Cursors.Default;
                }
                MessageBox.Show("Post to " + name.Text + " wall successful.", "Posted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("post_Click() " + ex.Message.ToString());
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            string[] theCookies = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
            foreach (string currentFile in theCookies)
            {
                try
                {
                    System.IO.File.Delete(currentFile);
                }
                catch (Exception ex)
                {
                }
            }
            Application.Exit();
        }

        private string getFBInstance()
        {
            string url = txtLink.Text.Trim();
            int index = url.LastIndexOf("/");
            return url.Substring(index);
        }

        private string getHtml()
        {
            try
            {
                HttpWebResponse getResponse = null;
                HttpWebRequest getRequest = null;
                Stream newStream = null;
                CookieCollection cookies = new CookieCollection();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.facebook.com");
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
                //Get the response from the server and save the cookies from the first request..
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                cookies = response.Cookies;

                string getUrl = txtLink.Text.Trim();
                string postData = "payoo";
                getRequest = (HttpWebRequest)WebRequest.Create(getUrl);
                getRequest.CookieContainer = new CookieContainer();
                getRequest.CookieContainer.Add(cookies); //recover cookies First request
                getRequest.Method = WebRequestMethods.Http.Post;
                getRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                getRequest.AllowWriteStreamBuffering = true;
                getRequest.ProtocolVersion = HttpVersion.Version11;
                getRequest.AllowAutoRedirect = true;
                getRequest.ContentType = "application/x-www-form-urlencoded";

                byte[] byteArray = Encoding.ASCII.GetBytes(postData);
                getRequest.ContentLength = byteArray.Length;
                newStream = getRequest.GetRequestStream(); //open connection
                newStream.Write(byteArray, 0, byteArray.Length); // Send the data.
                newStream.Close();

                getResponse = (HttpWebResponse)getRequest.GetResponse();
                StreamReader sr = new StreamReader(getResponse.GetResponseStream());
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("getHtml() " + ex.Message.ToString());
                }
                return null;
            }
        }
        #region Lay bai viet va link
        private string GetPostLink(string strHtml)
        {
            string strReturn = null;
            try
            {
                string strRegex = @"<span class=""userContent"">(.*?)</span>";
                MatchCollection matches = new Regex(strRegex, RegexOptions.Singleline | RegexOptions.Compiled).Matches(strHtml);
                //Lay bai viet co link
                foreach (Match match in matches)
                {
                    string contents = match.Groups[0].Value;
                    int so1 = contents.Length;
                    so1 = so1 - 7 - 26;
                    contents = contents.Substring(26, so1);
                    strReturn = contents;
                    //friendListbox.Items.Add(contents);
                }

            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("GetPostLink() " + ex.Message.ToString());
                }
            }
            return strReturn;
        }

        private string GetPostVideo(string strHtml)
        {
            string strReturn = null;
            try
            {
                string strRegex = @"<div class=""_wk"">(.*?)</div>";
                MatchCollection matchespost = new Regex(strRegex, RegexOptions.Singleline | RegexOptions.Compiled).Matches(strHtml);
                //Lay bai viet co video
                foreach (Match matchpost in matchespost)
                {
                    string contents = matchpost.Groups[0].Value;
                    int so1 = contents.Length;
                    so1 = so1 - 6 - 17;
                    contents = contents.Substring(17, so1);
                    strReturn = contents;
                    //friendListbox.Items.Add(contents);
                }
            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("GetPostVideo() " + ex.Message.ToString());
                }
            }
            return strReturn;
        }

        private string GetLink(string strHtml)
        {
            string strReturn = null;
            try
            {
                string strRegex3 = @"<a class=""shareLink _1y0 _5qjs""(.*?)>";
                MatchCollection matches3 = new Regex(strRegex3, RegexOptions.Singleline | RegexOptions.Compiled).Matches(strHtml);
                //Lay link
                foreach (Match match3 in matches3)
                {
                    string contents3 = match3.Groups[0].Value;
                    string strRegex4 = @"http:\\(.*?)&quot";
                    Regex myRegex = new Regex(strRegex4);
                    Match m = myRegex.Match(contents3);
                    string chuoi4 = null;
                    chuoi4 = m.Groups[0].ToString();
                    string[] chuoi2 = chuoi4.Split('\\');
                    string chuoi3 = null;
                    for (int i = 0; i < chuoi2.Length - 2; i++)
                    {
                        if (!string.IsNullOrEmpty(chuoi2[i].ToString()))
                        {
                            chuoi3 += chuoi2[i].ToString();
                        }
                    }
                    strReturn = chuoi3;
                }
            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("GetLink() " + ex.Message.ToString());
                }
            }
            return strReturn;
        }

        private string Getvideo(string strHtml)
        {
            string strReturn = null;
            try
            {
                //Lay video
                string strRegex5 = @"<a class=""uiVideoThumb videoThumb uiVideoLink""(.*?)>";
                MatchCollection matches5 = new Regex(strRegex5, RegexOptions.Singleline | RegexOptions.Compiled).Matches(strHtml);
                foreach (Match match5 in matches5)
                {
                    string contents5 = match5.Groups[0].Value;
                    string strRegex6 = @"href=""http(.*?)""";
                    MatchCollection matches6 = new Regex(strRegex6, RegexOptions.Singleline | RegexOptions.Compiled).Matches(contents5);
                    foreach (Match match6 in matches6)
                    {
                        string contents6 = match6.Groups[0].Value;
                        int so1 = contents6.Length;
                        so1 = so1 - 6;
                        contents6 = contents6.Substring(6, so1);
                        // listBox1.Items.Add(contents123);
                        strReturn = contents6;
                    }
                }
            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("Getvideo() " + ex.Message.ToString());
                }
            }
            return strReturn;
        }
        #endregion

        private void btnAlbums_Click(object sender, EventArgs e)
        {
            gvPost.Rows.Clear();
            GraphSearchFB(new Uri(String.Format(graphSearch, getFBInstance(), AppSettings.Default.AccessToken)));
        }

        private void GraphSearchFB(Uri uri)
        {
            try
            {
                if (!InternetConnection.IsConnectedToInternet())
                {
                    MessageBox.Show("Máy tinh của bạn chưa được kết nối tới Internet.\n Vui lòng kiểm tra và thử lại sau.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(AppSettings.Default.AccessToken))
                {
                    MessageBox.Show("Bạn chưa đăng nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logIn.Focus();
                    return;
                }

                loadingImage.Visible = true;
                Downloader = new WebClient();
                Downloader.DownloadStringAsync(uri);
                Downloader.DownloadStringCompleted += search_DownloadStringCompleted;
            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("btnAlbums_Click() " + ex.Message.ToString());
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        void search_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            //TODO: get search video here
            loadingImage.Visible = false;
            int i = gvPost.Rows.Count + 1;
            jResultsSearch = JObject.Parse(e.Result);
            JArray data = (JArray)jResultsSearch.GetValue("data");
            gvPost.RowTemplate.MinimumHeight = 73;
            foreach (JObject video in data)
            {
                JArray format = (JArray)video.GetValue("format");
                string picture = video.GetValue("picture").ToString();
                string file = video.GetValue("source").ToString();
                if (format.Count > 0)
                {
                    picture = ((JObject)format[0]).GetValue("picture").ToString();
                }
                try
                {
                    WebRequest req = WebRequest.Create(picture);
                    WebResponse response = req.GetResponse();
                    Stream ImgStream = response.GetResponseStream();

                    Console.WriteLine(video.GetValue("description").ToString());
                    gvPost.Rows.Add((i++).ToString(),
                        Image.FromStream(ImgStream),
                        video.GetValue("description").ToString(),
                        string.Format(linkVideo, video.GetValue("id").ToString()),
                        file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was a problem downloading the file.");
                }
            }
            gvPost.ClearSelection();
            btnLoadMore.Enabled = true;
            Downloader.DownloadStringCompleted -= search_DownloadStringCompleted;
        }

        private void btnVideo_Click(object sender, EventArgs e)
        {
            try
            {
                VideoFB al = new VideoFB();
                al.user = user;
                al.token = txtToken.Text.Trim();
                al.ShowDialog();
            }
            catch (Exception ex)
            {
            }
        }

        private void txtLink_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnAlbums_Click(null, null);
            }
        }

        private void gvPost_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string title = gvPost.CurrentRow.Cells[2].Value.ToString();
                status.Text = title;
                string chuoi1 = gvPost.CurrentRow.Cells[3].Value.ToString();
                string linkDownload = gvPost.CurrentRow.Cells[4].Value.ToString();
                chuoi1 = chuoi1.Replace('"', ' ');
                linkText.Text = chuoi1.Trim();
                linkText.Tag = linkDownload;
                URLToDownload = linkDownload;

                FileName = WebUtility.HtmlDecode(title);
                FileName = Regex.Replace(FileName, @"\s+", " ");
                FileName = Regex.Replace(FileName, "[^a-zA-Z0-9% ._]", string.Empty);
                FileName = FileName.Replace(" ", "_");
            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("gvPost_CellClick() " + ex.Message.ToString());
                }
            }
        }

        private void gvComment_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Comment cm = new Comment();
                cm.comment_id = gvComment.CurrentRow.Cells[2].Value.ToString(); ;
                cm.token = txtToken.Text.Trim();
                cm.user = user;
                cm.mess = gvComment.CurrentRow.Cells[1].Value.ToString(); ;
                cm.ShowDialog();
            }
            catch (Exception ex)
            {
            }
        }

        private void Dangtin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                exit_Click(null, null);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            post.Visible = true;
            btnDownloadVideo.Visible = false;
            label7.Text = "Picture:";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            post.Visible = false;
            btnDownloadVideo.Visible = true;
            label7.Text = "Video:";
        }

        private void btnDownloadVideo_Click(object sender, EventArgs e)
        {
            if (isDownload)
            {
                
                if (Downloader != null)
                {
                    Downloader.CancelAsync();
                    isDownload = false;
                    btnDownloadVideo.Text = "Download and Post";
                }
            }
            else
            {
                if (!InternetConnection.IsConnectedToInternet())
                {
                    MessageBox.Show("Máy tinh của bạn chưa được kết nối tới Internet.\n Vui lòng kiểm tra và thử lại sau.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(AppSettings.Default.AccessToken))
                {
                    MessageBox.Show("Bạn chưa đăng nhập.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    logIn.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(status.Text))
                {
                    MessageBox.Show("Bạn phải nhập nội dung cần đăng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    status.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(FileName))
                {
                    MessageBox.Show("You need load video first", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (Downloader = new WebClient())
                {
                    Downloader.DownloadFileAsync(new Uri(URLToDownload), Directory.GetCurrentDirectory() + "\\Facebook_Videos\\" + FileName + ".mp4");
                    Downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                    Downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                    lbStatus.Text = "Prepare download...";
                    progressBar1.Visible = true;
                    btnDownloadVideo.Text = "Cancel";
                    isDownload = true;
                }
            }
        }

        void Downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Downloader.DownloadProgressChanged -= Downloader_DownloadProgressChanged;
            Downloader.DownloadFileCompleted -= Downloader_DownloadFileCompleted;
            lbStatus.Text = "Download finished";
            progressBar1.Visible = false;
            progressBar1.Value = 0;
            picturePath.Text = Directory.GetCurrentDirectory() + "\\Facebook_Videos\\" + FileName + ".mp4";
            postVideo();
        }

        async void postVideo()
        {
            if (isDownload)
            {
                lbStatus.Text = "Posting...";
                Cursor.Current = Cursors.WaitCursor;
                FacebookClient fbClient = new FacebookClient(txtToken.Text.Trim());
                var videoStream = File.ReadAllBytes(picturePath.Text);

                dynamic parameters = new ExpandoObject();
                parameters.source = new FacebookMediaObject { 
                    ContentType = "video/mp4", 
                    FileName = Path.GetFileName(picturePath.Text) }.SetValue(videoStream);
                parameters.title = status.Text;
                //parameters.description = status.Text;

                object results = await fbClient.PostTaskAsync(String.Format("{0}/videos", user),  parameters);
                if (results != null)
                {
                    lbStatus.Text = "Post finished";
                    
                }
                btnDownloadVideo.Text = "Download and Post";
                setInfor(user);
                Cursor.Current = Cursors.Default;
                MessageBox.Show("Post video to " + name.Text + " wall successful.", "Posted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lbStatus.Text = "Received: " + e.BytesReceived + "/" + e.TotalBytesToReceive + " bytes";
            progressBar1.Value = e.ProgressPercentage;
        }

        private void Dangtin_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Facebook_Videos")))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Facebook_Videos");
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (jResultsSearch != null)
            {
                JObject paging = (JObject)jResultsSearch.GetValue("paging");
                nextPage = (string)paging.GetValue("next");
                GraphSearchFB(new Uri(nextPage));
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {

        }
    }
}
