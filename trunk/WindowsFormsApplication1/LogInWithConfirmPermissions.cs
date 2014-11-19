using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Windows.Forms;
using Facebook;
using System.Threading;

namespace FacebookTest
{
    public partial class LogInWithConfirmPermissions : Form
    {
        private Dangtin main;

        public LogInWithConfirmPermissions(Dangtin main)
        {
            InitializeComponent();
            this.main = main;
        }


        private void LogInWithConfirmPermissions_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate(@"https://www.facebook.com/dialog/oauth?client_id=1502044273348544" +
                                 "&redirect_uri=https://www.facebook.com/connect/login_success.html&response_type=token&scope=publish_stream,read_stream,user_events,user_photos");
        }

        private void webBrowser1_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.Url.AbsoluteUri.Contains("access_token"))
            {
                string url1 = webBrowser1.Url.AbsoluteUri;
                string url2 = url1.Substring(url1.IndexOf("access_token") + 13);
                AppSettings.Default.AccessToken = url2.Substring(0, url2.IndexOf("&"));
                main.logIn.Text = "Home";
                main.post.Enabled = true;
                main.please.Visible = false;

                main.isLogIn = true;
                main.setInfor("me");
                main.user = "me";
                main.token = AppSettings.Default.AccessToken;
                Close();
            }
        }

        
    }
}
