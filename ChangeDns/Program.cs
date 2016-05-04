using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace ChangeDns
{
    class Program
    {
        const string ADAPTER_NAME = "Realtek PCIe GBE Family Controller";

        static void Main(string[] args)
        {
            var isGoogleDns = GetCurrentDns();
            Console.WriteLine("Is " + (isGoogleDns ? "" : " NOT ") + "Google DNS");

            Console.WriteLine(".");
            Console.WriteLine(".");
            Console.WriteLine(".");

            Console.WriteLine("Set begin");
            SetDNS(ADAPTER_NAME, isGoogleDns ? null : "8.8.8.8");
            var succeed = GetCurrentDns() != isGoogleDns;

            Console.ForegroundColor = succeed? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("Set completed " + (succeed ? "成功" : "失败"));
            Console.ResetColor();

            if (!succeed)
            {
                Console.ReadLine();
            }
            else
            {
                Thread.Sleep(1500);
            }
        }

        /// <summary>
        /// 设置DNS
        /// </summary>
        private static void SetDNS(string NIC, string DNS)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("New dns : " + (DNS ?? "Auto"));
            Console.ResetColor();

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
                            newDNS["DNSServerSearchOrder"] = DNS?.Split(',');
                            ManagementBaseObject setDNS = objMO.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
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
                if (adapter.Description.Contains(ADAPTER_NAME) && dnsServers.Count > 0)
                {
                    Console.WriteLine(adapter.Description);
                    foreach (IPAddress dns in dnsServers)
                    {
                        if (dns.ToString().Contains("8.8"))
                        {
                            result = true;
                            break;
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
