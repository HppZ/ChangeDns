using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;

namespace ChangeDns
{
    class Program
    {
        const string adapter = "Realtek PCIe GBE Family Controller";

        static void Main(string[] args)
        {
            var isGoogleDns = GetCurrentDns();
            Console.WriteLine("current is " + (isGoogleDns ? "" : "not") + "Google DNS");

            Console.WriteLine(".");
            Console.WriteLine(".");
            Console.WriteLine(".");

            Console.WriteLine("Set begin");
            SetDNS(adapter,   "7.7.7.7" );
            Console.WriteLine("Set completed");

            Console.ReadLine();
        }

        /// <summary>
        /// 设置DNS
        /// </summary>
        private static void SetDNS(string NIC, string DNS)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    // if you are using the System.Net.NetworkInformation.NetworkInterface you'll need to change this line to if (objMO["Caption"].ToString().Contains(NIC)) and pass in the Description property instead of the name 
                    if (objMO["Caption"].ToString().Contains(NIC))
                    {
                        try
                        {
                            ManagementBaseObject newDNS = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                            newDNS["DNSServerSearchOrder"] = DNS?.Split('.');
                            ManagementBaseObject setDNS =
                                objMO.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("设置出错 " + exception.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取当前DNS
        /// </summary>
        private static bool GetCurrentDns()
        {
            bool result = false;
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {

                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
                if (dnsServers.Count > 0)
                {
                    Console.WriteLine(adapter.Description);
                    foreach (IPAddress dns in dnsServers)
                    {
                        if (dns.ToString().Contains("8.8"))
                        {
                            result = true;
                        }
                        Console.WriteLine("  DNS Servers ............................. : {0}", dns.ToString());
                    }
                    Console.WriteLine();
                }
            }

            return result;
        }


    }
}
