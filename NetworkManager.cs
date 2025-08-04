using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using IPConfiger.Interfaces;

namespace IPConfiger
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public class NetworkManager : INetworkManager
    {
        public NetworkManager()
        {
        }
        /// <summary>
        /// 获取所有网络适配器
        /// </summary>
        public async Task<List<NetworkAdapter>> GetNetworkAdaptersAsync()
        {
            return await Task.Run(() => GetNetworkAdapters());
        }
        
        /// <summary>
        /// 获取所有网络适配器（同步方法）
        /// </summary>
        private List<NetworkAdapter> GetNetworkAdapters()
        {
            var adapters = new List<NetworkAdapter>();

            try
            {
                var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                                ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up ||
                                ni.OperationalStatus == OperationalStatus.Down)
                    .Where(ni => IsPhysicalAdapter(ni));

                foreach (var ni in networkInterfaces)
                {
                    try
                    {
                        var adapter = new NetworkAdapter
                        {
                            Name = ni.Name,
                            Description = ni.Description,
                            MACAddress = ni.GetPhysicalAddress().ToString(),
                            IsEnabled = ni.OperationalStatus == OperationalStatus.Up
                        };

                        var ipProps = ni.GetIPProperties();
                        var ipv4 = ipProps.UnicastAddresses
                            .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                        
                        if (ipv4 != null)
                        {
                            adapter.CurrentIP = ipv4.Address.ToString();
                        }

                        try
                        {
                            adapter.IsDHCPEnabled = ipProps.GetIPv4Properties()?.IsDhcpEnabled ?? false;
                        }
                        catch
                        {
                            // 如果无法获取DHCP状态，默认为false
                            adapter.IsDHCPEnabled = false;
                        }

                        adapters.Add(adapter);
                    }
                    catch (Exception ex)
                    {
                        // 跳过有问题的适配器，继续处理其他适配器
                        System.Diagnostics.Debug.WriteLine($"跳过适配器 {ni.Name}: {ex.Message}");
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取网络适配器失败: {ex.Message}\n\n可能的解决方案:\n1. 以管理员身份运行程序\n2. 检查网络服务是否正常启动\n3. 重启网络适配器或计算机");
            }

            return adapters;
        }

        /// <summary>
        /// 判断是否为物理网卡（排除虚拟网卡）
        /// </summary>
        private bool IsPhysicalAdapter(NetworkInterface networkInterface)
        {
            var description = networkInterface.Description.ToLower();
            var name = networkInterface.Name.ToLower();
            
            // 排除虚拟网卡的关键词
            var virtualKeywords = new[]
            {
                "virtual", "虚拟", "vmware", "virtualbox", "vbox", "hyper-v", "hyperv",
                "tap", "tun", "loopback", "回环", "teredo", "isatap", "6to4",
                "microsoft", "软件", "software", "tunnel", "隧道", "vpn",
                "wan miniport", "ras", "pptp", "l2tp", "sstp", "ikev2",
                "bluetooth", "蓝牙", "npcap", "winpcap", "packet", "capture"
            };
            
            // 检查描述和名称是否包含虚拟网卡关键词
            foreach (var keyword in virtualKeywords)
            {
                if (description.Contains(keyword) || name.Contains(keyword))
                {
                    return false;
                }
            }
            
            // 检查MAC地址，虚拟网卡通常没有真实的MAC地址或使用特定的前缀
            var macAddress = networkInterface.GetPhysicalAddress().ToString();
            if (string.IsNullOrEmpty(macAddress) || macAddress == "000000000000")
            {
                return false;
            }
            
            // 检查常见的虚拟网卡MAC地址前缀
            var virtualMacPrefixes = new[]
            {
                "00155D", // Microsoft Virtual PC/Hyper-V
                "000C29", // VMware
                "001C14", // VMware
                "005056", // VMware
                "080027", // VirtualBox
                "0A0027", // VirtualBox
                "00505A", // VMware ESX
                "001DD8", // OpenVPN TAP
                "00FF",   // Microsoft
                "02",     // Locally administered (often virtual)
            };
            
            foreach (var prefix in virtualMacPrefixes)
            {
                if (macAddress.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// 应用网络配置
        /// </summary>
        public async Task<bool> ApplyNetworkConfigAsync(NetworkConfig config)
        {
            try
            {
                // 应用网络IP配置
                if (config.IsDHCP)
                {
                    return await SetDHCPAsync(config.AdapterName);
                }
                else
                {
                    return await SetStaticIPAsync(config);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"应用网络配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 设置为DHCP
        /// </summary>
        public async Task<bool> SetDHCPAsync(string adapterName)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"interface ip set address \"{adapterName}\" dhcp",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Verb = "runas" // 以管理员权限运行
            };

            using var process = Process.Start(processInfo);
            if (process != null)
            {
                await process.WaitForExitAsync();
                
                // 设置DNS为自动获取
                var dnsProcessInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ip set dns \"{adapterName}\" dhcp",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas"
                };

                using var dnsProcess = Process.Start(dnsProcessInfo);
                if (dnsProcess != null)
                {
                    await dnsProcess.WaitForExitAsync();
                    return dnsProcess.ExitCode == 0;
                }
            }

            return false;
        }

        /// <summary>
        /// 设置静态IP
        /// </summary>
        public async Task<bool> SetStaticIPAsync(NetworkConfig config)
        {
            try
            {
                // 设置IP地址
                var ipProcessInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = $"interface ip set address \"{config.AdapterName}\" static {config.IPAddress} {config.SubnetMask} {config.Gateway}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas"
                };

                using var ipProcess = Process.Start(ipProcessInfo);
                if (ipProcess != null)
                {
                    await ipProcess.WaitForExitAsync();
                    if (ipProcess.ExitCode != 0)
                    {
                        return false;
                    }
                }

                // 设置主DNS
                if (!string.IsNullOrEmpty(config.PrimaryDNS))
                {
                    var primaryDnsProcessInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"interface ip set dns \"{config.AdapterName}\" static {config.PrimaryDNS}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Verb = "runas"
                    };

                    using var primaryDnsProcess = Process.Start(primaryDnsProcessInfo);
                    if (primaryDnsProcess != null)
                    {
                        await primaryDnsProcess.WaitForExitAsync();
                    }
                }

                // 设置备用DNS
                if (!string.IsNullOrEmpty(config.SecondaryDNS))
                {
                    var secondaryDnsProcessInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"interface ip add dns \"{config.AdapterName}\" {config.SecondaryDNS} index=2",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Verb = "runas"
                    };

                    using var secondaryDnsProcess = Process.Start(secondaryDnsProcessInfo);
                    if (secondaryDnsProcess != null)
                    {
                        await secondaryDnsProcess.WaitForExitAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"设置静态IP失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前网络配置
        /// </summary>
        public async Task<NetworkConfig> GetCurrentNetworkConfigAsync(string adapterName)
        {
            return await Task.Run(() => GetCurrentConfig(adapterName));
        }
        
        /// <summary>
        /// 获取当前网络配置（同步方法）
        /// </summary>
        private NetworkConfig GetCurrentConfig(string adapterName)
        {
            var config = new NetworkConfig
            {
                AdapterName = adapterName,
                Name = $"当前配置 - {adapterName}"
            };

            try
            {
                var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(ni => ni.Name == adapterName);

                if (networkInterface != null)
                {
                    var ipProps = networkInterface.GetIPProperties();
                    
                    try
                    {
                        config.IsDHCP = ipProps.GetIPv4Properties()?.IsDhcpEnabled ?? false;
                    }
                    catch
                    {
                        config.IsDHCP = false;
                    }

                    var ipv4 = ipProps.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    
                    if (ipv4 != null)
                    {
                        config.IPAddress = ipv4.Address.ToString();
                        try
                        {
                            config.SubnetMask = ipv4.IPv4Mask.ToString();
                        }
                        catch
                        {
                            config.SubnetMask = "255.255.255.0"; // 默认子网掩码
                        }
                    }

                    try
                    {
                        var gateway = ipProps.GatewayAddresses.FirstOrDefault();
                        if (gateway != null)
                        {
                            config.Gateway = gateway.Address.ToString();
                        }
                    }
                    catch
                    {
                        // 忽略网关获取错误
                    }

                    try
                    {
                        var dnsServers = ipProps.DnsAddresses
                            .Where(dns => dns.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .ToList();
                        
                        if (dnsServers.Count > 0)
                        {
                            config.PrimaryDNS = dnsServers[0].ToString();
                            if (dnsServers.Count > 1)
                            {
                                config.SecondaryDNS = dnsServers[1].ToString();
                            }
                        }
                    }
                    catch
                    {
                        // 忽略DNS获取错误
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"获取当前配置失败: {ex.Message}\n\n可能的解决方案:\n1. 以管理员身份运行程序\n2. 检查网络适配器是否正常工作\n3. 重启网络服务");
            }

            return config;
        }
    }
}