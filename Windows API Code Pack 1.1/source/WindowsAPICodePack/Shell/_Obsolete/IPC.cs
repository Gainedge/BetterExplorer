using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IPC {
	[Obsolete("Do Not Use", true)]
	public static class IPC {
		public class Packet {
			public byte[] Buffer { get; private set; }
			public Packet(byte[] buffer) {
				Buffer = buffer;
			}
		}

		public class PacketEventArgs : EventArgs {
			public Packet Packet { get; set; }
		}

		private class UdpState {
			public UdpClient Client { get; set; }
			public IPEndPoint EndPoint { get; set; }
		}

		// Create the client UDP socket.
		static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 5006);
		static readonly UdpClient client = new UdpClient(endPoint);

		public class Receiver {
			public event EventHandler<PacketEventArgs> PacketReceived;
			private readonly Thread _thread;
			private readonly ManualResetEvent _shutdownThread = new ManualResetEvent(false);
			public void Start() { _thread.Start(); }
			public void Stop() { _shutdownThread.Set(); }
			public Receiver() {
				_thread = new Thread(
						delegate() {
							// Receive the packets asynchronously.
							try {
								client.BeginReceive(
										new AsyncCallback(OnPacketReceived),
										new UdpState() { Client = client, EndPoint = endPoint });
								// Wait for the thread to end.
								_shutdownThread.WaitOne();
							}
							catch (Exception) {
								//TODO: show already running instance and if found, close this application
							}
						}
				);
			}

			private void OnPacketReceived(IAsyncResult ar) {
				UdpState state = (UdpState)ar.AsyncState;
				IPEndPoint endPoint = state.EndPoint;
				byte[] bytes = state.Client.EndReceive(ar, ref endPoint);
				// Create the packet. 
				Packet packet = new Packet(bytes);
				// Notify any listeners.
				EventHandler<PacketEventArgs> handler = PacketReceived;
				if (handler != null) {
					handler(this, new PacketEventArgs() { Packet = packet });
				}
				// Read next packet.
				if (!_shutdownThread.WaitOne(0)) {
					state.Client.BeginReceive(
							new AsyncCallback(OnPacketReceived),
							state);
				}
			}
		}

		public class Processor {
			private Thread _thread;
			private object _sync = new object();
			private ManualResetEvent _packetReceived = new ManualResetEvent(false);
			private ManualResetEvent _shutdownThread = new ManualResetEvent(false);
			private Queue<Packet> _packetQueue = new Queue<Packet>(); // shared data
			public void Start() { _thread.Start(); }
			public void Stop() { _shutdownThread.Set(); }
			public Processor() {
				_thread = new Thread(
						delegate() {
							WaitHandle[] handles = new WaitHandle[] {
                    _shutdownThread,
                    _packetReceived
                };

							while (!_shutdownThread.WaitOne(0)) {
								switch (WaitHandle.WaitAny(handles)) {
									case 0: // Shutdown Thread Event
										break;
									case 1: // Packet Received Event
										_packetReceived.Reset();
										ProcessPackets();
										break;
									default:
										Stop();
										break;
								}
							}
						}
				);
			}

			private void ProcessPackets() {
				Queue<Packet> localPacketQueue = null;
				Queue<Packet> newPacketQueue = new Queue<Packet>();
				lock (_sync) {
					// Swap out the populated queue with the empty queue.
					localPacketQueue = _packetQueue;
					_packetQueue = newPacketQueue;
				}

				foreach (Packet packet in localPacketQueue) {
					Console.WriteLine(
							"Packet received with {0} bytes and value {1}",
							packet.Buffer.Length, BitConverter.ToInt32(packet.Buffer, 0));
				}
			}

			public void OnPacketReceived(object sender, PacketEventArgs e) {
				// NOTE:  This function executes on the Receiver thread.
				lock (_sync) {
					// Enqueue the packet.
					_packetQueue.Enqueue(e.Packet);
				}

				// Notify the Processor thread that a packet is available.
				_packetReceived.Set();
			}
		}
	}
}
