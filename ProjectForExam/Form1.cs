using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;
using System.Threading;
using System.Net;
using System.IO;
using Image = System.Drawing.Image;
/*using VkNet.AudioBypassService.Extensions;
using Microsoft.Extensions.DependencyInjection;*/
using VkNet.Enums.SafetyEnums;

namespace ProjectForExam
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string code, idFriend;
        VkApi api = new VkApi();
        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                new Thread(() =>
                {
                    /*api.Authorize(new ApiAuthParams
                      {
                            AccessToken = ""
                      });*/
                    api.Authorize(new ApiAuthParams
                    {
                        ApplicationId = AuthData.AppID,
                        Login = ProjectForExam.AuthData.Login,
                        Password = AuthData.Password,
                        Settings = Settings.All,
                        TwoFactorAuthorization = () =>
                        {
                            return code;
                        }
                    });
                }).Start();
                Thread.Sleep(0);
            }
            catch
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }
        public void InformationAboutUser()
        {
            var me = api.Users.Get(new long[] { api.UserId.Value }, ProfileFields.Photo200Orig | ProfileFields.BirthDate).FirstOrDefault();
            label2.Text = me.FirstName;
            label3.Text = me.LastName;
            label7.Text = me.BirthDate;
            pictureBox1.ImageLocation = me.Photo200Orig.ToString();
            pictureBox1.Show();
            label7.Show();
            label4.Show();
            label2.Show();
            label3.Show();
        }
        public void GetOnlineFriends()
        {
            var friends = api.Friends.Get(new FriendsGetParams
            {
                UserId = api.UserId,
                Order = FriendsOrder.Hints,
                Fields = ProfileFields.LastName | ProfileFields.FirstName | ProfileFields.Online | ProfileFields.Photo400Orig | ProfileFields.Uid
            });
            List<String> imagelist = new List<string>();
            ImageList images = new ImageList();
            images.ImageSize = new Size(200, 200);
            images.ColorDepth = ColorDepth.Depth32Bit;
            int count = 0;
            foreach (var friend in friends)
            {
                if (friend.Online == true)
                {
                    imagelist.Add(friend.Photo400Orig.ToString());
                    WebClient w = new WebClient();
                    byte[] imageByte = w.DownloadData(imagelist[count]);
                    MemoryStream stream = new MemoryStream(imageByte);
                    Image img = Image.FromStream(stream);
                    images.Images.Add(img);

                    listView1.Items.Add(friend.LastName + " " + friend.FirstName + " " + friend.Id, count++);
                }
                listView2.Items.Add(friend.LastName + " " + friend.FirstName + " " + friend.Id);
                listView1.LargeImageList = images;
            }
            label5.Show();
            label6.Show();
            listView2.Show();
            listView1.Show();
            button5.Show();
            button4.Show();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            code = textBox1.Text;
            button3.Show();
            button6.Show();
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            idFriend = e.Item.Text.Substring(e.Item.Text.LastIndexOf(" "));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var friendLike = api.Users.Get(new long[] { long.Parse(idFriend) }, ProfileFields.PhotoId).FirstOrDefault();
            long itemId = long.Parse(friendLike.PhotoId.Substring(friendLike.PhotoId.LastIndexOf("_")).Trim(new char[] { '_' }));
            //label4.Text = itemId.ToString() + " <- PHOTO ID FRIEND -> " + idFriend;
            var add = api.Likes.Add(new LikesAddParams
            {
                Type = LikeObjectType.Photo,
                ItemId = itemId,
                OwnerId = int.Parse(idFriend)
            });
        }

        private void button6_Click(object sender, EventArgs e)
        {
            InformationAboutUser();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                var friendLike = api.Users.Get(new long[] { long.Parse(idFriend) }, ProfileFields.PhotoId).FirstOrDefault();
                long itemId = long.Parse(friendLike.PhotoId.Substring(friendLike.PhotoId.LastIndexOf("_")).Trim(new char[] { '_' }));
                //label4.Text = itemId.ToString() + " <- PHOTO ID FRIEND -> " + idFriend;
                var delete = api.Likes.Delete(type: LikeObjectType.Photo, itemId: itemId, ownerId: int.Parse(idFriend));
            }
            catch
            {
                MessageBox.Show("Лайка нет");
            }
        }

        private void listView2_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            idFriend = e.Item.Text.Substring(e.Item.Text.LastIndexOf(" "));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listView2.Items.Clear();
            listView1.Items.Clear();
            GetOnlineFriends();
        }

    }
}
