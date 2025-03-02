using NPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeaMain
{
    public static class Program
    {

        public class decisionTree
        {
            private List<PerformanceData> PerfData;
            public bool Predict(PerformanceData data, Node node)
            {
                if (node.Leaf)
                {
                    return node.Label;
                }

                if (node.Feature == "bef")
                {
                    if (data.percIncreaseB10H < node.Value)
                        return Predict(data, node.Left);
                    else
                        return Predict(data, node.Right);
                }
                else
                {
                    if (data.percIncreaseA10H < node.Value)
                        return Predict(data, node.Left);
                    else
                        return Predict(data, node.Right);
                }
            }
            public decisionTree(List<PerformanceData> info)
            {
                this.PerfData = info;
            }
            private decimal GiniCalc(List<PerformanceData> infor)
            {
                if (infor.Count > 0)
                {
                    int total = infor.Count;
                    int good = IsGoodBuy(infor);
                    int bad = total - good;


                    decimal probGood = (decimal)good / total;
                    decimal probBad = (decimal)bad / total;
                    return 1 - (probGood * probGood + probBad * probBad);
                }
                else
                {
                    return 1;
                }

            }

            private int IsGoodBuy(List<PerformanceData> infor)
            {
                int numberGood = 0;
                for (int i = 0; i < infor.Count; i++)
                {
                    if (infor[i].goodBuy)
                    {
                        numberGood++;
                    }
                }
                return numberGood;
            }
            private (List<PerformanceData> left, List<PerformanceData> right) SplitData(List<PerformanceData> list, string feature, decimal limit, int minSize = 5)
            {
                List<PerformanceData> left = new List<PerformanceData>();
                List<PerformanceData> right = new List<PerformanceData>();

                for (int i = 0; i < list.Count; i++)
                {
                    if (feature == "bef")
                    {
                        if (list[i].percIncreaseB10H < limit)
                        { left.Add(list[i]); }
                        else
                        { right.Add(list[i]); }
                    }
                    else
                    {
                        if (list[i].percIncreaseA10H < limit)
                        { left.Add(list[i]); }
                        else
                        { right.Add(list[i]); }
                    }
                }
                if (left.Count >= minSize && right.Count >= minSize)
                {
                    return (left, right);
                }
                else
                {
                    return (new List<PerformanceData>(), new List<PerformanceData>());
                }
            }

            


            public Node Tree(List<PerformanceData> info, int depth = 0, int maxDepth = 10)
            {
                if (info.Count == 0 || depth >= maxDepth || info.All(x => x.goodBuy) || info.All(x => !x.goodBuy))
                {
                    return new Node { Leaf = true, Label = IsGoodBuy(info) > info.Count / 2 };
                }

                decimal bestGini = 1;
                string bestFeature = "";
                decimal bestValue = 0;
                Node bestLeft = null;
                Node bestRight = null;
                foreach (var feature in new[] { "bef", "aft" })
                {
                    // Getting all possible threshold values (there is probably a more optimal way to do this requiring less processing power considering my datasize)
                    HashSet<decimal> thresholds = new HashSet<decimal>();

                    for (int i = 0; i < info.Count; i++)
                    {
                        decimal value = 0;
                        if (feature == "bef")
                        {
                            if (thresholds.Contains(info[i].percIncreaseB10H))
                            {

                            }
                            else
                            {
                                value = info[i].percIncreaseB10H;
                            }

                        }
                        else
                        {
                            if (thresholds.Contains(info[i].percIncreaseA10H))
                            {

                            }
                            else
                            {
                                value = info[i].percIncreaseA10H;
                            }

                        }
                        if (value != 0)
                        {
                            thresholds.Add(value);
                        }

                    }

                    



                    
                    foreach (var candidate in thresholds)
                    {
                        var split = SplitData(info, feature, candidate);
                        List<PerformanceData> left = split.left;
                        List<PerformanceData> right = split.right;

                        
                        if (left.Count == 0 || right.Count == 0)
                            continue;

                        
                        decimal gini = (GiniCalc(left) * left.Count / info.Count) +
                                       (GiniCalc(right) * right.Count / info.Count);

                        
                        if (gini < bestGini)
                        {
                            bestGini = gini;
                            bestFeature = feature;
                            bestValue = candidate;
                            Task<Node> leftTask = Task.Run(() => Tree(split.left, depth + 1, maxDepth));
                            Task<Node> rightTask = Task.Run(() => Tree(split.right, depth + 1, maxDepth));

                            Task.WaitAll(leftTask, rightTask);
                            bestLeft = leftTask.Result;
                            bestRight = rightTask.Result;
                        }
                    }
                }


                if (bestLeft == null || bestRight == null) 
                {
                    return new Node { Leaf = true, Label = IsGoodBuy(info) > info.Count / 2 };
                }

                return new Node
                {
                    Value = bestValue,
                    Feature = bestFeature,
                    Left = bestLeft,
                    Right = bestRight
                };

            }

        }
        public class PerformanceData
        {
            //Percentage increase/decrease between the price 10 hours ago and the current price
            public decimal percIncreaseB10H;
            //Percentage increase/decrease between the price now and 10 hours from now
            public decimal percIncreaseA10H;
            public bool goodBuy;
            public void setgoodBuy()
            {
                if (percIncreaseA10H - percIncreaseB10H > 9)
                {
                    goodBuy = true;

                }
                else
                {
                    goodBuy = false;
                }
            }
            public void print()
            {
                Console.WriteLine("10 Hour Increase " + percIncreaseB10H);
                Console.WriteLine("Increase after 10H " + percIncreaseA10H);
            }
        }
        public class Node
        {
            public decimal Value { get; set; }
            public string Feature { get; set; } // getters and setters like this don't work, dunno how I hadnt noticed 
            public Node Left { get; set; }
            public Node Right { get; set; }
            public bool Label { get; set; }
            public bool Leaf { get; set; }

        }
        public class StockItem
        {
            public string date, ticker;

            private decimal open, high, low, close, volume;
            public decimal GetClose()
            {
                return close;
            }
            public void SetOpen(string open)
            {
                this.open = Convert.ToDecimal(open);

            }
            public void SetHigh(string high)
            {
                this.high = Convert.ToDecimal(high);

            }
            public void SetLow(string low)
            {
                this.low = Convert.ToDecimal(low);
            }
            public void SetClose(string close)
            {
                this.close = Convert.ToDecimal(close);
            }
            public void SetVolume(string volume)
            {
                this.volume = Convert.ToDecimal(volume);
            }
            public void print()
            {
                Console.WriteLine("Date " + date);
                Console.WriteLine("Open:" + open);
                Console.WriteLine("High " + high);
                Console.WriteLine("Low " + low);
                Console.WriteLine("Close " + close);
                Console.WriteLine("Volume" + volume);
                Console.WriteLine();
            }
            public StockItem(){
                ticker = "ETH"; //might want to change this later
            }

        }
        public class Purchase {
            public decimal AverageBuyPrice { get; set; }
            public int Amount { get; set; }
            public StockItem stock {  get; set; }
            public Purchase() { 
                stock = new StockItem();
            
            }
        }
        public class UserData
        {
            public string name;
            public decimal cash;
            public List<Purchase> stocks;
            public void buy(int amount, StockItem stock)
            {
                
                Purchase purchase = new Purchase();
                decimal price = 0;
                bool flag = false;
                price = amount * stock.GetClose();
                if (cash>price)
                {
                    cash = cash - price;
                    for (int i = 0; i < stocks.Count; i++)
                    {
                        if (stock.ticker == stocks[i].stock.ticker && stock.date == stocks[i].stock.date)
                        {
                            stocks[i].AverageBuyPrice = (stocks[i].AverageBuyPrice * stocks[i].Amount + price) / (amount + stocks[i].Amount);
                            stocks[i].Amount = stocks[i].Amount + amount;
                            File.Delete("userdata.txt");
                            StreamWriter writer = new StreamWriter("userdata.txt");
                            writer.WriteLine("Name");
                            writer.WriteLine(name);
                            writer.WriteLine("Initial cash");
                            writer.WriteLine(cash);
                            for (int j = 0; j < stocks.Count; j++)
                            {
                                writer.WriteLine("Avg Price");
                                writer.WriteLine(stocks[j].AverageBuyPrice);
                                writer.WriteLine("Amount");
                                writer.WriteLine(stocks[j].Amount);
                                writer.WriteLine("Date");
                                writer.WriteLine(stocks[j].stock.date);
                                writer.WriteLine("Ticker");
                                writer.WriteLine(stocks[j].stock.ticker);
                                
                            }
                            flag = true;
                            writer.Close();
                        }
                    }
                    if (flag == false)
                    {
                        StreamWriter writer = new StreamWriter("userdata.txt");
                        purchase.stock = stock;
                        purchase.Amount = amount;
                        purchase.AverageBuyPrice = stock.GetClose();
                        stocks.Add(purchase);
                        writer.WriteLine("Name");
                        writer.WriteLine(name);
                        writer.WriteLine("Initial cash");
                        writer.WriteLine(cash);
                        writer.WriteLine("Avg Price");
                        writer.WriteLine(stock.GetClose());
                        writer.WriteLine("Amount");
                        writer.WriteLine(amount);
                        writer.WriteLine("Date");
                        writer.WriteLine(stock.date);
                        writer.WriteLine("Ticker");
                        writer.WriteLine(stock.ticker);
                        writer.Flush();
                        writer.Close();
                    }
                    
                }
                else
                {
                    //might want to add more stuff later, like an error message
                }
            }
            public void sell(int amount, StockItem stock)
            {
                
                Purchase purchase = new Purchase();
                decimal price = 0;
                bool flag = false;
                price = amount * stock.GetClose();
                
                
                for (int i = 0; i < stocks.Count; i++)
                {
                    if (stock.ticker == stocks[i].stock.ticker && stock.date == stocks[i].stock.date)
                    {
                        if(stocks[i].Amount - amount > 0)
                        {
                            stocks[i].Amount = stocks[i].Amount - amount;
                            cash = cash + stock.GetClose() * amount;
                            File.Delete("userdata.txt");
                            StreamWriter writer = new StreamWriter("userdata.txt");
                            writer.WriteLine("Name");
                            writer.WriteLine(name);
                            writer.WriteLine("Initial cash");
                            writer.WriteLine(cash);
                            for (int j = 0; j < stocks.Count; j++)
                            {
                                writer.WriteLine("Avg Price");
                                writer.WriteLine(stocks[j].AverageBuyPrice);
                                writer.WriteLine("Amount");
                                writer.WriteLine(stocks[j].Amount);
                                writer.WriteLine("Date");
                                writer.WriteLine(stocks[j].stock.date);
                                writer.WriteLine("Ticker");
                                writer.WriteLine(stocks[j].stock.ticker);

                            }
                            writer.Close();
                            flag = true;
                        }
                        else if(stocks[i].Amount - amount == 0)
                        {
                            stocks.RemoveAt(i);
                            cash = cash + stock.GetClose()*amount;
                            File.Delete("userdata.txt");
                            StreamWriter writer = new StreamWriter("userdata.txt");
                            writer.WriteLine("Name");
                            writer.WriteLine(name);
                            writer.WriteLine("Initial cash");
                            writer.WriteLine(cash);
                            for (int j = 0; j < stocks.Count; j++)
                            {
                                writer.WriteLine("Avg Price");
                                writer.WriteLine(stocks[j].AverageBuyPrice);
                                writer.WriteLine("Amount");
                                writer.WriteLine(stocks[j].Amount);
                                writer.WriteLine("Date");
                                writer.WriteLine(stocks[j].stock.date);
                                writer.WriteLine("Ticker");
                                writer.WriteLine(stocks[j].stock.ticker);

                            }
                            writer.Flush();
                            flag = true;
                            writer.Close();
                        }
                        else
                        {
                            Console.WriteLine("Error");
                        }
                    }
                }
                if (flag == false)
                {
                    //reusing code. might want to reuse this later for error message ecc.
                    Console.WriteLine("Error2");
                }
                
            }
                
            
            public UserData(string nam1, decimal initCash) {
                name = nam1;
                cash = initCash;
                stocks = new List<Purchase> { };
            }
            public UserData(string nam1, decimal initCash, List<Purchase> stock1)
            {
                name = nam1;
                cash = initCash;
                stocks = stock1;
            }
        }

        public static FileStream fs = new FileStream("userdata.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        public static UserData user;
        public static List<StockItem> list;
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string name;
            decimal initCash;
            
            Purchase purchase;
            List<Purchase> stocks = new List<Purchase>() ;
            
            
            StreamWriter streamWriter = new StreamWriter(fs);
            StreamReader streamReader = new StreamReader(fs);

            

            if (string.IsNullOrEmpty(streamReader.ReadLine()))
            {
                Console.WriteLine("Looks like you are a new user");
                Console.WriteLine();
                Console.WriteLine("Please insert your name:");
                name = Console.ReadLine();
                Console.WriteLine("How much money would you like to start with?");
                initCash = Convert.ToDecimal(Console.ReadLine());
                user = new UserData(name, initCash);
                streamWriter.WriteLine("Name");
                streamWriter.WriteLine(name);
                streamWriter.WriteLine("Initial cash");
                streamWriter.WriteLine(initCash);
                streamWriter.Flush();
                Console.WriteLine("Creating new user, please wait");
                Thread.Sleep(1000);
            }
            else
            {
                
                name = streamReader.ReadLine();
                streamReader.ReadLine();
                initCash = Convert.ToDecimal(streamReader.ReadLine());
                while (streamReader.ReadLine() != null) {
                    purchase = new Purchase();
                    
                    purchase.AverageBuyPrice = Convert.ToDecimal(streamReader.ReadLine());
                    streamReader.ReadLine();
                    purchase.Amount = Convert.ToInt32(streamReader.ReadLine());
                    streamReader.ReadLine();
                    purchase.stock.date = streamReader.ReadLine();
                    streamReader.ReadLine();
                    purchase.stock.ticker = streamReader.ReadLine();
                    stocks.Add(purchase);
                }
                user = new UserData(name, initCash, stocks);
            }
            streamReader.Close();
           
            string line;
            int index, mod;
            StockItem item;
            PerformanceData item1;
            int i = 0;
            StreamReader sr = new StreamReader("ActualData.json");
            sr.ReadLine();
            sr.ReadLine();
            list = new List<StockItem>();
            item = new StockItem();

            //Parsing Json File and putting all of it in the StockItem Class
            while ((line = sr.ReadLine()) != null)
            {
                mod = i % 11;



                switch (mod)
                {
                    case 1:
                        index = line.IndexOf(':');
                        index++;
                        if (line.Length - index - 2 > 0)
                        {
                            item.date = line.Substring(index + 1, (line.Length - index - 2));
                        }

                        break;
                    case 2:
                        index = line.IndexOf(':');
                        index++;
                        if (line.Length - index - 4 > 0)
                        {
                            item.SetOpen(line.Substring(index + 2, line.Length - index - 4));
                        }

                        break;
                    case 3:
                        index = line.IndexOf(':');
                        index++;
                        if (line.Length - index - 4 > 0)
                        {
                            item.SetHigh(line.Substring(index + 2, line.Length - index - 4));
                        }
                        break;
                    case 4:
                        index = line.IndexOf(':');
                        index++;
                        if (line.Length - index - 4 > 0)
                        {
                            item.SetLow(line.Substring(index + 2, line.Length - index - 4));
                        }
                        break;
                    case 5:
                        index = line.IndexOf(':');
                        index++;
                        if (line.Length - index - 4 > 0)
                        {
                            item.SetClose(line.Substring(index + 2, line.Length - index - 4));
                        }
                        break;
                    case 6:
                        index = line.IndexOf(':');
                        index++;
                        if (line.Length - index - 4 > 0)
                        {
                            item.SetVolume(line.Substring(index + 2, line.Length - index - 4));
                        }
                        break;

                    case 9:

                        list.Add(item);
                        item = new StockItem();
                        break;


                }




                //resetting index
                index = -1;
                i++;
            }

            
            List<PerformanceData> mlList = new List<PerformanceData>();
            item1 = new PerformanceData();
            for (i = 10; i < list.Count - 10; i++)
            {

                for (int j = 0; j < 2; j++)
                {
                    mod = (j) % 2;
                    switch (mod)
                    {
                        case 0:

                            if (list[i].GetClose() > 0)
                            {
                                item1.percIncreaseB10H = ((list[i].GetClose() - list[i - 10].GetClose()) / list[i].GetClose());
                            }

                            break;
                        case 1:
                            if (list[i].GetClose() > 0)
                            {
                                item1.percIncreaseA10H = ((list[i].GetClose() - list[i + 10].GetClose()) / list[i].GetClose());
                            }
                            mlList.Add(item1);
                            item1 = new PerformanceData();
                            break;

                    }
                }

            }






            
            double[] dataX = new double[list.Count];
            double[] dataY = new double[list.Count];
            for (int j = list.Count - 1; j > -1; j--)
            {

                dataX[j] = j * 10;

                dataY[j] = Convert.ToDouble(list[j].GetClose());

            }
            Array.Reverse(dataY);
            Form1 form1 = new Form1(dataX, dataY);
            Application.Run(form1);

        }
    }


}


   
        
        
    

