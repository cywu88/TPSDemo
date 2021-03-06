using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Game.Actor {
    public static class ActorMgr {
        private static GameObject playerPrefab = Resources.Load("Prefab/Shooter") as GameObject;
        private static Dictionary<string, GameObject> playerMap = new Dictionary<string, GameObject>();

        public static GameObject NewPlayer(PlayerData playerData, bool isLocal) {
            var obj = GameObject.Instantiate(ActorMgr.playerPrefab, playerData.position, playerData.rotation);
            obj.name = playerData.fd;
            obj.GetComponent<Identity>().fd = playerData.fd;
            obj.GetComponent<Battle>().hp = playerData.hp;
            ActorMgr.playerMap.Add(playerData.fd, obj);
            
            if (isLocal) {
                var camera = GameObject.FindWithTag("MainCamera");
                var con = camera.GetComponent<ParentConstraint>();
                var source = new ConstraintSource(){
                    sourceTransform = obj.transform,
                    weight = 1
                };

                con.translationOffsets = new Vector3[]{camera.transform.position};
                con.rotationOffsets = new Vector3[]{camera.transform.rotation.eulerAngles};
                con.AddSource(source);
            }

            return obj;
        }

        public static bool DelPlayer(string fd) {
            if (!ActorMgr.playerMap.ContainsKey(fd)) {
                return false;
            }

            GameObject.Destroy(ActorMgr.playerMap[fd]);
            ActorMgr.playerMap.Remove(fd);

            return true;
        }

        public static PlayerData[] ToPlayerDatas() {
            var playerDatas = new PlayerData[ActorMgr.playerMap.Values.Count];
            int i = 0;

            foreach (var p in ActorMgr.playerMap.Values) {
                playerDatas[i] = new PlayerData();
                p.SendMessage("HandlePlayerData", playerDatas[i]);
                i++;
            }

            return playerDatas;
        }

        public static GameObject GetPlayer(string fd) {
            return ActorMgr.playerMap[fd];
        }

        public static void Input(Snapshot snapshot) {
            if (ActorMgr.playerMap.ContainsKey(snapshot.fd)) {
                var identity = ActorMgr.playerMap[snapshot.fd].GetComponent<Identity>();
                identity.RunEvent(snapshot);
            }
        }

        public static void Simulate(string fd) {
            if (!ActorMgr.playerMap.ContainsKey(fd)) {
                return;
            }

            var player = ActorMgr.playerMap[fd];
            player.SendMessage("Simulate");
        }

        public static void Debug() {
            foreach (var p in ActorMgr.playerMap) {
                var v = p.Value;
                GUILayout.Label(p.Key + ": " + v.GetComponent<Transform>().position.ToString() + ", " + v.GetComponent<Battle>().hp);
            }
        }
    }
}
