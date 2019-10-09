using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace VRServerSDK
{
    class ClientList
    {
        [Serializable]
        public class Client : IEquatable<Client>
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public bool VersionOk { get; set; }
            public int RefreshRate { get; set; }
            public bool Online { get; set; }

            public int deviceType { get; set; }//设备类型：0：Unknown；1：Go；2：Quest

            public int renderWidth { get; set; }
            public int renderHeight { get; set; }

            public float leftEyeFovLeft { get; set; }
            public float leftEyeFovRight { get; set; }
            public float leftEyeFovTop { get; set; }
            public float leftEyeFovBottom { get; set; }
            public float rightEyeFovLeft { get; set; }
            public float rightEyeFovRight { get; set; }
            public float rightEyeFovTop { get; set; }
            public float rightEyeFovBottom { get; set; }

            public Client() { }
            
            public Client(string clientName, string address, bool versionOk, int refreshRate = 0, bool online = false)
            {
                Name = clientName;
                Address = address;
                VersionOk = versionOk;
                RefreshRate = refreshRate;
                Online = online;
            }

            public bool Equals(Client other)
            {
                if (other == null)
                    return false;
                return Name == other.Name
                    && Address == other.Address
                    && RefreshRate == other.RefreshRate
                    && deviceType == other.deviceType
                    && renderWidth == other.renderWidth
                    && renderHeight == other.renderHeight
                    && leftEyeFovLeft == other.leftEyeFovLeft
                    && leftEyeFovRight == other.leftEyeFovRight
                    && leftEyeFovTop == other.leftEyeFovTop
                    && leftEyeFovBottom == other.leftEyeFovBottom
                    && rightEyeFovLeft == other.rightEyeFovLeft
                    && rightEyeFovRight == other.rightEyeFovRight
                    && rightEyeFovTop == other.rightEyeFovTop
                    && rightEyeFovBottom == other.rightEyeFovBottom;
            }

            public object Copy()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    if (this.GetType().IsSerializable)
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, this);
                        stream.Position = 0;
                        object result = formatter.Deserialize(stream);
                        return result;
                    }
                }
                return null;
            }

        }

        List<Client> autoConnectList = new List<Client>();
        List<Client> clients = new List<Client>();
        public bool EnableAutoConnect { get; set; } = true;

        public ClientList(string serialized)
        {
            try
            {
                autoConnectList.AddRange((Client[])DynamicJson.Parse(serialized));
            }
            catch (Exception e)
            {
                autoConnectList.Clear();
            }
        }

        public string Serialize()
        {
            return DynamicJson.Serialize(autoConnectList);
        }

        public List<Client> ParseRequests(string requests)
        {
            clients.Clear();

            foreach (var s in requests.Split('\n'))
            {
                var elem = s.Split(" ".ToCharArray(), 15);
                if (elem.Length != 15)
                {
                    continue;
                }
                /*
                 * elem[0]:address
                 * elem[1]:versionOk
                 * elem[2]:refreshRate
                 * elem[3]:clientName
                 * elem[4]:deviceType
                 * elem[5]:renderWidth
                 * elem[6]:renderHeight
                 * elem[7]:leftEyeFovLeft
                 * elem[8]:leftEyeFovRight
                 * elem[9]:leftEyeFovTop
                 * elem[10]:leftEyeFovBottom
                 * elem[11]:rightEyeFovLeft
                 * elem[12]:rightEyeFovRight
                 * elem[13]:rightEyeFovTop
                 * elem[14]:rightEyeFovBottom
                 */
                var client = new Client(elem[3], elem[0], elem[1] == "1", int.Parse(elem[2]), true);

                client.deviceType = int.Parse(elem[4]);//设备类型：0：Unknown；1：Go；2：Quest
                client.renderWidth = int.Parse(elem[5]);
                client.renderHeight = int.Parse(elem[6]);
                client.leftEyeFovLeft = float.Parse(elem[7]);
                client.leftEyeFovRight = float.Parse(elem[8]);
                client.leftEyeFovTop = float.Parse(elem[9]);
                client.leftEyeFovBottom = float.Parse(elem[10]);
                client.rightEyeFovLeft = float.Parse(elem[11]);
                client.rightEyeFovRight = float.Parse(elem[12]);
                client.rightEyeFovTop = float.Parse(elem[13]);
                client.rightEyeFovBottom = float.Parse(elem[14]);

                if (clients.Contains(client))
                {
                    // Update status.
                    clients.Remove(client);
                }
                clients.Add(client);
            }
            return clients.Concat(autoConnectList.Where(x => !clients.Contains(x))).ToList();
        }

        public void AddAutoConnect(Client client)
        {
            if (!autoConnectList.Contains(client))
            {
                autoConnectList.Add(client);
            }
        }

        public void RemoveAutoConnect(string ClientName, string Address)
        {
            var client = new Client(ClientName, Address, true);
            autoConnectList.Remove(client);
        }

        public void RemoveAutoConnect(Client client)
        {
            autoConnectList.Remove(client);
        }

        public List<Client> getList()
        {
            return autoConnectList;
        }

        public Client GetAutoConnectableClient()
        {
            var list = autoConnectList.Where(x => clients.Contains(x));
            if (list.Count() != 0)
            {
                if (!EnableAutoConnect)
                {
                    return null;
                }

                return list.First();
            }
            return null;
        }

        async public Task<bool> Connect(ControlSocket socket, Client client)
        {
            var ret = await socket.SendCommand("Connect " + client.Address);
            return ret == "OK";
        }

        public bool InAutoConnectList(string ClientName, string Address)
        {
            return autoConnectList.Contains(new Client(ClientName, Address, true));
        }
    }
}
