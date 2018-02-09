using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CBSync
{
    public class IPNetwork
    {
        public IPAddress NetworkIP { get; private set; }
        public IPAddress SubnetMask { get; private set; }

        public IPAddress BroadcastIP { get; private set; }
        public IPAddress FirstUsableIP { get; private set; }
        public IPAddress LastUsableIP { get; private set; }

        public int HostCount { get; private set; }

        public IPNetwork(IPAddress ip, IPAddress subnetMask)
        {
            Recalculate(ip, subnetMask);
        }

        public void Recalculate(IPAddress ip, IPAddress subnetMask)
        {
            SubnetMask = subnetMask;

            byte[] ipBuffer = ip.GetAddressBytes();
            byte[] mask = subnetMask.GetAddressBytes();

            byte[] network = new byte[4] {
                    (byte)(ipBuffer[0] & mask[0]),
                    (byte)(ipBuffer[1] & mask[1]),
                    (byte)(ipBuffer[2] & mask[2]),
                    (byte)(ipBuffer[3] & mask[3])
                };
            NetworkIP = new IPAddress(network);

            int numberOfSetBits = NumberOfSetBits(mask[0]) + NumberOfSetBits(mask[1]) + NumberOfSetBits(mask[2]) + NumberOfSetBits(mask[3]);
            HostCount = (int)Math.Pow(2, 32 - numberOfSetBits) - 2;

            byte[] first = new byte[4]
            {
                    network[0], network[1], network[2], (byte)(network[3] + 1)
            };
            FirstUsableIP = new IPAddress(first);

            byte[] last = new byte[4]
            {
                    first[0],
                    first[1],
                    first[2],
                    first[3]
            };
            int lastCalc = HostCount;
            int index = 3;
            while (lastCalc > 0 && index >= 0)
            {
                byte mod = (byte)(lastCalc % 256);
                last[index] += mod;
                lastCalc /= 256;
                index--;
            }
            BroadcastIP = new IPAddress(last);
            last[3] -= 1;
            LastUsableIP = new IPAddress(last);
        }

        private int NumberOfSetBits(int i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        public IEnumerable<IPAddress> IterateUsableIPs()
        {
            byte[] first = FirstUsableIP.GetAddressBytes();
            byte[] last = LastUsableIP.GetAddressBytes();
            byte[] current = new byte[4];
            first.CopyTo(current, 0);

            if (current.SequenceEqual(last))
            {
                yield return new IPAddress(current);
            }
            else
            {
                while (!current.SequenceEqual(last))
                {
                    yield return new IPAddress(current);

                    if (current[3] < 255)
                    {
                        current[3]++;
                    }
                    else
                    {
                        current[3] = 0;
                        if (current[2] < 255)
                        {
                            current[2]++;
                        }
                        else
                        {
                            current[2] = 0;
                            if (current[1] < 255)
                            {
                                current[1]++;
                            }
                            else
                            {
                                current[1] = 0;
                                if (current[0] < 255)
                                {
                                    current[0]++;
                                }
                                else
                                {
                                    yield break;
                                }
                            }
                        }
                    }
                }
                yield return new IPAddress(current);
            }
        }
    }
}
