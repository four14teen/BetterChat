using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using Sulakore.Communication;
using Sulakore.Modules;
using Sulakore.Habbo;

using Tangine;

using HAlerts;

namespace BetterChat
{
    [Module("BetterChat", "Adds chat notifications to Habbo!")]
    [Author("Mika", HabboName = "M-ka", Hotel = HHotel.Nl, ResourceName = "Twitter", ResourceUrl = "https://www.twitter.com/Metoniem")]
    public partial class MainFrm : ExtensionForm
    {
        public const string EXIT_ICON_URL = "http://habboo-a.akamaihd.net/c_images/album1584/UK369.gif";

        private const string USER_TIP_MSG = "You can close the BetterChat module using the following command:\n\n:closebc";
        private const string CLOSED_MSG = "BetterChat has been closed!";

        public override bool IsRemoteModule => true;

        [MessageId("3ed1b2efef33a502709fdf49436f3e37")]
        public ushort HabboAlert { get; set; }

        [MessageId("4cd9a988356735f867f5c2080c8521ef")]
        public ushort TriggerEvent { get; set; }

        private List<(int UserId, string Message)> _pendingMessages;
        private Dictionary<int, BasicUser> _friends;

        public MainFrm()
        {
            InitializeComponent();

            Shown += OnFormShown;

            _friends = new Dictionary<int, BasicUser>();
            _pendingMessages = new List<(int userId, string message)>();
        }

        private async void OnFormShown(object sender, EventArgs e)
        {
            await Connection.SendToClientAsync(TriggerEvent, 
                HabboEvents.ShowHelpBubble(HUIControl.CHAT_INPUT, USER_TIP_MSG));

            Hide();
        }

        [InDataCapture("UserProfile")]
        public async void IncomingUserProfile(DataInterceptedEventArgs e)
        {
            e.Continue();

            BasicUser user = new BasicUser(e.Packet.ReadInteger(), e.Packet.ReadString(), e.Packet.ReadString());

            if (!_friends.ContainsKey(user.Id))
                _friends.Add(user.Id, user);
            
            foreach (var message in _pendingMessages.Where(m => m.UserId == user.Id))
            {
                await SendAlertAsync(user, message.Message);
                _pendingMessages.Remove(message);
            }
        }

        [InDataCapture("feb75b1e16f6f03dada3e349b81cfa25")]
        public async void IncomingPrivateMessage(DataInterceptedEventArgs e)
        {
            e.Continue();

            var id = e.Packet.ReadInteger();
            string message = e.Packet.ReadString();

            if (!_friends.TryGetValue(id, out var user))
            {
                _pendingMessages.Add((id, message));
                await Connection.SendToServerAsync(Out.RequestUserProfile, id, false);
            }
            else await SendAlertAsync(user, message);
        }

        [OutDataCapture("RoomUserTalk")]
        public async void OutgoingSpeech(DataInterceptedEventArgs e)
        {
            e.Continue();

            if (e.Packet.ReadString().ToLower() == ":closebc")
            {
                e.IsBlocked = true;
                HAlert alert = HAlertBuilder.CreateAlert(HAlertType.Bubble, CLOSED_MSG).ImageUrl(EXIT_ICON_URL);
                await Connection.SendToClientAsync(alert.ToPacket(HabboAlert));

                Close();
            }
        }

        public async Task SendAlertAsync(BasicUser user, string message)
        {
            HAlert alert = HAlertBuilder.CreateAlert(HAlertType.Bubble, $"{user.Username} said:\n\n{message}")
                .EventUrl(HabboEvents.ShowPlayerChat(user.Id))
                .ImageUrl(HabboLook.GetImagingUrl(user.Figure, Hotel));

            await Connection.SendToClientAsync(alert.ToPacket(HabboAlert));
        }
    }
}
