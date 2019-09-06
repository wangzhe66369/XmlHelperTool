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
using System.Xml.Linq;

namespace WindowsFormsApp1
{
    public partial class XmlTool : Form
    {
        private BackgroundWorker bgWorker = new BackgroundWorker();
        XDocument docTarget = null;
        IEnumerable<XElement> target = null;
        IEnumerable<XElement> elementsSource = null;
 
        public XmlTool()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }
        string filePath1 = string.Empty;
        string filePath2 = string.Empty;
        private void button1_Click(object sender, EventArgs e)
        {
            //创建文件弹出选择窗口（包括文件名）对象
            OpenFileDialog ofd = new OpenFileDialog();
            //判断选择的路径
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.txtSoundPath1.Text = ofd.FileName.ToString();
            }
            filePath1 = this.txtSoundPath1.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //创建文件弹出选择窗口（包括文件名）对象
            OpenFileDialog ofd = new OpenFileDialog();
            //判断选择的路径
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.txtSoundPath2.Text = ofd.FileName.ToString();
            }
            filePath2 = this.txtSoundPath2.Text;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (bgWorker.IsBusy)
                return;

            //将要修改的xml
            docTarget = XDocument.Load(filePath1);
            XElement rootNodeTarget = docTarget.Root;
            var childTarget = docTarget.Descendants().Where(d => d.Name == "data");
            target = childTarget.Elements();
            this.progressBar1.Maximum = target.Count();
            //提供修改的数据
            var docSource = XDocument.Load(filePath2);
            XElement rootNodeSource = docSource.Root;
            elementsSource = rootNodeSource?.Elements();
            
            //this.progressBar1.Maximum = 100;
            bgWorker.RunWorkerAsync("hello");
        }


        private void InitializeBackgroundWorker()
        {
            bgWorker.WorkerReportsProgress = true;//能否报告进度更新。
            bgWorker.WorkerSupportsCancellation = true;//是否支持异步取消
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgessChanged);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_WorkerCompleted);
        }

        public void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //句柄sender指向的就是该BackgroundWorker。

            //e.Argument 获取异步操作参数的值  
            //e.Cancel 是否应该取消事件
            //e.Result  获取或设置异步操作结果的值(在RunWorkerCompleted事件可能会使用到)
            //object a = e.Argument;//获取RunWorkerAsync(object argument)传入的值

        
            int i = 0;
            int j = 0;
            foreach (var item in target)
            {
                if (bgWorker.CancellationPending)//在耗时操作中判断CancellationPending属性，如果为false则退出
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                   
                    string parentvalue = item.Parent.Attribute("name").Value;
                    var source = elementsSource.Where(d => d.Attribute("name")?.Value == parentvalue);
                    var sourceValue = source?.Descendants()?.FirstOrDefault()?.Value;
                    if (sourceValue != null)
                    {
                        if (item.Value != sourceValue)
                        {
                            if (item.Name == "value")
                            {
                                j++;
                                item.Value = sourceValue;
                            }
                            else if (item.Name == "comment")
                            {
                                item.Remove();
                            }
                            docTarget.Save(filePath1);
                        }
                    }
                    
                    i++;
                    bgWorker.ReportProgress(i, j);// 将触发BackgroundWorker.ProgressChanged事件，向ProgressChanged报告进度
                }
            }
        }

        public void bgWorker_ProgessChanged(object sender, ProgressChangedEventArgs e)
        {
            //string state = (string)e.UserState;//接收ReportProgress方法传递过来的userState
            this.progressBar1.Value = e.ProgressPercentage;
            //e.ProgressPercentage  获取异步操作进度的百分比
            //this.prompt.Text = "处理条数:" + Convert.ToString(e.ProgressPercentage);
            this.prompt.Text = "处理条数:" + Convert.ToString(e.UserState);

        }
        /// <summary>
        /// 当DoWork事件处理完成之后，将会触发该事件。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void bgWorker_WorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            //e.Cancelled指示异步操作是否已被取消
            //e.Error 指示异步操作期间发生的错误
            //e.Result 获取异步操作结果的值,即DoWork事件中，Result设置的值。
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString());
                return;
            }
            //if (!e.Cancelled)
            //    this.prompt.Text = "处理完毕!";
            //else
            //    this.prompt.Text = "处理终止!";

        }

        private void btnStop_Click_1(object sender, EventArgs e)
        {
            bgWorker.CancelAsync();
        }
    }
}
