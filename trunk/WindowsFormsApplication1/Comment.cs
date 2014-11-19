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
    public partial class Comment : Form
    {
        public string user = "me";
        public string comment_id ;
        public string token;
        public string mess;
       
        public Comment()
        {
            InitializeComponent();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Comment_Load(object sender, EventArgs e)
        {
            txtToken.Text = token;
            setInfor(user);
            txtMess.Text = mess;
        }

        public void setInfor(string username)
        {
            if (string.IsNullOrEmpty(txtToken.Text))
            {
                txtToken.Text = AppSettings.Default.AccessToken;
            }
            FacebookClient fb = new FacebookClient(txtToken.Text);
            try
            {
                if (username.ToUpper().Equals("ME"))
                {
                    gv_Comment.Rows.Clear();
                    dynamic friendList = fb.Get(comment_id);
                    int count = (int)friendList.comments.data.Count;
                    for (int i = 0; i < count; i++)
                    {
                        gv_Comment.Rows.Add((i + 1).ToString(), friendList.comments.data[i].from.name, friendList.comments.data[i].message, friendList.comments.data[i].id);
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
            finally
            {
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

        private void post_Click(object sender, EventArgs e)
        {
            try
            {
                if (!InternetConnection.IsConnectedToInternet())
                {
                    MessageBox.Show("Máy tinh của bạn chưa được kết nối tới Internet.\n Vui lòng kiểm tra và thử lại sau.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(status.Text))
                {
                    MessageBox.Show("Bạn phải nhập nội dung cần đăng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    status.Focus();
                    return;
                }
                if (picturePath.Text.Length == 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    FacebookClient fbClient = new FacebookClient(txtToken.Text.Trim());

                    Dictionary<string, object> postArgs = new Dictionary<string, object>();

                    postArgs["message"] = status.Text;

                    if (linkText.Text.Length > 0)
                        postArgs["link"] = linkText.Text;
                    fbClient.Post(String.Format("{0}/comments?message=", comment_id), postArgs);

                    Cursor.Current = Cursors.Default;
                }
                else
                {
                    Cursor.Current = Cursors.WaitCursor;
                    FacebookClient fbClient = new FacebookClient(txtToken.Text.Trim());
                    var imgStream = File.OpenRead(picturePath.Text);
                    string mess = status.Text + "\n" + linkText.Text;
                    fbClient.Post(String.Format("{0}/comments?message=", comment_id), new
                    {
                        message = mess,
                        file = new FacebookMediaStream
                        {
                            ContentType = "image/jpg",
                            FileName = Path.GetFileName(picturePath.Text)
                        }.SetValue(imgStream)
                    });
                    Cursor.Current = Cursors.Default;
                }
                MessageBox.Show("Post to " + name.Text + " wall successful.", "Posted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string strPathFile = "Log.txt";
                using (System.IO.StreamWriter w = new System.IO.StreamWriter(strPathFile, false))
                {
                    w.WriteLine("post_Click() " + ex.Message.ToString());
                }
            }
            finally
            {
                setInfor(user);
                Cursor.Current = Cursors.Default;
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

        private void gv_Comment_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                comment_id = gv_Comment.CurrentRow.Cells[3].Value.ToString();
            }
            catch (Exception ex) { }
        }

        private void Comment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}
