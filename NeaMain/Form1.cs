using NPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static NeaMain.Program;
namespace NeaMain
{
    public partial class Form1 : Form
    {
        int amount;
        double[] dataX1;
        double[] dataY1;
        public Form1(double[] dataX, double[] dataY)
        {
            dataX1 = dataX;
            dataY1 = dataY;
            InitializeComponent();
            

            
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            
            
            
            
            ListViewItem item;
            
            for(int i = 1;i< user.stocks.Count; i++)
            {
                item = new ListViewItem(user.stocks[i].stock.ticker);
                item.SubItems.Add(user.stocks[i].Amount.ToString());
                item.SubItems.Add(user.stocks[i].AverageBuyPrice.ToString());
                
                listView1.Items.Add(item);
            }
            
            
            
            formsPlot1.Plot.Add.Scatter(dataX1, dataY1);
            formsPlot1.Refresh();
            label2.Text = user.cash.ToString();
        }

        private void formsPlot1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            amount = Convert.ToInt32(numericUpDown1.Value);
            user.buy(amount, list[0]);
            label2.Text = user.cash.ToString();
            ListViewItem item;
            listView1.Items.Clear();
            for (int i = 1; i < user.stocks.Count; i++)
            {
                item = new ListViewItem(user.stocks[i].stock.ticker);
                item.SubItems.Add(user.stocks[i].Amount.ToString());
                item.SubItems.Add(user.stocks[i].AverageBuyPrice.ToString());

                listView1.Items.Add(item);
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            amount = Convert.ToInt32(numericUpDown1.Value);
        }

        private void Sell_Click(object sender, EventArgs e)
        {
            user.sell(amount, list[0]);
            label2.Text = user.cash.ToString();
            ListViewItem item;
            listView1.Items.Clear();
            for (int i = 1; i < user.stocks.Count; i++)
            {
                item = new ListViewItem(user.stocks[i].stock.ticker);
                item.SubItems.Add(user.stocks[i].Amount.ToString());
                item.SubItems.Add(user.stocks[i].AverageBuyPrice.ToString());

                listView1.Items.Add(item);
            }
        }
    }
}
