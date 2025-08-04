using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using IPConfiger.Interfaces;
using IPConfiger.Services;

namespace IPConfiger
{
    public partial class MainForm : Form
    {
        private readonly INetworkManager _networkManager;
        private readonly IConfigManager<NetworkConfig> _configManager;
        private readonly IProxyManager _proxyManager;
        private readonly IConfigManager<ProxyConfig> _proxyConfigManager;
        private ComboBox _adapterComboBox;
        private ListBox _configListBox;
        private ListBox _proxyConfigListBox;
        private Button _refreshButton;
        private Button _applyButton;
        private Button _addButton;
        private Button _editButton;
        private Button _deleteButton;
        private Button _getCurrentButton;
        private Button _getProxyButton;
        private Button _addProxyButton;
        private Button _editProxyButton;
        private Button _deleteProxyButton;
        private Button _applyProxyButton;
        private Label _statusLabel;
        private GroupBox _adapterGroupBox;
        private GroupBox _configGroupBox;
        private GroupBox _proxyConfigGroupBox;
        private GroupBox _actionGroupBox;
        private MenuStrip _menuStrip;
        private ToolStripMenuItem _fileMenuItem;
        private ToolStripMenuItem _exportMenuItem;
        private ToolStripMenuItem _importMenuItem;
        private ToolStripMenuItem _helpMenuItem;
        private ToolStripMenuItem _aboutMenuItem;

        public MainForm()
        {
            // 从依赖注入容器获取服务
            _networkManager = ServiceLocator.GetService<INetworkManager>();
            _configManager = ServiceLocator.GetService<IConfigManager<NetworkConfig>>();
            _proxyManager = ServiceLocator.GetService<IProxyManager>();
            _proxyConfigManager = ServiceLocator.GetService<IConfigManager<ProxyConfig>>();
            InitializeComponent();
            LoadAdapters();
            LoadConfigs();
            LoadProxyConfigs();
        }

        private void InitializeComponent()
        {
            this.Text = "IP配置管理器";
            this.Size = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 550);
            this.Icon = SystemIcons.Application;

            // 创建菜单栏
            _menuStrip = new MenuStrip();
            _fileMenuItem = new ToolStripMenuItem("文件(&F)");
            _exportMenuItem = new ToolStripMenuItem("导出配置(&E)");
            _importMenuItem = new ToolStripMenuItem("导入配置(&I)");
            _helpMenuItem = new ToolStripMenuItem("帮助(&H)");
            _aboutMenuItem = new ToolStripMenuItem("关于(&A)");

            _fileMenuItem.DropDownItems.AddRange(new ToolStripItem[] { _exportMenuItem, _importMenuItem });
            _helpMenuItem.DropDownItems.Add(_aboutMenuItem);
            _menuStrip.Items.AddRange(new ToolStripItem[] { _fileMenuItem, _helpMenuItem });

