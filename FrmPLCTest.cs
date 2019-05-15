using HslCommunication.BasicFramework;
using HslCommunication.LogNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestPLC
{
    public partial class FrmPLCTest : Form
    {
        private ILogNet logNet2;

        private ILogNet LogNetInt32;
        private static bool ThreadState1 = false;
        //private static bool ThreadState2 = false;
        //private static bool ThreadState3 = false;
        private static bool ThreadState4 = false;
        //private static bool ThreadState = false;
        private static bool ThreadState6 = false;
        public FrmPLCTest()
        {
            InitializeComponent();
        }
        private LibnodavePLC _LibnodavePLC;

        #region 线程间访问控件处理
        public delegate void SetTextCallback(string str);

        public void SetText(string str)
        {
            if (label1.InvokeRequired)
            {
                // 解决窗体关闭时出现“访问已释放句柄”异常
                while (label1.IsHandleCreated == false)
                {
                    if (label1.Disposing || label1.IsDisposed) return;
                }

                SetTextCallback d = new SetTextCallback(SetText);
                label10.Invoke(d, new object[] { str });

            }
            else
            {
                label10.Text = str;
            }

        }
        #endregion

        private void FrmPLCTest_Load(object sender, EventArgs e)
        {

        }
        private void LibCreate()
        {
            _LibnodavePLC = new LibnodavePLC();
            bool isopen = _LibnodavePLC.Init(Txt_IP.Text);
            if (isopen)
            {
                this.SetText("连接成功");
                label10.BackColor = Color.Green;
            }
            else
            {
                this.SetText("断开连接");
                label10.BackColor = Color.Red;
            }
        }

        private void Btn_Connect_Click(object sender, EventArgs e)
        {
            Task.Run(() => LibCreate());
        }

        private void Btn_FloatTest_Click(object sender, EventArgs e)
        {
            if (ThreadState1)
            {
                return;
            }
            Task.Run(() => AsyncFloatStart(Connect(txt_faddr.Text)));
        }

        private LibnodavePLC Connect(string addr)
        {

            LibnodavePLC _LIBnodavePLC = new LibnodavePLC();
            bool isopen = _LIBnodavePLC.Init(Txt_IP.Text);
            _LIBnodavePLC.Device = _LIBnodavePLC.GetDeviceAddress(addr);
            return _LIBnodavePLC;
        }

        private void AsyncFloatStart(LibnodavePLC lIBnodavePLC)
        {
            int ERROR = 0;
            string recive = null;
            logNet2 = new LogNetSingle("FloatDataLog.txt");
            ThreadState1 = true;
            Task.Run(() =>
            {
                float _value = 1.00f;
                while (true)
                {
                    _value = _value + 0.01f;
                    lIBnodavePLC.WriteFloat(_value);
                    this.Invoke(new Action(() =>
                    {
                        logNet2.RecordMessage(HslMessageDegree.DEBUG, "写入", _value.ToString());
                        recive = lIBnodavePLC.ReadFloat().ToString();
                        logNet2.RecordMessage(HslMessageDegree.DEBUG, "读取", recive);
                        if (_value.ToString() != recive.ToString())
                        {
                            ERROR++;
                        }
                        logNet2.RecordMessage(HslMessageDegree.DEBUG, "ERROR", ERROR.ToString());
                    }
                                      )
                                      );
                }
            });
        }

        private void Btn_StrTest_Click(object sender, EventArgs e)
        {
            if (ThreadState4)
            {
                return;
            }
            LibInit();
        }

        private void LibInit()
        {
            try
            {
                ThreadState4 = true;
                logNet2 = new LogNetSingle("StrDataLog.txt");
                _LibnodavePLC = new LibnodavePLC();
                bool isopen = _LibnodavePLC.Init("DB1.0", Txt_IP.Text); ;
                byte[] ReciveData = new byte[1000];
                string sendmessage = "";
                Task.Run(() =>
                {
                    while (true)
                    {
                        this.Invoke(new Action(() =>
                        {
                            lock (this)
                            {
                                sendmessage = Txt_Send.Text = GetRandomString(400);
                            }
                        }
                        )
                        );

                        _LibnodavePLC.WriteBytes(SoftBasic.HexStringToBytes(Txt_Send.Text));
                        logNet2.RecordMessage(HslMessageDegree.DEBUG, "写入", sendmessage);
                        ReciveData = _LibnodavePLC.ReadBytes(200);
                        this.Invoke(new Action(() =>
                        {
                            lock (this)
                                Txt_Recieve.Text = SoftBasic.ByteToHexString(ReciveData);
                            logNet2.RecordMessage(HslMessageDegree.DEBUG, "读取", Txt_Recieve.Text);
                            logNet2.RecordMessage(HslMessageDegree.DEBUG, null, "是否正常" + Check(Txt_Recieve.Text, Txt_Send.Text));
                        }));
                    }
                }
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #region 5.0 生成随机字符串 + static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        ///<summary>
        ///生成随机字符串 
        ///</summary>
        ///<param name="length">目标字符串的长度</param>
        ///<param name="useNum">是否包含数字，1=包含，默认为包含</param>
        ///<param name="useLow">是否包含小写字母，1=包含，默认为包含</param>
        ///<param name="useUpp">是否包含大写字母，1=包含，默认为包含</param>
        ///<param name="useSpe">是否包含特殊字符，1=包含，默认为不包含</param>
        ///<param name="custom">要包含的自定义字符，直接输入要包含的字符列表</param>
        ///<returns>指定长度的随机字符串</returns>
        public static string GetRandomString(int length, bool useNum = true, bool useLow = false, bool useUpp = false)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = "";
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }

            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
        #endregion

        private bool Check(string a, string b)
        {
            int result = a.CompareTo(b);
            if (result == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void BtnTestCount_Click(object sender, EventArgs e)
        {
            if (ThreadState6)
            {

                return;
            }
            Task.Run(() => AsyncInt32Start(Connect(txtInt32addr.Text)));
        }
        private void AsyncInt32Start(LibnodavePLC lIBnodavePLC)
        {
            try
            {
                int ERROR = 0;
                string log = string.Format("Int32Log.txt");
                LogNetInt32 = new LogNetSingle(log);
                string recive = null;
                ThreadState6 = true;
                Task task = new Task(() =>
                {
                    int _value = 2147483647;
                    while (true)
                    {
                        _value = _value + 1;
                        lIBnodavePLC.WriteInt32(_value);

                        //  Txt_Send.Text = _value.ToString();
                        LogNetInt32.RecordMessage(HslMessageDegree.DEBUG, "写入", _value.ToString());
                        recive = lIBnodavePLC.ReadInt32().ToString();
                        LogNetInt32.RecordMessage(HslMessageDegree.DEBUG, "读取", recive);

                        if (_value.ToString() != recive.ToString())
                        {
                            ERROR++;
                        }
                        LogNetInt32.RecordMessage(HslMessageDegree.DEBUG, "ERROR", ERROR.ToString());
                    }
                });
                task.Start();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
