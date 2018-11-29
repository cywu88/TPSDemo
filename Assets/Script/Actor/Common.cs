using UnityEngine;
using UnityEngine.Networking;

namespace Game.Actor {
    public class PlayerData {
        public string fd;
        public Vector3 position;
        public Quaternion rotation;

        public void Serialize(NetworkWriter writer) {
            writer.Write(this.fd);
            writer.Write(this.position);
            writer.Write(this.rotation);
        }

        public void Deserialize(NetworkReader reader) {
            this.fd = reader.ReadString();
            this.position = reader.ReadVector3();
            this.rotation = reader.ReadQuaternion();
        }
    }

    public class Snapshot {
        public string fd;
        public int frame;

        public virtual void Serialize(NetworkWriter writer, bool isFull) {
            writer.Write(this.GetType().ToString());
            writer.Write(this.fd);
            writer.Write(this.frame);
        }

        public virtual void Deserialize(NetworkReader reader, bool isFull) {
            this.fd = reader.ReadString();
            this.frame = reader.ReadInt32();
        }

        public virtual bool Equals(Snapshot snapshot) {
            return this.fd == snapshot.fd && this.frame == snapshot.frame;
        }

        public virtual string Print() {
            return "(" + this.fd + ", " + this.frame + ", " + this.ToString() + ")";
        }
    }

    namespace Snapshots {
        public class Move : Snapshot {
            public Vector3 velocity;
            public Vector3 position;
            
            public override void Serialize(NetworkWriter writer, bool isFull) {
                base.Serialize(writer, isFull);

                writer.Write(this.velocity);

                if (isFull) {
                    writer.Write(this.position);
                }
            }

            public override void Deserialize(NetworkReader reader, bool isFull) {
                base.Deserialize(reader, isFull);
                
                this.velocity = reader.ReadVector3();
                
                if (isFull) {
                    this.position = reader.ReadVector3();
                }
            }
            
            public override bool Equals(Snapshot snapshot) {
                if (!base.Equals(snapshot)) {
                    return false;
                }

                var move = snapshot as Move;
                return this.velocity == move.velocity && this.position == move.position;
            }
        }

        public class Rotate : Snapshot {
            public Vector3 velocity;
            public Quaternion rotation;
            
            public override void Serialize(NetworkWriter writer, bool isFull) {
                base.Serialize(writer, isFull);

                writer.Write(this.velocity);

                if (isFull) {
                    writer.Write(this.rotation);
                }
            }

            public override void Deserialize(NetworkReader reader, bool isFull) {
                base.Deserialize(reader, isFull);
                
                this.velocity = reader.ReadVector3();
                
                if (isFull) {
                    this.rotation = reader.ReadQuaternion();
                }
            }
            
            public override bool Equals(Snapshot snapshot) {
                if (!base.Equals(snapshot)) {
                    return false;
                }

                var rotate = snapshot as Rotate;
                return this.velocity == rotate.velocity && this.rotation == rotate.rotation;
            }
        }

        public class Shoot : Snapshot {
            public Vector3 position;
            public Quaternion rotation;

            public override void Serialize(NetworkWriter writer, bool isFull) {
                base.Serialize(writer, isFull);

                if (isFull) {
                    writer.Write(this.position);
                    writer.Write(this.rotation);
                }
            }

            public override void Deserialize(NetworkReader reader, bool isFull) {
                base.Deserialize(reader, isFull);
                
                if (isFull) {
                    this.position = reader.ReadVector3();
                    this.rotation = reader.ReadQuaternion();
                }
            }
            
            public override bool Equals(Snapshot snapshot) {
                if (!base.Equals(snapshot)) {
                    return false;
                }

                var shoot = snapshot as Shoot;
                return this.position == shoot.position && this.rotation == shoot.rotation;
            }
        }
    }
}