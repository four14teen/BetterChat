using HAlerts;
using Sulakore.Communication;
using Sulakore.Habbo;
using Sulakore.Modules;
using Sulakore.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tangine;

namespace BetterChat
{
    [Module("BetterChat", "Adds chat notifications to Habbo!")]
    [Author("Mika", HabboName = "M-ka", Hotel = HHotel.Nl, ResourceName = "Twitter", ResourceUrl = "https://www.twitter.com/Metoniem")]
    public partial class MainFrm : ExtensionForm
    {
        [MessageId("3ed1b2efef33a502709fdf49436f3e37")]
        public ushort HabboAlert { get; set; }
        [MessageId("4cd9a988356735f867f5c2080c8521ef")]
        public ushort TriggerEvent { get; set; }

        public override bool IsRemoteModule => true;

        public List<BasicUser> FriendList { get; set; }
        public List<(int UserId, string Message)> PendingAlerts { get; set; }

        public const string EXIT_ICON_URL = "http://habboo-a.akamaihd.net/c_images/album1584/UK369.gif";

        public MainFrm()
        {
            InitializeComponent();
            FriendList = new List<BasicUser>();
            PendingAlerts = new List<(int UserId, string Message)>();
        }

        [InDataCapture("UserProfile")]
        public async void IncomingUserProfile(DataInterceptedEventArgs e)
        {
            e.Continue();

            BasicUser user = new BasicUser(e.Packet.ReadInteger(), e.Packet.ReadString(), e.Packet.ReadString());

            if (!FriendList.Any(f => f.Id == user.Id))
                FriendList.Add(user);

            var pendingMessages = PendingAlerts.FindAll(f => f.UserId == user.Id);

            foreach (var pendingMessage in pendingMessages)
            {
                await SendAlert(user, pendingMessage.Message);
                PendingAlerts.Remove(pendingMessage);
            }

        }

        [InDataCapture("feb75b1e16f6f03dada3e349b81cfa25")]
        public async void IncomingPrivateMessage(DataInterceptedEventArgs e)
        {
            e.Continue();

            int id = e.Packet.ReadInteger();
            string message = e.Packet.ReadString();

            BasicUser user = FriendList.Find(f => f.Id == id);

            if (user != null)
                await SendAlert(user, message);
            else
            {
                PendingAlerts.Add((id, message));
                await Connection.SendToServerAsync(Out.RequestUserProfile, id, false);
            }
        }

        [OutDataCapture("RoomUserTalk")]
        public async void OutgoingSpeech(DataInterceptedEventArgs e)
        {
            e.Continue();

            if (e.Packet.ReadString().ToLower() == ":closebc")
            {
                e.IsBlocked = true;
                HAlert alert = HAlertBuilder.CreateAlert(HAlertType.Bubble, "BetterChat has been closed!").ImageUrl(EXIT_ICON_URL);
                await Connection.SendToClientAsync(alert.ToPacket(HabboAlert));

                Close();
            }
        }

        public async Task SendAlert(BasicUser user, string message)
        {
            HAlert alert = HAlertBuilder.CreateAlert(HAlertType.Bubble, $"{user.Username} said:\n\n{message}")
                .EventUrl(HabboEvents.ShowPlayerChat(user.Id))
                .ImageUrl(HabboLook.GetImagingUrl(user.Figure, Hotel));

            await Connection.SendToClientAsync(alert.ToPacket(HabboAlert));
        }

        private async void MainFrm_Load(object sender, EventArgs e)
        {
            await Connection.SendToClientAsync(TriggerEvent, HabboEvents.ShowHelpBubble(HUIControl.CHAT_INPUT, "You can close the BetterChat module using the following command:\n\n:closebc"));
            await Task.Delay(1000);
            Hide();
        }
    }
}
