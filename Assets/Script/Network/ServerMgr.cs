using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Random = UnityEngine.Random;

namespace Game.Network {
    using Actor;

    public class ServerMgr : MonoBehaviour {
        private const int INTERVAL = 10;
        private class Unit {
            public List<Snapshot> list;
            public int count;

            public Unit() {
                this.list = new List<Snapshot>();
            }
        }

        [SerializeField]
        private int port;
        private Server server;
        private int frameCount;
        private Dictionary<string, Unit> unitMap;
        private List<List<Snapshot>> syncList;
        //private StreamWriter writer;

        protected void Awake() {
            this.server = new Server();
            this.unitMap = new Dictionary<string, Unit>();
            this.syncList = new List<List<Snapshot>>();
        }

        protected void Start() {
            this.server.RegisterHandler(MsgId.Connect, this.NewConnection);
            this.server.RegisterHandler(MsgId.Disconnect, this.DelConnection);
            this.server.RegisterHandler(MsgId.Input, this.Input);
            this.server.Listen(this.port);

            if (!this.server.Active) {
                Destroy(this);
                return;
            }

            //this.writer = new StreamWriter("server.log");
        }

        protected void FixedUpdate() {
            this.server.Update(Time.fixedDeltaTime);
            
            if (this.server.Active) {
                this.frameCount++;

                var list = new List<Snapshot>();
                
                foreach (var i in this.unitMap) {
                    int frame = -1;
                    var sl = i.Value.list;

                    while (sl.Count > 0 && (i.Value.count > INTERVAL || (frame == -1 || sl[0].frame == frame))) {
                        var s = sl[0];
                        list.Add(s);
                        sl.RemoveAt(0);

                        if (frame != s.frame) {
                            frame = s.frame;
                            i.Value.count--;
                        }
                    }
                }
                
                if (list.Count > 0) {
                    /*
                    foreach (var s in list) {
                        this.writer.Write(s.Print() + " ");
                    }

                    this.writer.Write("\n"); */
                    this.syncList.Add(list);
                }

                if (this.frameCount % INTERVAL == 0) {
                    this.server.SendToAll(MsgId.Sync, new Msg.Sync() {syncList = this.syncList});
                    this.syncList.Clear();
                }
                /*
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) {
                    this.server.Close();
                    //this.writer.Close();
                } */
            }
        }

        private void NewConnection(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var fd = ep.ToString();
            
            {
                var msg = new Msg.Connect() {
                    fd = fd,
                    playerDatas = ActorMgr.ToPlayerDatas()
                };
                
                this.server.Send(ep, MsgId.Connect, msg);
            }
            
            {
                var x = Mathf.Lerp(-2, 2, Random.value);
                var z = Mathf.Lerp(-2, 2, Random.value);

                var msg = new Msg.NewPlayer() {
                    playerData = new PlayerData() {
                        fd = fd,
                        position = new Vector3(x, 0, z)
                    }
                };

                this.server.SendToAll(MsgId.NewPlayer, msg);
            }

            print("New Client: " + fd);
        }

        private void DelConnection(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var fd = ep.ToString();
            var msg = new Msg.DelPlayer() {
                fd = fd
            };
            this.server.SendToAll(MsgId.DelPlayer, msg);
            
            print("Del Client: " + fd);
        }

        private void Input(byte msgId, NetworkReader reader, IPEndPoint ep) {
            var fd = ep.ToString();

            if (!this.unitMap.ContainsKey(fd)) {
                this.unitMap.Add(fd, new Unit());
            }
            
            var msg = new Msg.Input() {
                snapshotList = this.unitMap[fd].list
            };
            msg.Deserialize(reader);
            this.unitMap[fd].count += msg.snapshotFrameCount;
        }
    }
}