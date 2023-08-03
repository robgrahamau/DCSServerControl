using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
namespace servermonitor
{
    public partial class Form1 : Form
    {
        // these are our base variables, really we should make a class for the DCS process holding everything
        // But i'm lazy and this is a quick program its not like i'm doing it for $$.
        // We are also redundent in a few places here.. i could just use the resource strings.. but old habits.
        string dcspath = ""; 
        string srspath1 = ""; // these are not used yet... srs handling doesn't exist yet
        
        Process dcs1 = new Process(); // this is the process for the first server
        bool dcs1start = false; // we started
        int dcs1pid; // process id handler
        int dcs1hangtime = 0; // how long has a hang been going?
        int dcs1uptime = 0; // how long have we been up
        int dcs1totalhangs = 0;
        int dcs2totalhangs = 0;
        int dcs3totalhangs = 0;
        bool hanging1 = false;
        bool hanging2 = false;
        bool hanging3 = false;
        int dcs1hangsrestart = 0;
        int dcs2hangsrestart = 0;
        int dcs3hangsrestart = 0;
        System.Windows.Forms.Timer dcs1timer = new System.Windows.Forms.Timer(); // our timer for 1
       
        // same as above.. only for server 2
        Process dcs2 = new Process();
        bool dcs2start = false;
        int dcs2pid;
        int dcs2hangtime = 0;
        int dcs2uptime = 0;
        bool firstrun1 = true;
        bool firstrun2 = true;
        bool firstrun3 = true;

        System.Windows.Forms.Timer dcs2timer = new System.Windows.Forms.Timer();
        // same as above only for server 3
        Process dcs3 = new Process();
        bool dcs3start = false;
        int dcs3pid;
        int dcs3hangtime = 0;
        int defaulthangtime = 240;
        int dcs3uptime = 0;
        System.Windows.Forms.Timer dcs3timer = new System.Windows.Forms.Timer();
        int processorcount = Environment.ProcessorCount;
        /// <summary>
        /// Our main forum initalisations.
        /// </summary>
        public Form1()
        {

            InitializeComponent();
            // base set and all that jazz for our form
            TB_DCS_PID1.Text = "Not Started";
            TB_DCS_PID2.Text = "Not Started";
            TB_DCS_PID3.Text = "Not Started";
            dcspath = Properties.Settings.Default.pathtodcs;
            textBox1.Text = Properties.Settings.Default.pathtodcs.ToString();
            cb_server1.Checked = Properties.Settings.Default.server1;
            cb_server2.Checked = Properties.Settings.Default.server2;
            cb_server3.Checked = Properties.Settings.Default.server3;
            cb_web1.Checked = Properties.Settings.Default.web1;
            cb_web2.Checked = Properties.Settings.Default.web2;
            cb_web3.Checked = Properties.Settings.Default.web3;
            cb_norender1.Checked = Properties.Settings.Default.norender1;
            cb_norender2.Checked = Properties.Settings.Default.norender2;
            cb_norender3.Checked = Properties.Settings.Default.norender3;
            tb_savefolder1.Text = Properties.Settings.Default.save1;
            tb_savefolder2.Text = Properties.Settings.Default.save2;
            tb_savefolder3.Text = Properties.Settings.Default.save3;
            defaulthangtime = Properties.Settings.Default.timeout;
            numericUpDown1.Value = defaulthangtime;
            cb_rs1.Checked = Properties.Settings.Default.rs1;
            cb_rs2.Checked = Properties.Settings.Default.rs2;
            cb_rs3.Checked = Properties.Settings.Default.rs3;
            cb_nonresponsive.Checked = Properties.Settings.Default.noresponse;
            num_serveruptime.Value = Properties.Settings.Default.rsvr1;
            num_serveruptime2.Value = Properties.Settings.Default.rsvr2;
            num_serveruptime3.Value = Properties.Settings.Default.rsvr3;
            synchour.Value = Properties.Settings.Default.synchr1;
            syncminutes.Value = Properties.Settings.Default.syncmin1;
            synchr2.Value = Properties.Settings.Default.synchr2;
            syncmin2.Value = Properties.Settings.Default.syncmin2;
            synchr3.Value = Properties.Settings.Default.synchr3;
            syncmin3.Value = Properties.Settings.Default.syncmin3;
            cb_forced.Checked = Properties.Settings.Default.forced;
            corecount.Text = processorcount.ToString();
            dcs1timer.Tick += new EventHandler(DCS1TimerEvent);
            dcs1timer.Interval = 60000;
            dcs2timer.Tick += new EventHandler(DCS2TimerEvent);
            dcs2timer.Interval = 60000;
            dcs3timer.Tick += new EventHandler(DCS3TimerEvent);
            dcs3timer.Interval = 60000;
            S1PA.Text = "0";
            PP.Text = "0";
        }

