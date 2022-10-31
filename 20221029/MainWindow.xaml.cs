using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using CsvHelper;
using System.Globalization;
using RadioButton = System.Windows.Controls.RadioButton;
using CheckBox = System.Windows.Controls.CheckBox;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;
using Binding = System.Windows.Data.Binding;

namespace _20221029
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Dictionary<string, int> foods = new Dictionary<string, int>();
        List<Drink> drinks = new List<Drink>();
        List<OrderItem> order = new List<OrderItem>();
        string takeout;
        public MainWindow()
        {
            InitializeComponent();
            AddNewDrink(drinks);
            DisplayDrink(drinks);
        }

        private void DisplayDrink(List<Drink> myDrink)
        {
            foreach(Drink d in drinks)
            {
                StackPanel sp = new StackPanel();
                CheckBox cb = new CheckBox();
                Slider sl = new Slider();
                Label lb = new Label();

                sp.Orientation = Orientation.Horizontal;

                cb.Content = d.Name + d.Size + d.Price;
                cb.Width = 150;
                cb.Margin = new Thickness(10);

                sl.Width = 100;
                sl.Minimum = 0;
                sl.Maximum = 20;
                sl.IsSnapToTickEnabled = true;
                sl.AutoToolTipPlacement = System.Windows.Controls.Primitives.AutoToolTipPlacement.BottomRight;

                lb.Width = 50;
                lb.Content = 0;

                sp.Children.Add(cb);
                sp.Children.Add(sl);
                sp.Children.Add(lb);

                Binding myBinding = new Binding("Value");
                myBinding.Source = sl;
                lb.SetBinding(ContentProperty,myBinding);

                StackPanel_DrinkMenu.Children.Add(sp);
            }
        }

        private void AddNewDrink(List<Drink> myDrink)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Filter = "CSV檔案|*.csv|文字檔案|*.txt|所有檔案|*.*";
            if (fileDialog.ShowDialog() == true)
            {
                string path = fileDialog.FileName;
                StreamReader sr = new StreamReader(path, Encoding.Default);
                CsvReader csv = new CsvReader(sr , CultureInfo.InvariantCulture);

                csv.Read();
                csv.ReadHeader();
                while (csv.Read()==true)
                {
                    Drink d = new Drink() { Name = csv.GetField("Name"), Size = csv.GetField("Size"), Price = csv.GetField<int>("Price") };
                    myDrink.Add(d);
                }
            }
        }

        private void DisplayOrderDetail(List<OrderItem> myOrder)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "文字檔案|.txt|所有檔案|*.*";
            sfd.FileName = "保存";
            sfd.DefaultExt = "txt";
            sfd.AddExtension = true;
            DPT.Inlines.Clear();
            DPT.Inlines.Add(new Run("您所訂購的商品："));
            DPT.Inlines.Add(new Bold(new Run($"{takeout}，" )));
            DPT.Inlines.Add(new Run("訂購明細如下:\n"));
            int total = 0,sellPrice=0;
            string message = "";
            int i = 1;
            foreach(OrderItem item in order)
            {
                total += item.SubTotle;
                Drink drinkItem = drinks[item.Index];
                DPT.Inlines.Add(new Run($"訂購品項{i}：{drinkItem.Name}{drinkItem.Size}X{item.Quantity}杯，每杯{drinkItem.Price}元，小計{item.SubTotle}元。\n"));
                i++;
            }
            if(total >= 500)
            {
                sellPrice = Convert.ToInt32(Math.Round(Convert.ToDouble(total) * 0.8));
                message = "訂購總價超過500元，打8折";
            }
            else if (total >= 300)
            {
                sellPrice = Convert.ToInt32(Math.Round(Convert.ToDouble(total) * 0.85));
                message = "訂購總價超過300元，打85折";
            }
            else if (total >= 200)
            {
                sellPrice = Convert.ToInt32(Math.Round(Convert.ToDouble(total) * 0.9));
                message = "訂購總價超過200元，打9折";
            }
            else
            {
                sellPrice = total;
                message = "訂購總價未滿200元，不打折";
            }
            DPT.Inlines.Add(new Italic(new Run($"總價{total}元，{message}，售價{sellPrice}元")));
            string savestr = DPT.Text;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    sw.WriteLineAsync($"{savestr}");
                }
            }


        }

        private void PlaceOrder(List<OrderItem> myOrder)
        {
            myOrder.Clear();
            for(int i = 0; i < StackPanel_DrinkMenu.Children.Count; i++)
            {
                StackPanel sp = StackPanel_DrinkMenu.Children[i] as StackPanel;
                CheckBox cb = sp.Children[0] as CheckBox;
                Slider sl = sp.Children[1] as Slider;
                int quantity = Convert.ToInt32(sl.Value);
                if(cb.IsChecked == true && quantity!= 0)
                {
                    int price = drinks[i].Price;
                    int subtotle = price * quantity;
                    myOrder.Add(new OrderItem() { Index = i, Quantity = quantity, SubTotle = subtotle });
                }
            }
        }

        private void Radio_checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
           if (rb.IsChecked == true) takeout = rb.Content.ToString();
           
        }

        private void OrderBotton_Click(object sender, RoutedEventArgs e)
        {
            

            PlaceOrder(order);

            DisplayOrderDetail(order);
        }
    }
}