            // 网络适配器组
            _adapterGroupBox = new GroupBox
            {
                Text = "网络适配器",
                Location = new Point(10, 30),
                Size = new Size(760, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _adapterComboBox = new ComboBox
            {
                Location = new Point(10, 25),
                Size = new Size(500, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _refreshButton = new Button
            {
                Text = "刷新",
                Location = new Point(520, 23),
                Size = new Size(80, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _getCurrentButton = new Button
            {
                Text = "获取当前配置",
                Location = new Point(610, 23),
                Size = new Size(120, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _getProxyButton = new Button
            {
                Text = "获取代理配置",
                Location = new Point(610, 58),
                Size = new Size(120, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _adapterGroupBox.Controls.AddRange(new Control[] { _adapterComboBox, _refreshButton, _getCurrentButton, _getProxyButton });

            // 网络配置管理组
            _configGroupBox = new GroupBox
            {
                Text = "网络IP配置",
                Location = new Point(10, 140),
                Size = new Size(375, 280),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };

            _configListBox = new ListBox
            {
                Location = new Point(10, 25),
                Size = new Size(270, 240),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };

            _addButton = new Button
            {
                Text = "新增",
                Location = new Point(290, 25),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _editButton = new Button
            {
                Text = "编辑",
                Location = new Point(290, 65),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _deleteButton = new Button
            {
                Text = "删除",
                Location = new Point(290, 105),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _configGroupBox.Controls.AddRange(new Control[] { _configListBox, _addButton, _editButton, _deleteButton });

            // 代理配置管理组
            _proxyConfigGroupBox = new GroupBox
            {
                Text = "代理服务器配置",
                Location = new Point(395, 140),
                Size = new Size(375, 280),
                Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
            };

            _proxyConfigListBox = new ListBox
            {
                Location = new Point(10, 25),
                Size = new Size(270, 240),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };

            _addProxyButton = new Button
            {
                Text = "新增",
                Location = new Point(290, 25),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _editProxyButton = new Button
            {
                Text = "编辑",
                Location = new Point(290, 65),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _deleteProxyButton = new Button
            {
                Text = "删除",
                Location = new Point(290, 105),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            _applyProxyButton = new Button
            {
                Text = "应用代理",
                Location = new Point(290, 145),
                Size = new Size(75, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.LightBlue
            };

            _proxyConfigGroupBox.Controls.AddRange(new Control[] { _proxyConfigListBox, _addProxyButton, _editProxyButton, _deleteProxyButton, _applyProxyButton });

            // 操作组
            _actionGroupBox = new GroupBox
            {
                Text = "操作",
                Location = new Point(10, 430),
                Size = new Size(760, 80),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _applyButton = new Button
            {
                Text = "应用选中配置",
                Location = new Point(10, 25),
                Size = new Size(150, 40),
                BackColor = Color.LightGreen,
                Font = new Font("Microsoft YaHei", 10, FontStyle.Bold)
            };

            _statusLabel = new Label
            {
                Text = "就绪",
                Location = new Point(180, 35),
                Size = new Size(560, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _actionGroupBox.Controls.AddRange(new Control[] { _applyButton, _statusLabel });

            // 添加控件到窗体
            this.Controls.AddRange(new Control[] { _menuStrip, _adapterGroupBox, _configGroupBox, _proxyConfigGroupBox, _actionGroupBox });
            this.MainMenuStrip = _menuStrip;

            // 绑定事件
            _refreshButton.Click += RefreshButton_Click;
            _getCurrentButton.Click += GetCurrentButton_Click;
            _getProxyButton.Click += GetProxyButton_Click;
            _addButton.Click += AddButton_Click;
            _editButton.Click += EditButton_Click;
            _deleteButton.Click += DeleteButton_Click;
            _applyButton.Click += ApplyButton_Click;
            _addProxyButton.Click += AddProxyButton_Click;
            _editProxyButton.Click += EditProxyButton_Click;
            _deleteProxyButton.Click += DeleteProxyButton_Click;
            _applyProxyButton.Click += ApplyProxyButton_Click;
            _adapterComboBox.SelectedIndexChanged += AdapterComboBox_SelectedIndexChanged;
            _configListBox.SelectedIndexChanged += ConfigListBox_SelectedIndexChanged;
            _configListBox.DoubleClick += ConfigListBox_DoubleClick;
            _proxyConfigListBox.SelectedIndexChanged += ProxyConfigListBox_SelectedIndexChanged;
            _proxyConfigListBox.DoubleClick += ProxyConfigListBox_DoubleClick;
            _exportMenuItem.Click += ExportMenuItem_Click;
            _importMenuItem.Click += ImportMenuItem_Click;
            _aboutMenuItem.Click += AboutMenuItem_Click;

            // 初始状态
            UpdateButtonStates();
        }

        private async void LoadAdapters()
        {
            await AsyncHelper.ExecuteWithErrorHandling(async () =>
            {
                _adapterComboBox.Items.Clear();
                var adapters = await _networkManager.GetNetworkAdaptersAsync();
                
                foreach (var adapter in adapters)
                {
                    _adapterComboBox.Items.Add(adapter);
                }

                if (_adapterComboBox.Items.Count > 0)
                {
                    _adapterComboBox.SelectedIndex = 0;
                }

                UpdateStatus($"找到 {adapters.Count} 个网络适配器");
            }, "加载网络适配器");
        }

        private async void LoadConfigs()
        {
            await AsyncHelper.ExecuteWithErrorHandling(async () =>
            {
                _configListBox.Items.Clear();
                var configs = await _configManager.GetConfigsAsync();
                
                foreach (var config in configs)
                {
                    _configListBox.Items.Add(config);
                }

                UpdateStatus($"加载了 {configs.Count} 个配置");
            }, "加载配置");
        }

        private async void LoadProxyConfigs()
        {
            await AsyncHelper.ExecuteWithErrorHandling(async () =>
            {
                _proxyConfigListBox.Items.Clear();
                var proxyConfigs = await _proxyConfigManager.GetConfigsAsync();
                
                foreach (var config in proxyConfigs)
                {
                    _proxyConfigListBox.Items.Add(config);
                }

                UpdateStatus($"加载了 {proxyConfigs.Count} 个代理配置");
            }, "加载代理配置");
        }

        private async void LoadConfigsForCurrentAdapter()
        {
            if (_adapterComboBox.SelectedItem is NetworkAdapter adapter)
            {
                await AsyncHelper.ExecuteWithErrorHandling(async () =>
                {
                    _configListBox.Items.Clear();
                    var allConfigs = await _configManager.GetConfigsAsync();
                    var configs = allConfigs.Where(c => c.AdapterName == adapter.Name).ToList();
                    
                    foreach (var config in configs)
                    {
                        _configListBox.Items.Add(config);
                    }

                    UpdateStatus($"为适配器 '{adapter.Name}' 找到 {configs.Count} 个配置");
                }, "加载适配器配置");
            }
        }

        private void UpdateButtonStates()
        {
            var hasAdapter = _adapterComboBox.SelectedItem != null;
            var hasConfig = _configListBox.SelectedItem != null;

            _getCurrentButton.Enabled = hasAdapter;
            _getProxyButton.Enabled = true; // 代理配置不依赖于特定适配器
            _addButton.Enabled = hasAdapter;
            _editButton.Enabled = hasConfig;
            _deleteButton.Enabled = hasConfig;
            _applyButton.Enabled = hasConfig;
        }

        private void UpdateStatus(string message)
        {
            _statusLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
            Application.DoEvents();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadAdapters();
            LoadConfigs();
        }

        private async void GetCurrentButton_Click(object sender, EventArgs e)
        {
            if (_adapterComboBox.SelectedItem is NetworkAdapter adapter)
            {
                await AsyncHelper.ExecuteWithErrorHandling(async () =>
                {
                    UpdateStatus("正在获取当前配置...");
                    var currentConfig = await _networkManager.GetCurrentNetworkConfigAsync(adapter.Name);
                    
                    var configForm = new ConfigForm(currentConfig, adapter.Name);
                    if (configForm.ShowDialog() == DialogResult.OK)
                    {
                        await _configManager.AddConfigAsync(configForm.NetworkConfig);
                        LoadConfigs();
                        UpdateStatus("当前配置已保存");
                    }
                }, "获取当前配置");
            }
        }

        private async void GetProxyButton_Click(object sender, EventArgs e)
        {
            await AsyncHelper.ExecuteWithErrorHandling(async () =>
            {
                var currentConfig = await _proxyManager.GetCurrentProxyConfigAsync();
                
                var configName = $"当前代理配置_{DateTime.Now:yyyyMMdd_HHmmss}";
                var proxyConfig = new ProxyConfig
                {
                    Name = configName,
                    UseProxy = currentConfig.UseProxy,
                    ProxyServer = currentConfig.ProxyServer,
                    ProxyPort = currentConfig.ProxyPort,
                    ProxyBypassList = currentConfig.ProxyBypassList
                };
                
                await _proxyConfigManager.AddConfigAsync(proxyConfig);
                LoadProxyConfigs();
                MessageBox.Show($"已保存当前代理配置: {configName}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }, "获取当前代理配置");
        }

        private async void AddButton_Click(object sender, EventArgs e)
        {
            await AsyncHelper.ExecuteWithErrorHandling(async () =>
            {
                using (var configForm = new ConfigForm(null, ""))
                {
                    if (configForm.ShowDialog() == DialogResult.OK)
                    {
                        await _configManager.AddConfigAsync(configForm.NetworkConfig);
                        LoadConfigs();
                        MessageBox.Show("配置已添加", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }, "添加配置");
        }

        private async void EditButton_Click(object sender, EventArgs e)
        {
            await AsyncHelper.ExecuteWithErrorHandling(async () =>
            {
                if (_configListBox.SelectedItem == null)
                {
                    MessageBox.Show("请先选择要编辑的配置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var configName = _configListBox.SelectedItem.ToString();
                var config = await _configManager.GetConfigAsync(configName);
                
                using (var configForm = new ConfigForm(config, config?.AdapterName ?? ""))
                {
                    if (configForm.ShowDialog() == DialogResult.OK)
                    {
                        await _configManager.UpdateConfigAsync(configForm.NetworkConfig);
                        LoadConfigs();
                        MessageBox.Show("配置已更新", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }, "编辑配置");
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            await AsyncHelper.ExecuteWithErrorHandling(async () =>
            {
                if (_configListBox.SelectedItem == null)
                {
                    MessageBox.Show("请先选择要删除的配置", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var configName = _configListBox.SelectedItem.ToString();
                var result = MessageBox.Show($"确定要删除配置 '{configName}' 吗？", "确认删除", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    await _configManager.DeleteConfigAsync(configName);
                    LoadConfigs();
                    MessageBox.Show("配置已删除", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }, "删除配置");
        }

        private async void ApplyButton_Click(object sender, EventArgs e)
        {
            if (_configListBox.SelectedItem is NetworkConfig config)
            {
                var result = MessageBox.Show($"确定要应用配置 '{config.Name}' 吗？\n\n" +
                    $"适配器: {config.AdapterName}\n" +
                    $"IP地址: {(config.IsDHCP ? "自动获取(DHCP)" : config.IPAddress)}\n" +
                    $"子网掩码: {config.SubnetMask}\n" +
                    $"默认网关: {config.Gateway}\n" +
                    $"首选DNS: {config.PrimaryDNS}\n" +
                    $"备用DNS: {config.SecondaryDNS}", 
                    "确认应用配置", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _applyButton.Enabled = false;
                        UpdateStatus("正在应用配置，请稍候...");
                        
                        var success = await _networkManager.ApplyNetworkConfigAsync(config);
                        
                        if (success)
                        {
                            UpdateStatus("配置应用成功");
                            MessageBox.Show("配置应用成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            UpdateStatus("配置应用失败");
                            MessageBox.Show("配置应用失败，请检查权限或配置参数。", "失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"应用配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatus("应用配置失败");
                    }
                    finally
                    {
                        _applyButton.Enabled = true;
                    }
                }
            }
        }

        private void AdapterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadConfigsForCurrentAdapter();
            UpdateButtonStates();
        }

        private void ConfigListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void ConfigListBox_DoubleClick(object sender, EventArgs e)
        {
            if (_configListBox.SelectedItem != null)
            {
                ApplyButton_Click(sender, e);
            }
        }

        private async void ExportMenuItem_Click(object sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "JSON文件|*.json|所有文件|*.*",
                DefaultExt = "json",
                FileName = $"IPConfigs_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                await AsyncHelper.ExecuteWithErrorHandling(async () =>
                {
                    var configData = await _configManager.ExportConfigsAsync();
                    await System.IO.File.WriteAllTextAsync(saveDialog.FileName, configData);
                    MessageBox.Show("配置导出成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus("配置已导出");
                }, "导出配置");
            }
        }

        private async void ImportMenuItem_Click(object sender, EventArgs e)
        {
            using var openDialog = new OpenFileDialog
            {
                Filter = "JSON文件|*.json|所有文件|*.*",
                DefaultExt = "json"
            };

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                await AsyncHelper.ExecuteWithErrorHandling(async () =>
                {
                    var configData = await System.IO.File.ReadAllTextAsync(openDialog.FileName);
                    await _configManager.ImportConfigsAsync(configData);
                    LoadConfigs();
                    MessageBox.Show("配置导入成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus("配置已导入");
                }, "导入配置");
            }
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("IP配置管理器 v1.0\n\n" +
                "功能特性：\n" +
                "• 管理多个网络配置\n" +
                "• 快速切换IP设置\n" +
                "• 支持DHCP和静态IP\n" +
                "• 配置导入导出\n\n" +
                "使用说明：\n" +
                "1. 选择网络适配器\n" +
                "2. 添加或编辑配置\n" +
                "3. 双击或点击应用按钮切换配置\n\n" +
                "注意：应用配置需要管理员权限", 
                "关于", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void AddProxyButton_Click(object sender, EventArgs e)
        {
            var proxyConfigForm = new ProxyConfigForm(null);
            if (proxyConfigForm.ShowDialog() == DialogResult.OK)
            {
                await AsyncHelper.ExecuteWithErrorHandling(async () =>
                {
                    await _proxyConfigManager.AddConfigAsync(proxyConfigForm.ProxyConfig);
                    LoadProxyConfigs();
                    UpdateStatus("代理配置已添加");
                }, "添加代理配置");
            }
        }

        private async void EditProxyButton_Click(object sender, EventArgs e)
        {
            if (_proxyConfigListBox.SelectedItem is ProxyConfig config)
            {
                var proxyConfigForm = new ProxyConfigForm(config);
                if (proxyConfigForm.ShowDialog() == DialogResult.OK)
                {
                    await AsyncHelper.ExecuteWithErrorHandling(async () =>
                    {
                        await _proxyConfigManager.UpdateConfigAsync(proxyConfigForm.ProxyConfig);
                        LoadProxyConfigs();
                        UpdateStatus("代理配置已更新");
                    }, "更新代理配置");
                }
            }
        }

        private async void DeleteProxyButton_Click(object sender, EventArgs e)
        {
            if (_proxyConfigListBox.SelectedItem is ProxyConfig config)
            {
                var result = MessageBox.Show($"确定要删除代理配置 '{config.Name}' 吗？", "确认删除", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    await AsyncHelper.ExecuteWithErrorHandling(async () =>
                    {
                        await _proxyConfigManager.DeleteConfigAsync(config.Name);
                        LoadProxyConfigs();
                        UpdateStatus("代理配置已删除");
                    }, "删除代理配置");
                }
            }
        }

        private async void ApplyProxyButton_Click(object sender, EventArgs e)
        {
            if (_proxyConfigListBox.SelectedItem is ProxyConfig config)
            {
                var result = MessageBox.Show($"确定要应用代理配置 '{config.Name}' 吗？\n\n" +
                     $"代理服务器: {config.ProxyServer}:{config.ProxyPort}\n" +
                     $"代理类型: {config.ProxyType}\n" +
                     $"需要身份验证: {(config.ProxyRequiresAuth ? "是" : "否")}", 
                    "确认应用代理配置", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        _applyProxyButton.Enabled = false;
                        UpdateStatus("正在应用代理配置，请稍候...");
                        
                        var success = await _proxyManager.ApplyProxyConfigAsync(config);
                        
                        if (success)
                        {
                            UpdateStatus("代理配置应用成功");
                            MessageBox.Show("代理配置应用成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            UpdateStatus("代理配置应用失败");
                            MessageBox.Show("代理配置应用失败，请检查配置参数。", "失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"应用代理配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatus("应用代理配置失败");
                    }
                    finally
                    {
                        _applyProxyButton.Enabled = true;
                    }
                }
            }
        }

        private void ProxyConfigListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var hasProxyConfig = _proxyConfigListBox.SelectedItem != null;
            _editProxyButton.Enabled = hasProxyConfig;
            _deleteProxyButton.Enabled = hasProxyConfig;
            _applyProxyButton.Enabled = hasProxyConfig;
        }

        private void ProxyConfigListBox_DoubleClick(object sender, EventArgs e)
        {
            if (_proxyConfigListBox.SelectedItem != null)
            {
                ApplyProxyButton_Click(sender, e);
            }
        }
    }
}