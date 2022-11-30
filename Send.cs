using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TeleSharp.TL;
using TeleSharp.TL.Contacts;
using TeleSharp.TL.Upload;
using TLSharp.Core;

namespace WinApp
{
    public partial class Send : Form
    {
        int apiId = 7547006; //nhap id của bạn
        string apiHash = "dc0e5e7f3b17254634ee1fe249d30685";
        TelegramClient client;
        string myPhoneNumber = "+8801750675743";
        string hash;
        TLContacts tLContact;
        string userIDSelected;

        public Send()
        {
            InitializeComponent();
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                File.Delete("session.dat");
                client = new TelegramClient(apiId, apiHash, dcIpVersion: DataCenterIPVersion.OnlyIPv4);
                await client.ConnectAsync();
                hash = await client.SendCodeRequestAsync(myPhoneNumber);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async void btnAuth_Click(object sender, EventArgs e)
        {
            await client.MakeAuthAsync(myPhoneNumber, hash, txtCode.Text);
            tLContact = await client.GetContactsAsync();

            foreach (TLUser user in tLContact.Users)
            {
                if (!string.IsNullOrEmpty(user.Phone))
                {
                    var fullName = $"{user.FirstName} {user.LastName}";
                    var phone = user.Phone;
                    string[] row = { fullName, phone, user.Id.ToString() };
                    var listviewItem = new ListViewItem(row);
                    listContact.Items.Add(listviewItem);
                }
            }
        }

        public Image ReadImageFromUser(TLUser user)
        {
            if (user.Photo == null) return Properties.Resources.no_avatar;
            var photo = ((TLUserProfilePhoto)user.Photo);
            var photoLocation = (TLFileLocation)photo.PhotoBig;

            TLFile file = client.GetFile(new TLInputFileLocation()
            {
                LocalId = photoLocation.LocalId,
                Secret = photoLocation.Secret,
                VolumeId = photoLocation.VolumeId
            }, 1024 * 256).Result;

            Image image;
            using (var m = new MemoryStream(file.Bytes))
            {
                image = Image.FromStream(m);
            }
            return image;
        }

        private void listContact_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listContact.SelectedItems.Count == 0)
                return;
            ListViewItem item = listContact.SelectedItems[0];
            var phone = item.SubItems[1].Text;
            userIDSelected = item.SubItems[2].Text;

            var user = tLContact.Users
               .Where(x => x.GetType() == typeof(TLUser))
               .Cast<TLUser>()
               .FirstOrDefault(x => x.Phone == phone);

            //var userPhoto = ReadImageFromUser(user);
            //picPhoto.Image = userPhoto;
        }

        private async void btnSendMessage_Click(object sender, EventArgs e)
        {
            await client.SendMessageAsync(new TLInputPeerUser() { UserId = Convert.ToInt32(userIDSelected) }, txtMessage.Text);
        }
    }
}