        /// <summary>
        /// this handles our main auto shutdown and restart of the DCS1 server
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="mYEventArgs"></param>
        void DCS1TimerEvent(Object myObject, EventArgs mYEventArgs)
        {
            dcs1uptime += 1;
            lb_uptime1.Text = dcs1uptime.ToString();
            DateTime currenttime = new DateTime();
            currenttime = System.DateTime.Now;
            int hour = currenttime.Hour;
            int minutes = currenttime.Minute;

            if (((dcs1uptime == num_serveruptime.Value) && (num_serveruptime.Value != 0)) || ((hour == Decimal.ToInt32(synchour.Value)) && (cb_server1forcerestart.Checked == true) && (cb_forced.Checked == true) && (minutes == Decimal.ToInt32(syncminutes.Value))))
            {
                lb_uptime1.Text = "Restarting";
                dcs1_stop();
                Thread.Sleep(200); 
                dcs1_start();
            }
        }
        
        /// <summary>
        /// This handles the main auto shut down and restart of the 2nd dcs server
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="mYEventArgs"></param>
        void DCS2TimerEvent(Object myObject, EventArgs mYEventArgs)
        {
            dcs2uptime += 1;
            lb_uptime2.Text = dcs2uptime.ToString();
            DateTime currenttime = new DateTime();
            currenttime = System.DateTime.Now;
            int hour = currenttime.Hour;
            int minutes = currenttime.Minute;
            if (((dcs2uptime == num_serveruptime2.Value) && (num_serveruptime2.Value != 0)) || ((hour == Decimal.ToInt32(synchr2.Value)) && (cb_server2forcerestart.Checked == true) && (cb_forced.Checked == true) && (minutes == Decimal.ToInt32(syncmin2.Value))))
            {
                lb_uptime2.Text = "Restarting";
                dcs2_stop();
                Thread.Sleep(200);
                dcs2_start();
            }

        }
        /// <summary>
        /// This handles the main auto shut down and restart of the 3rd dcs server.
        /// </summary>
        /// <param name="myObject"></param>
        /// <param name="mYEventArgs"></param>
        void DCS3TimerEvent(Object myObject, EventArgs mYEventArgs)
        {
            dcs3uptime += 1;
            lb_uptime3.Text = dcs3uptime.ToString();
            DateTime currenttime = new DateTime();
            currenttime = System.DateTime.Now;
            int hour = currenttime.Hour;
            int minutes = currenttime.Minute;
            if (((dcs3uptime == num_serveruptime3.Value) && (num_serveruptime3.Value != 0)) || ((hour == Decimal.ToInt32(synchr3.Value)) && (cb_server3forcerestart.Checked == true) && (cb_forced.Checked == true) && (minutes == Decimal.ToInt32(syncmin3.Value))))
            {
                lb_uptime3.Text = "Restarting";
                dcs3_stop();
                Thread.Sleep(200);
                dcs3_start();
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Starts dcs 1 or stops it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_savefolder1.Text == "")
                {
                    MessageBox.Show("Unable to start Server as you have no valid save game folder \n please enter a valid save folder.");
                }
                else
                {
                    if (dcs1start == false)
                    {
                        dcs1_start();
                    }
                    else
                    {
                        dcs1_stop();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("An Exception occured, please check your paths are correct! \n Exception was:" + ex.ToString());
            }
        }


        // DCS1 Items
        /// <summary>
        /// This actually STARTS the srever.
        /// </summary>
        void dcs1_start()
        {
            //make our path and set up our extensions
            if (Properties.Settings.Default.dedicated == false)
            { dcs1.StartInfo.FileName = dcspath + "\\bin\\dcs.exe";  }
            else
            {
              dcs1.StartInfo.FileName = dcspath + "\\bin\\DCS_Server.exe";
            }
            
            string aserver = "";
            string anorender = "";
            string awebgui = "";
            string asaved = "";
            if (cb_server1.Checked == true)
            {
                aserver = "--server";
            }
            
            if (cb_norender1.Checked == true)
            {
                anorender = "--norender";
            }
            
            if (cb_web1.Checked == true)
            {
                awebgui = "--webgui";
            }

            if (tb_savefolder1.Text != "")
            {
                asaved = "-w " + tb_savefolder1.Text;
            }

            
            dcs1.StartInfo.Arguments = aserver + " " + anorender + " " + awebgui + " " + asaved;
            // now we done that start the damned hting and store the process.
            try
            {
                dcs1start = dcs1.Start();
                dcs1pid = dcs1.Id;
                // put it to text and update the ui.
                TB_DCS_PID1.Text = dcs1pid.ToString();
                if (dcs1start == true)
                {
                    button1.Text = "Started";
                    tabPage1.Text = "DCS SERVER 1: Started";
                }
                else
                {
                    button1.Text = "Stopped";
                    tabPage1.Text = "DCS SERVER 1: Stopped";
                }
            }catch(Exception ex)
            {
                MessageBox.Show("Exception please tell rob \n" + ex.ToString());
            }
            //  we want events to trigger especially exited! and we want to reset uptime and start the timer.
            dcs1hangsrestart = 0;
            S1PA.Text = "0";
            dcs1.EnableRaisingEvents = true;
            dcs1.Exited += new EventHandler(dcs1_exited);
            dcs1uptime = 0;
            dcs1timer.Start();
            
        }
        /// <summary>
        /// Kills the process
        /// </summary>
        void dcs1_stop()
        {
            // does what it says, kills the process, updates the UI resets uptime and kills the timer.
            try
            {
                dcs1.Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception {0}", e.Message);
                MessageBox.Show("Exception please tell rob \n" + e.ToString());
            }
            button1.Text = "Stopped";
            tabPage1.Text = "DCS SERVER 1: Stopped";
            dcs1start = false;
            dcs1pid = 0;
            dcs1uptime = 0;
            dcs1timer.Stop();
        }
        /// <summary>
        /// If the process exit's for any reason set DCS1 Start to false.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void dcs1_exited(object sender, EventArgs e)
        {
            dcs1start = false;
        }

        // DCS 2 Items


        void dcs2_start()
        {
            if (Properties.Settings.Default.dedicated == false)
            { dcs2.StartInfo.FileName = dcspath + "\\bin\\dcs.exe"; }
            else
            {
                dcs2.StartInfo.FileName = dcspath + "\\bin\\DCS_Server.exe";
            }
            string aserver = "";
            string anorender = "";
            string awebgui = "";
            string asaved = "";
            if (cb_server2.Checked == true)
            {
                aserver = "--server";
            }

            if (cb_norender2.Checked == true)
            {
                anorender = "--norender";
            }

            if (cb_web2.Checked == true)
            {
                awebgui = "--webgui";
            }

            if (tb_savefolder2.Text != "")
            {
                asaved = "-w " + tb_savefolder2.Text;
            }

            dcs2.StartInfo.Arguments = aserver + " " + anorender + " " + awebgui + " " + asaved;
            dcs2start = dcs2.Start();
            dcs2pid = dcs2.Id;
            TB_DCS_PID2.Text = dcs2pid.ToString();
            if (dcs2start == true)
            {
                button2.Text = "Started";
                tabPage2.Text = "DCS SERVER 2: Started";

            }
            else
            {
                button2.Text = "Stopped";
                tabPage2.Text = "DCS SERVER 2: Stopped";
            }
            dcs2.EnableRaisingEvents = true;
            dcs2.Exited += new EventHandler(dcs2_exited);
            dcs2uptime = 0;
            dcs2timer.Start();
        }
        void dcs2_stop()
        {
            try
            {
                dcs2.Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception {0}", e.Message);
            }
            button2.Text = "Stopped";
            tabPage2.Text = "DCS SERVER 2: Stopped";
            dcs2start = false;
            dcs2pid = 0;
            dcs2uptime = 0;
            dcs2timer.Stop();
        }

        void dcs2_exited(object sender, EventArgs e)
        {
            dcs2start = false;
        }

        // dcs 3 items


        void dcs3_start()
        {
            if (Properties.Settings.Default.dedicated == false)
            { dcs3.StartInfo.FileName = dcspath + "\\bin\\dcs.exe"; }
            else
            {
                dcs3.StartInfo.FileName = dcspath + "\\bin\\DCS_Server.exe";
            }
            string aserver = "";
            string anorender = "";
            string awebgui = "";
            string asaved = "";
            if (cb_server3.Checked == true)
            {
                aserver = "--server";
            }

            if (cb_norender3.Checked == true)
            {
                anorender = "--norender";
            }

            if (cb_web3.Checked == true)
            {
                awebgui = "--webgui";
            }

            if (tb_savefolder3.Text != "")
            {
                asaved = "-w " + tb_savefolder3.Text;
            }

            dcs3.StartInfo.Arguments = aserver + " " + anorender + " " + awebgui + " " + asaved;
            dcs3start = dcs3.Start();
            dcs3pid = dcs3.Id;
            TB_DCS_PID3.Text = dcs3pid.ToString();
            if (dcs3start == true)
            {
                button3.Text = "Started";
                tabPage3.Text = "DCS SERVER 3: Started";
            }
            else
            {
                button3.Text = "Stopped";
                tabPage3.Text = "DCS SERVER 3: Stopped";
            }
            dcs3.EnableRaisingEvents = true;
            dcs3.Exited += new EventHandler(dcs3_exited);
            dcs3uptime = 0;
            dcs3timer.Start();
        }
        void dcs3_stop()
        {
            try
            {
                dcs3.Kill();
            }
            catch (Exception e)
            {
                Console.WriteLine("exception {0}", e.Message);
            }
            button3.Text = "Stopped";
            tabPage3.Text = "DCS SERVER 3: Stopped";
            dcs3start = false;
            dcs3pid = 0;
            dcs3uptime = 0;
            dcs3timer.Stop();
        }

        void dcs3_exited(object sender, EventArgs e)
        {
            dcs3start = false;
        }
        // our timer


        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime currenttime = new DateTime();
            currenttime = System.DateTime.Now;
            curtime.Text = "Current Time:" + currenttime.Hour.ToString() + "hrs " + currenttime.Minute.ToString() + "minutes";
            if (dcs1start == true)
            {
                if (cb_nonresponsive.Checked == true)
                { 
                    if (dcs1.Responding == true) 
                    {
                        dcs1hangtime = 0;
                        button1.Text = "Started";
                        tabPage1.Text = "DCS SERVER 1: Running";
                        hanging1 = false;
                    }
                    else
                    {
                        if (hanging1 == false)
                        {
                            dcs1totalhangs = dcs1totalhangs + 1;
                            dcs1hangsrestart = dcs1hangsrestart + 1;
                            hanging1 = true;
                            S1PA.Text = dcs1hangsrestart.ToString();
                            PP.Text = dcs1totalhangs.ToString();
                        }
                        dcs1hangtime = dcs1hangtime + 1;
                        button1.Text = "NR Time:" + dcs1hangtime.ToString() + "/" + defaulthangtime;
                        tabPage1.Text = "DCS SERVER 1: NR Time" + dcs1hangtime.ToString() + "/" + defaulthangtime;
                        if (dcs1hangtime >= defaulthangtime)
                        {
                            try
                            {
                                dcs1.Kill();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("exception {0}", ex.Message);
                            }

                            
                            dcs1_start();
                        }
                    }
                }
                else
                {
                    dcs1hangtime = 0;
                    button1.Text = "Started";
                    tabPage1.Text = "DCS SERVER 1: Running";
                    hanging1 = false;
                }
            }
            else
            {
                button1.Text = "Stopped";
                tabPage1.Text = "DCS SERVER 1: Stopped";
                dcs1pid = 0;
                TB_DCS_PID1.Text = "Not Started";
                if (Properties.Settings.Default.rs1 == true)
                {
                    dcs1_start();
                }
            }


            if (dcs2start == true)
            {
                if (cb_nonresponsive.Checked == true)
                {
                    if (dcs2.Responding == true)
                    {
                        dcs2hangtime = 0;
                        button2.Text = "Started";
                        tabPage2.Text = "DCS SERVER 2: Running";
                        hanging2 = false;
                    }
                    else
                    {
                        if (hanging2 == false)
                        {
                            dcs2totalhangs = dcs2totalhangs + 1;
                            dcs2hangsrestart = dcs2hangsrestart + 1;
                            hanging2 = true;
                            s2nr.Text = dcs2hangsrestart.ToString();
                            S2th.Text = dcs2totalhangs.ToString();
                        }
                        dcs2hangtime = dcs2hangtime + 1;
                        button2.Text = "NR Time:" + dcs2hangtime.ToString() + "/" + defaulthangtime;
                        tabPage2.Text = "DCS SERVER 2: NonResponsive Time" + dcs2hangtime.ToString() + "/" + defaulthangtime;
                        if (dcs2hangtime >= defaulthangtime)
                        {
                            try
                            {
                                dcs2.Kill();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("exception {0}", ex.Message);
                            }
                            dcs2_start();
                        }
                    }
                }
                else
                {
                        dcs2hangtime = 0;
                        button2.Text = "Started";
                        tabPage2.Text = "DCS SERVER 2: Running";
                        hanging2 = false;
                }
            }
            else
            {
                button2.Text = "Stopped";
                tabPage2.Text = "DCS SERVER 2: Stopped";
                dcs2pid = 0;
                TB_DCS_PID2.Text = "Not Started";
                if (Properties.Settings.Default.rs2 == true)
                {
                    dcs2_start();
                }
            }

            if (dcs3start == true)
            {
                if (cb_nonresponsive.Checked == true)
                {
                    if (dcs3.Responding == true)
                    {
                        dcs3hangtime = 0;
                        button3.Text = "Started";
                        tabPage3.Text = "DCS SERVER 3: Running";
                        hanging3 = false;
                    }
                    else
                    {
                        if (hanging3 == false)
                        {
                            dcs3totalhangs = dcs3totalhangs + 1;
                            dcs3hangsrestart = dcs3hangsrestart + 1;
                            hanging3 = true;
                            srv3nr.Text = dcs3hangsrestart.ToString();
                            svr3th.Text = dcs3totalhangs.ToString();
                        }
                        dcs3hangtime = dcs3hangtime + 1;
                        button3.Text = "NR Time:" + dcs3hangtime.ToString() + "/" + defaulthangtime;
                        tabPage2.Text = "DCS SERVER 3: NonResponsive Time" + dcs3hangtime.ToString() + "/" + defaulthangtime;
                        if (dcs3hangtime >= defaulthangtime)
                        {
                            try
                            {
                                dcs3.Kill();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("exception {0}", ex.Message);
                            }
                            dcs3_start();
                        }
                    }
                }
                else
                {
                        dcs3hangtime = 0;
                        button3.Text = "Started";
                        tabPage3.Text = "DCS SERVER 3: Running";
                        hanging3 = false;
                }
            }
            else
            {
                button3.Text = "Stopped";
                tabPage3.Text = "DCS SERVER 3: Stopped";
                dcs3pid = 0;
                TB_DCS_PID3.Text = "Not Started";
                if (Properties.Settings.Default.rs3 == true)
                {
                    dcs3_start();
                }
            }
            if (firstrun1 == true)
            {
                firstrun1 = false;
                firstrun2 = false;
                firstrun3 = false;
            }
        }


