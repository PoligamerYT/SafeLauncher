using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CmlLib.Core;
using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft.UI.WinForm;
using Microsoft.Web.WebView2.WinForms;
using static System.Collections.Specialized.BitVector32;

namespace SafeLauncher
{
    public partial class Launcher : Form
    {
        public MSession session = null;

        public Launcher()
        {
            InitializeComponent();
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            webView21.Source = new Uri("https://bs-community.github.io/skinview3d/");

            webView21.CoreWebView2InitializationCompleted += (sender_, args) =>
            {
                if (args.IsSuccess)
                {
                    var vw2 = (WebView2)sender_;
                    vw2.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    vw2.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                    vw2.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                }
            };

            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            var login = new MLogin();
            var response = login.TryAutoLogin();

            if (response.IsSuccess) // failed to automatically log in
            {
                session = response.Session;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Play();
        }

        public async Task Play()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 256;

            //var path = new MinecraftPath("game_directory_path");
            var path = new MinecraftPath(); // use default directory

            var launcher = new CMLauncher(path);

            // show launch progress to console
            launcher.FileChanged += (e) =>
            {
                Console.WriteLine("[{0}] {1} - {2}/{3}", e.FileKind.ToString(), e.FileName, e.ProgressedFileCount, e.TotalFileCount);
            };
            launcher.ProgressChanged += (s, e) =>
            {
                progressBar1.Value = e.ProgressPercentage;
                Console.WriteLine("{0}%", e.ProgressPercentage);
            };

            var versions = await launcher.GetAllVersionsAsync();
            foreach (var item in versions)
            {
                // show all version names
                // use this version name in CreateProcessAsync method.
                Console.WriteLine(item.Name);
            }

            var launchOption = new MLaunchOption
            {
                MaximumRamMb = 1024,
                Session = session, // replace this with login session value. ex) Session = MSession.GetOfflineSession("hello")

                //ScreenWidth = 1600,
                //ScreenHeight = 900,
                //ServerIp = "mc.hypixel.net"
            };

            //var process = await launcher.CreateProcessAsync("input version name here", launchOption);
            var process = await launcher.CreateProcessAsync("1.16.5", launchOption); // vanilla
                                                                                     // var process = await launcher.CreateProcessAsync("1.12.2-forge1.12.2-14.23.5.2838", launchOption); // forge
                                                                                     // var process = await launcher.CreateProcessAsync("1.12.2-LiteLoader1.12.2"); // liteloader
                                                                                     // var process = await launcher.CreateProcessAsync("fabric-loader-0.11.3-1.16.5") // fabric-loader

            process.Start();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(1);
        }

        public async Task Premiun()
        {
            button2.Enabled = false;
            button3.Enabled = false;

            MicrosoftLoginForm loginForm = new MicrosoftLoginForm();
            session = await loginForm.ShowLoginDialog();

            if (session.CheckIsValid())
            {
                ExecuteScript($"skinViewer.loadSkin('https://crafatar.com/skins/{session.UUID}');");

                LoadInfo();

                tabControl2.SelectTab(1);
            }
            else
            {
                button2.Enabled = true;
                button3.Enabled = true;
            }
        } 

        private void Update_Tick(object sender, EventArgs e)
        {
            if(session == null)
            {
                button1.Enabled = false;
            }
            else
            {
                if (!session.CheckIsValid())
                {
                    button1.Enabled = false;
                }
                else
                {
                    button1.Enabled = true;
                }
            }

            if (!string.IsNullOrEmpty(richTextBox1.Text) && richTextBox1.TextLength >= 3)
            {
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }

        public async Task ExecuteScript(string script)
        {
            await webView21.ExecuteScriptAsync(script);
        }

        private void webView21_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            ExecuteScript("var elements = document.getElementsByClassName('controls'); while(elements.length > 0){elements[0].parentNode.removeChild(elements[0]);}");
            ExecuteScript("var ele = document.getElementsByTagName('footer'); ele[0].parentNode.removeChild(ele[0]);");
            ExecuteScript("const list = document.getElementsByTagName('body'); if (list[0].hasChildNodes()) {list[0].removeChild(list[0].children[5]);}");
            ExecuteScript("skinViewer.width = 536; skinViewer.height = 536;");
            ExecuteScript("const list_ = document.getElementById('skin_container'); list_.style.width = '531px'; list_.style.height = '520px';");
            ExecuteScript("window.addEventListener('beforeunload', function (e) {e.preventDefault(); e.returnValue = ''; });");
            if (session != null)
            {
                if (session.CheckIsValid())
                {
                    ExecuteScript($"skinViewer.loadSkin('https://crafatar.com/skins/{session.UUID}');");
                }
                else
                {
                    ExecuteScript($"skinViewer.loadSkin('https://raw.githubusercontent.com/PoligamerYT/3DMV/main/2023_01_24_steve-21269468.png');");
                }
            }
            else
            {
                ExecuteScript($"skinViewer.loadSkin('https://raw.githubusercontent.com/PoligamerYT/3DMV/main/2023_01_24_steve-21269468.png');");
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Premiun();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button3.Enabled = false;

            session = MSession.GetOfflineSession(richTextBox1.Text);

            ExecuteScript($"skinViewer.loadSkin('https://crafatar.com/skins/{session.UUID}');");

            LoadInfo();

            tabControl2.SelectTab(1);
        }

        public void LoadInfo()
        {
            pictureBox3.Load($"https://mc-heads.net/avatar/{session.UUID}/100");
            label2.Text = session.Username;
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(3);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(4);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(session != null)
            {
                if(session.CheckIsValid()) 
                {
                    MicrosoftLoginForm loginForm = new MicrosoftLoginForm();
                    loginForm.ShowLogoutDialog();

                    session = null;

                    tabControl2.SelectTab(0);

                    button2.Enabled = true;
                    button3.Enabled = true;
                }
            }
        }
    }
}
