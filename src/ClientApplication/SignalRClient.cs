using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApplication
{

    /// <summary>
    /// Generic client class that interfaces .NET Standard/Blazor with SignalR Javascript client
    /// </summary>
    public class SignalRClient : IDisposable
    {

        #region static methods

        /// <summary>
        /// internal dictionary of SignalR clients by Key
        /// </summary>
        private static readonly Dictionary<string, SignalRClient> _clients = new Dictionary<string, SignalRClient>();


        /// <summary>
        /// Inbound message handler 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="method"></param>
        /// <param name="username"></param>
        /// <param name="message"></param>
        /// <remarks>
        /// This method is called from Javascript when amessage is received
        /// </remarks>
        [JSInvokable]
        public static void ReceiveMessage(string key, string method, string username, object message)
        {
            Console.WriteLine(message);
            // if (_clients.ContainsKey(key))
            // {
            //     var client = _clients[key];
            //     switch (method)
            //     {
            //         case "ReceiveMessage":
            //             client.HandleReceiveMessage(username, message);
            //             return;

            //         default:
            //             throw new NotImplementedException(method);
            //     }
            // }
            // else
            // {
            //     // unable to match the message to a client
            //     Console.WriteLine($"ReceiveMessage: unable to find {key}");
            // }
        }

        #endregion

        /// <summary>
        /// Ctor: create a new client for the given hub URL
        /// </summary>
        /// <param name="hubUrl"></param>
        public SignalRClient(string username, IJSRuntime JSRuntime)
        {
            _JSruntime = JSRuntime;
            // save username
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            _username = username;
            // create a unique key for this client
            _key = Guid.NewGuid().ToString();
            // add to the list of clients
            _clients.Add(_key, this);
        }

        /// <summary>
        /// The Hub URL for chat client
        /// </summary>
        const string HUBURL = "/chathub";

        /// <summary>
        /// Our unique key for this client instance
        /// </summary>
        /// <remarks>
        /// We cannot pass JS objects to Blazor/C# so we use a unique key
        /// to reference each instance. The JS client will store the object
        /// under our key so we can reference it
        /// </remarks>
        private readonly string _key;

        /// <summary>
        /// Name of the chatter
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// Flag to show if started
        /// </summary>
        private bool _started = false;

        /// <summary>
        /// JS runtime from DI
        /// </summary>
        private readonly IJSRuntime _JSruntime;


        /// <summary>
        /// Start the SignalR client on JS
        /// </summary>
        public async Task Start(string url)
        {
            if (!_started)
            {
                // var connection = _hubConnectionBuilder
                //     .WithUrl(connectionInfo.Url, options =>
                //     {
                //         options.LogLevel = SignalRLogLevel.Information;
                //         options.Transport = HttpTransportType.WebSockets;
                //         options.AccessTokenProvider = () => Task.FromResult(connectionInfo.AccessToken);
                //     })
                //     .Build();

                // connection.On<string>("newMessage", this.Handle);
                // await connection.StartAsync();
                // Console.WriteLine(connection.);


                // the callback method for inbound messages
                const string assembly = "ClientApplication";
                const string method = "ReceiveMessage";
                // invoke the JS interop start client method
                Console.WriteLine("ChatClient: calling Start()");
                var _ = await _JSruntime.InvokeAsync<object>("ChatClient.Start", _key, url, assembly, method);
                Console.WriteLine("ChatClient: Start returned");
                _started = true;

                // register user
//                await _JSruntime.InvokeAsync<object>("ChatClient.Register", _key, _username);
            }
        }

        /// <summary>
        /// Handle an inbound message from a hub
        /// </summary>
        /// <param name="method">event name</param>
        /// <param name="message">message content</param>
        private void HandleReceiveMessage(string username, string message)
        {
            // raise an event to subscribers
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(username, message));
        }

        /// <summary>
        /// Event raised when this client receives a message
        /// </summary>
        /// <remarks>
        /// Instance classes should subscribe to this event
        /// </remarks>
        public event MessageReceivedEventHandler MessageReceived;

        /// <summary>
        /// Send a message to the hub
        /// </summary>
        /// <param name="message">message to send</param>
        public async Task Send(string message)
        {
            // check we are connected
            if (!_started)
                throw new InvalidOperationException("Client not started");
            // send the message
            await _JSruntime.InvokeAsync<object>("ChatClient.Send", _key, _username, message);
        }

        /// <summary>
        /// Stop the client (if started)
        /// </summary>
        public async Task Stop()
        {
            if (_started)
            {
                // disconnect the client
                await _JSruntime.InvokeAsync<object>("ChatClient.Stop", _key);
                _started = false;
            }
        }

        /// <summary>
        /// Dispose of client
        /// </summary>
        public void Dispose()
        {
            Console.WriteLine("ChatClient: Disposing");
            // ensure we stop if connected
            if (_started)
            {
                Task.Run(async () =>
                {
                    await Stop();
                }).Wait();
            }

            // remove this key from the list of clients
            if (_clients.ContainsKey(_key))
                _clients.Remove(_key);
        }
    }

    /// <summary>
    /// Delegate for the message handler
    /// </summary>
    /// <param name="sender">the SignalRclient instance</param>
    /// <param name="e">Event args</param>
    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

    /// <summary>
    /// Message received argument class
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(string username, string message)
        {
            Username = username;
            Message = message;
        }

        /// <summary>
        /// Name of the message/event
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Message data items
        /// </summary>
        public string Message { get; set; }

    }

}
