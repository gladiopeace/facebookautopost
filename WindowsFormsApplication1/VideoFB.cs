using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Facebook;

namespace FacebookTest
{
    public partial class VideoFB : Form
    {
        public string user;
        public string token;
        public VideoFB()
        {
            InitializeComponent();
        }

        public void setInfor(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    token = AppSettings.Default.AccessToken;
                }
                FacebookClient fb = new FacebookClient(token);
                if (username.ToUpper().Equals("ME"))
                {
                    ListVideo.Items.Clear();

                    dynamic friendList = fb.Get("/me/videos");

                    int count = (int)friendList.data.Count;

                    for (int i = 0; i < count; i++)
                    {
                        ListVideo.Items.Add(friendList.data[i].id + "_" + friendList.data[i].name + "_" + friendList.data[i].count + " ảnh");
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
            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("setInfor() " + ex.Message.ToString());
                }
            }
        }

        private void VideoFB_Load(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            setInfor(user);
            Cursor.Current = Cursors.Default;
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