        // This saves all our data.


        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath.ToString();
                dcspath = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.pathtodcs = dcspath;
                Properties.Settings.Default.Save();
            }
        }

        private void cb_server1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.server1 = cb_server1.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_server2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.server2 = cb_server2.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_server3_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.server3 = cb_server3.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_norender1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.norender1 = cb_norender1.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_norender2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.norender2 = cb_norender2.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_norender3_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.norender3 = cb_norender3.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_web1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.web1 = cb_web1.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_web2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.web2 = cb_web2.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_web3_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.web3 = cb_web3.Checked;
            Properties.Settings.Default.Save();
        }

        private void tb_savefolder1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.save1 = tb_savefolder1.Text;
            Properties.Settings.Default.Save();
        }

        private void tb_savefolder2_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.save2 = tb_savefolder2.Text;
            Properties.Settings.Default.Save();
        }

        private void tb_savefolder3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.save3 = tb_savefolder3.Text;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_savefolder2.Text == "")
                {
                    MessageBox.Show("Unable to start Server as you have no valid save game folder \n please enter a valid save folder.");
                }
                else
                {
                    if (dcs2start == false)
                    {
                        dcs2_start();
                    }
                    else
                    {
                        dcs2_stop();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An Exception occured, please check your paths are correct! \n Exception was:" + ex.ToString());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_savefolder3.Text == "")
                {
                    MessageBox.Show("Unable to start Server as you have no valid save game folder \n please enter a valid save folder.");
                }
                else
                {
                    if (dcs3start == false)
                    {
                        dcs3_start();
                    }
                    else
                    {
                        dcs3_stop();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An Exception occured, please check your paths are correct! \n Exception was:" + ex.ToString());
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            decimal hang = numericUpDown1.Value;
            defaulthangtime = Decimal.ToInt32(hang);
            Properties.Settings.Default.timeout = defaulthangtime;
            Properties.Settings.Default.Save();
        }

        private void cb_rs1_CheckedChanged(object sender, EventArgs e)
        {
            if (firstrun1 == true)
            {
                firstrun1 = false;
            }
            else if (cb_rs1.Checked == true)
            {
                DialogResult dr = MessageBox.Show("Activing this will Automatically start the server instantly, please make certain you have everything filled in", "Are you certain?", MessageBoxButtons.YesNo);
                if(dr == DialogResult.Yes)
                {
                    cb_rs1.Checked = true;
                    num_serveruptime.Enabled = false;
                    Properties.Settings.Default.rs1 = cb_rs1.Checked;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    cb_rs1.Checked = false;
                    Properties.Settings.Default.rs1 = false;
                    Properties.Settings.Default.Save();
                    num_serveruptime.Enabled = true;
                }
            }
            else
            {
                Properties.Settings.Default.rs1 = cb_rs1.Checked;
                Properties.Settings.Default.Save();
                num_serveruptime.Enabled = true;
            }
            
        }

        private void cb_rs2_CheckedChanged(object sender, EventArgs e)
        {
            if (firstrun2 == true)
            {
                firstrun2 = false;
            }
            else if (cb_rs2.Checked == true)
            {
                DialogResult dr = MessageBox.Show("Activing this will Automatically start the server instantly, please make certain you have everything filled in", "Are you certain?", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    cb_rs2.Checked = true;
                    num_serveruptime2.Enabled = false;
                    Properties.Settings.Default.rs2 = cb_rs2.Checked;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    cb_rs2.Checked = false;
                    Properties.Settings.Default.rs2 = false;
                    Properties.Settings.Default.Save();
                    num_serveruptime2.Enabled = true;
                }
            }
            else
            {
                Properties.Settings.Default.rs2 = cb_rs2.Checked;
                Properties.Settings.Default.Save();
                num_serveruptime2.Enabled = true;
            }
        }

        private void cb_rs3_CheckedChanged(object sender, EventArgs e)
        {
            if (firstrun3 == true)
            {
                firstrun3 = false;
            }
            else if (cb_rs3.Checked == true)
            {
                DialogResult dr = MessageBox.Show("Activing this will Automatically start the server instantly, please make certain you have everything filled in", "Are you certain?", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    if (tb_savefolder3.Text == "")
                    {
                        MessageBox.Show("Unable to start Save Folder is EMPTY Enter the save folder name in SAVED GAMES");
                    }
                    else
                    {
                        cb_rs3.Checked = true;
                        num_serveruptime3.Enabled = false;
                        Properties.Settings.Default.rs3 = cb_rs3.Checked;
                        Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    cb_rs3.Checked = false;
                    Properties.Settings.Default.rs3 = false;
                    Properties.Settings.Default.Save();
                    num_serveruptime3.Enabled = true;
                }
            }
            else
            {
                Properties.Settings.Default.rs3 = cb_rs3.Checked;
                Properties.Settings.Default.Save();
                num_serveruptime3.Enabled = true;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                
                srspath1 = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.srspath1 = srspath1;
                Properties.Settings.Default.Save();
            }
        }

        private void cb_autosrs_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void num_serveruptime_ValueChanged(object sender, EventArgs e)
        {
            
            Properties.Settings.Default.rsvr1 = Decimal.ToInt32(num_serveruptime.Value);
            Properties.Settings.Default.Save();
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.rsvr2 = Decimal.ToInt32(num_serveruptime2.Value);
            Properties.Settings.Default.Save();
        }

        private void num_serveruptime3_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.rsvr3 = Decimal.ToInt32(num_serveruptime3.Value);
            Properties.Settings.Default.Save();
        }

        private void synchour_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.synchr1 = Decimal.ToInt32(synchour.Value);
            Properties.Settings.Default.Save();
        }

        private void syncminutes_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.syncmin1 = Decimal.ToInt32(syncminutes.Value);
            Properties.Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void TB_SRSPath_TextChanged(object sender, EventArgs e)
        {

        }

        private void caffin_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void synchr2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.synchr2 = Decimal.ToInt32(synchr2.Value);
            Properties.Settings.Default.Save();
        }

        private void syncmin2_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.syncmin2 = Decimal.ToInt32(syncmin2.Value);
            Properties.Settings.Default.Save();
        }

        private void synchr3_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.synchr3 = Decimal.ToInt32(synchr3.Value);
            Properties.Settings.Default.Save();
        }

        private void syncmin3_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.syncmin3 = Decimal.ToInt32(syncmin3.Value);
            Properties.Settings.Default.Save();
        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void cb_forced_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.forced = cb_forced.Checked;
            Properties.Settings.Default.Save();
        }

        private void cb_nonresponsive_CheckedChanged(object sender, EventArgs e)
        {
            MessageBox.Show("Be Aware DCS 2.7.0.46250 does not support this for some reason \n Advise setting it FALSE (OFF)");
            Properties.Settings.Default.noresponse = cb_nonresponsive.Checked;
            Properties.Settings.Default.Save();
        }

        private void standalone_CheckedChanged(object sender, EventArgs e)
        {
            MessageBox.Show("This sets your server to a dedicated server install \n It will look for dcs_server.exe instead of dcs.exe");
            Properties.Settings.Default.dedicated = standalone.Checked;
            Properties.Settings.Default.Save();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
