using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IPConfiger
{
    public partial class ConfigForm : Form
    {
        private TextBox _nameTextBox;
        private TextBox _descriptionTextBox;
        private RadioButton _dhcpRadioButton;
        private RadioButton _staticRadioButton;
        private TextBox _ipTextBox;
        private TextBox _subnetTextBox;
        private TextBox _gatewayTextBox;
        private TextBox _primaryDnsTextBox;
        private TextBox _secondaryDnsTextBox;
        private Button _okButton;
        private Button _cancelButton;
        private GroupBox _typeGroupBox;
        private GroupBox _staticGroupBox;
        private Label _adapterLabel;
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NetworkConfig NetworkConfig { get; private set; } = new();
        private readonly string _adapterName;
        private readonly bool _isEdit;

        public ConfigForm(NetworkConfig? config, string adapterName)
        {
            _adapterName = adapterName;
            _isEdit = config != null;
            NetworkConfig = config ?? new NetworkConfig { AdapterName = adapterName };
            
            InitializeComponent();
            LoadConfig();
        }

        private void InitializeComponent()
        {
            this.Text = _isEdit ? "编辑网络配置" : "新增网络配置";
            this.Size = new Size(450, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            // 适配器信息
            var adapterInfoLabel = new Label
            {
                Text = "适配器信息:",
                Location = new Point(15, 15),
                Size = new Size(80, 20),
                Font = new Font("Microsoft YaHei", 9, FontStyle.Bold)
            };

            _adapterLabel = new Label
            {
                Text = _adapterName,
                Location = new Point(100, 15),
                Size = new Size(320, 20),
                ForeColor = Color.Blue
            };

            // 配置名称
            var nameLabel = new Label
            {
                Text = "配置名称:",
                Location = new Point(15, 45),
                Size = new Size(80, 20)
            };

            _nameTextBox = new TextBox
            {
                Location = new Point(100, 43),
                Size = new Size(320, 23)
            };

            // 描述
            var descLabel = new Label
            {
                Text = "描述:",
                Location = new Point(15, 75),
                Size = new Size(80, 20)
            };

            _descriptionTextBox = new TextBox
            {
                Location = new Point(100, 73),
                Size = new Size(320, 23),
                Multiline = true,
                Height = 40
            };

            // IP类型选择
            _typeGroupBox = new GroupBox
            {
                Text = "IP地址类型",
                Location = new Point(15, 125),
                Size = new Size(405, 60)
            };

            _dhcpRadioButton = new RadioButton
            {
                Text = "自动获取IP地址(DHCP)",
                Location = new Point(15, 25),
                Size = new Size(180, 20),
                Checked = true
            };

            _staticRadioButton = new RadioButton
            {
                Text = "使用下面的IP地址",
                Location = new Point(200, 25),
                Size = new Size(150, 20)
            };

            _typeGroupBox.Controls.AddRange(new Control[] { _dhcpRadioButton, _staticRadioButton });

            // 静态IP配置
            _staticGroupBox = new GroupBox
            {
                Text = "静态IP配置",
                Location = new Point(15, 195),
                Size = new Size(405, 200),
                Enabled = false
            };

            var ipLabel = new Label
            {
                Text = "IP地址:",
                Location = new Point(15, 30),
                Size = new Size(80, 20)
            };

            _ipTextBox = new TextBox
            {
                Location = new Point(100, 28),
                Size = new Size(280, 23),
                PlaceholderText = "例如: 192.168.1.100"
            };

            var subnetLabel = new Label
            {
                Text = "子网掩码:",
                Location = new Point(15, 60),
                Size = new Size(80, 20)
            };

            _subnetTextBox = new TextBox
            {
                Location = new Point(100, 58),
                Size = new Size(280, 23),
                PlaceholderText = "例如: 255.255.255.0"
            };

            var gatewayLabel = new Label
            {
                Text = "默认网关:",
                Location = new Point(15, 90),
                Size = new Size(80, 20)
            };

            _gatewayTextBox = new TextBox
            {
                Location = new Point(100, 88),
                Size = new Size(280, 23),
                PlaceholderText = "例如: 192.168.1.1"
            };

            var primaryDnsLabel = new Label
            {
                Text = "首选DNS:",
                Location = new Point(15, 120),
                Size = new Size(80, 20)
            };

            _primaryDnsTextBox = new TextBox
            {
                Location = new Point(100, 118),
                Size = new Size(280, 23),
                PlaceholderText = "例如: 8.8.8.8"
            };

            var secondaryDnsLabel = new Label
            {
                Text = "备用DNS:",
                Location = new Point(15, 150),
                Size = new Size(80, 20)
            };

            _secondaryDnsTextBox = new TextBox
            {
                Location = new Point(100, 148),
                Size = new Size(280, 23),
                PlaceholderText = "例如: 8.8.4.4 (可选)"
            };

            _staticGroupBox.Controls.AddRange(new Control[] {
                ipLabel, _ipTextBox,
                subnetLabel, _subnetTextBox,
                gatewayLabel, _gatewayTextBox,
                primaryDnsLabel, _primaryDnsTextBox,
                secondaryDnsLabel, _secondaryDnsTextBox
            });



            // 按钮
            _okButton = new Button
            {
                Text = "确定",
                Location = new Point(260, 410),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };

            _cancelButton = new Button
            {
                Text = "取消",
                Location = new Point(350, 410),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            // 添加控件到窗体
            this.Controls.AddRange(new Control[] {
                adapterInfoLabel, _adapterLabel,
                nameLabel, _nameTextBox,
                descLabel, _descriptionTextBox,
                _typeGroupBox, _staticGroupBox,
                _okButton, _cancelButton
            });

            // 绑定事件
            _dhcpRadioButton.CheckedChanged += DhcpRadioButton_CheckedChanged;
            _staticRadioButton.CheckedChanged += StaticRadioButton_CheckedChanged;
            _okButton.Click += OkButton_Click;
            _nameTextBox.TextChanged += NameTextBox_TextChanged;

            // 初始状态
            UpdateButtonState();
        }

        private void LoadConfig()
        {
            if (_isEdit && NetworkConfig != null)
            {
                _nameTextBox.Text = NetworkConfig.Name;
                _descriptionTextBox.Text = NetworkConfig.Description;
                
                if (NetworkConfig.IsDHCP)
                {
                    _dhcpRadioButton.Checked = true;
                }
                else
                {
                    _staticRadioButton.Checked = true;
                    _ipTextBox.Text = NetworkConfig.IPAddress;
                    _subnetTextBox.Text = NetworkConfig.SubnetMask;
                    _gatewayTextBox.Text = NetworkConfig.Gateway;
                    _primaryDnsTextBox.Text = NetworkConfig.PrimaryDNS;
                    _secondaryDnsTextBox.Text = NetworkConfig.SecondaryDNS;
                }
            }
            else
            {
                // 新增配置时的默认值
                _nameTextBox.Text = $"配置 {DateTime.Now:yyyyMMdd_HHmmss}";
                _dhcpRadioButton.Checked = true;
            }
        }

        private void DhcpRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _staticGroupBox.Enabled = !_dhcpRadioButton.Checked;
        }

        private void StaticRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _staticGroupBox.Enabled = _staticRadioButton.Checked;
        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            _okButton.Enabled = !string.IsNullOrWhiteSpace(_nameTextBox.Text);
        }



        private void OkButton_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                this.DialogResult = DialogResult.None;
                return;
            }

            // 更新配置对象
            NetworkConfig.Name = _nameTextBox.Text.Trim();
            NetworkConfig.Description = _descriptionTextBox.Text.Trim();
            NetworkConfig.AdapterName = _adapterName;
            NetworkConfig.IsDHCP = _dhcpRadioButton.Checked;

            if (_staticRadioButton.Checked)
            {
                NetworkConfig.IPAddress = _ipTextBox.Text.Trim();
                NetworkConfig.SubnetMask = _subnetTextBox.Text.Trim();
                NetworkConfig.Gateway = _gatewayTextBox.Text.Trim();
                NetworkConfig.PrimaryDNS = _primaryDnsTextBox.Text.Trim();
                NetworkConfig.SecondaryDNS = _secondaryDnsTextBox.Text.Trim();
            }
            else
            {
                // DHCP模式清空静态配置
                NetworkConfig.IPAddress = string.Empty;
                NetworkConfig.SubnetMask = string.Empty;
                NetworkConfig.Gateway = string.Empty;
                NetworkConfig.PrimaryDNS = string.Empty;
                NetworkConfig.SecondaryDNS = string.Empty;
            }
        }

        private bool ValidateInput()
        {
            // 验证配置名称
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("请输入配置名称。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nameTextBox.Focus();
                return false;
            }

            // 如果选择静态IP，验证IP配置
            if (_staticRadioButton.Checked)
            {
                if (!ValidateIPAddress(_ipTextBox.Text.Trim(), "IP地址"))
                {
                    _ipTextBox.Focus();
                    return false;
                }

                if (!ValidateIPAddress(_subnetTextBox.Text.Trim(), "子网掩码"))
                {
                    _subnetTextBox.Focus();
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(_gatewayTextBox.Text) && 
                    !ValidateIPAddress(_gatewayTextBox.Text.Trim(), "默认网关"))
                {
                    _gatewayTextBox.Focus();
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(_primaryDnsTextBox.Text) && 
                    !ValidateIPAddress(_primaryDnsTextBox.Text.Trim(), "首选DNS"))
                {
                    _primaryDnsTextBox.Focus();
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(_secondaryDnsTextBox.Text) && 
                    !ValidateIPAddress(_secondaryDnsTextBox.Text.Trim(), "备用DNS"))
                {
                    _secondaryDnsTextBox.Focus();
                    return false;
                }

                // 验证IP地址和子网掩码是否在同一网段
                if (!string.IsNullOrWhiteSpace(_gatewayTextBox.Text))
                {
                    if (!IsInSameSubnet(_ipTextBox.Text.Trim(), _gatewayTextBox.Text.Trim(), _subnetTextBox.Text.Trim()))
                    {
                        MessageBox.Show("IP地址和默认网关不在同一网段。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _gatewayTextBox.Focus();
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidateIPAddress(string ip, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                MessageBox.Show($"请输入{fieldName}。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!IPAddress.TryParse(ip, out var ipAddress) || ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                MessageBox.Show($"{fieldName}格式不正确。", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool IsInSameSubnet(string ip1, string ip2, string subnetMask)
        {
            try
            {
                if (!IPAddress.TryParse(ip1, out var addr1) ||
                    !IPAddress.TryParse(ip2, out var addr2) ||
                    !IPAddress.TryParse(subnetMask, out var mask))
                {
                    return false;
                }

                var ip1Bytes = addr1.GetAddressBytes();
                var ip2Bytes = addr2.GetAddressBytes();
                var maskBytes = mask.GetAddressBytes();

                for (int i = 0; i < 4; i++)
                {
                    if ((ip1Bytes[i] & maskBytes[i]) != (ip2Bytes[i] & maskBytes[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}