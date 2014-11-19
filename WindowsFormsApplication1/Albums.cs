using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Facebook;
using System.IO;

namespace FacebookTest
{
    public partial class Albums : Form
    {
        public string user;
        public string token;
        public string comment_id;
        public Albums()
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
                    ListAlbums.Items.Clear();

                    dynamic friendList = fb.Get("/me/albums");

                    int count = (int)friendList.data.Count;

                    for (int i = 0; i < count; i++)
                    {
                        string chuoi = friendList.data[i].id + "_" + friendList.data[i].name + "_" + friendList.data[i].count + " ảnh";
                        ListAlbums.Items.Add(chuoi);  
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


        private void Albums_Load(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            setInfor(user);
            Cursor.Current = Cursors.Default;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
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
                if (string.IsNullOrEmpty(status.Text))
                {
                    MessageBox.Show("Bạn phải nhập nội dung cần comment.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    status.Focus();
                    return;
                }
                Cursor.Current = Cursors.WaitCursor;
                FacebookClient fbClient = new FacebookClient(token);

                Dictionary<string, object> postArgs = new Dictionary<string, object>();

                postArgs["message"] = status.Text;

                if (linkText.Text.Length > 0)
                    postArgs["link"] = linkText.Text;
                fbClient.Post(String.Format("{0}/comments?message=", comment_id), postArgs);
                Cursor.Current = Cursors.Default;
                
                string name = "your";
                if (!user.ToUpper().Equals("ME"))
                    name = ListAlbums.SelectedItem.ToString() + "'s";
                MessageBox.Show("Post to " + name + " wall successful.", "Posted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FacebookOAuthException eb)
            {
                MessageBox.Show("Post failed: " + eb.Message + ".\nPlease try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Post failed: " + ex.Message + ".\nPlease try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

        private void ListAlbums_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string chuoi = ListAlbums.SelectedItem.ToString();
                string[] mang = chuoi.Split('_');
                comment_id = mang[0].ToString();
            }
            catch (Exception ex) { }
            
        }
    }
}
