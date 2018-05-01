using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using WindowsInput.Native;
using WindowsInput;

namespace PoeStalker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string charName = "";
        public List<RootObject> rootObjects = new List<RootObject>();
        private Stash charStash = new Stash();
        Dictionary<string, List<StashItemsFiltered>> itemsPerType = new Dictionary<string, List<StashItemsFiltered>>();
        Dictionary<string, string> itemData = new Dictionary<string, string>();
        Mouse mouse = new Mouse();
        Keyboard kb = new Keyboard();

        private void Form1_Load(object sender, EventArgs e)
        {
            itemData = ItemData.FillOutItemData();
            RefreshStash();
        }

        private void RefreshStash()
        {
            //try
            {
                itemsPerType.Clear();
                itemsPerType = DataConversion.SortItemsToCategories(itemData, WebTools.GetStashItemsFromWeb(textBoxAcc.Text,textBoxStashNo.Text));
                GridFormating.SetGridRowsColumns(dataGridViewStash, true);
                GridFormating.SetGridColorsPerItem(itemsPerType, dataGridViewStash, checkBox1.Checked);
                GridFormating.MakeItemsSummary(itemsPerType, dataGridViewItems);
            }
            //catch (Exception ex) { MessageBox.Show("Something went wrong :("+Environment.NewLine+ex.Message+Environment.NewLine+ex.StackTrace); }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RefreshStash();
           
        }

        private StashItemsFiltered copyStashItem(StashItemsFiltered inputItem)
        {
            StashItemsFiltered result = new StashItemsFiltered();

                result.category = inputItem.category;
                result.subCategory = inputItem.subCategory;
                result.icon = inputItem.icon;
                result.id = inputItem.id;
                result.itlvl = inputItem.itlvl;
                result.typeName = inputItem.typeName;
                result.x = inputItem.x;
                result.y = inputItem.y;
                result.w = inputItem.w;
                result.h = inputItem.h;
                result.quadLayout = inputItem.quadLayout;
                result.picked = inputItem.picked;

            return result;
        }
        public class StashItemsFiltered
        {
            public string category { get; set; }
            public string subCategory { get; set; }
            public string icon { get; set; }
            public string id { get; set; }
            public Int32 itlvl { get; set; }
            public string typeName { get; set; }
            public Int32 x { get; set; }
            public Int32 y { get; set; }
            public Int32 w { get; set; }
            public Int32 h { get; set; }
            public bool quadLayout { get; set; }
            public bool picked { get; set; }
        }

        public class PrivateStash
        {   
            public bool quadLayout { get; set; }
            public List<object> items { get; set; }
        }

        public class Stash
        {
            public string accountName { get; set; }
            public string lastCharacterName { get; set; }
            public string id { get; set; }
            public string stash { get; set; }
            public string stashType { get; set; }
            public List<object> items { get; set; }
            public bool @public { get; set; }
        }
        
        public class RootObject
        {
            public string next_change_id { get; set; }
            public List<Stash> stashes { get; set; }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            charName = textBoxAcc.Text;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                PublicStash.fillRootObjects(charStash, textBoxAcc.Text, PublicStash.GetLatestId());
            }).Start();

            
        }
        
        private void button3_Click(object sender, EventArgs e)
        {
            List<StashItemsFiltered> set = null; 
            int setsNo = SetCalculation.HowManySets(itemsPerType);
            if (setsNo > 0)
            {
                Dictionary<string, List<StashItemsFiltered>> itemsPerTypeCopy = new Dictionary<string, List<StashItemsFiltered>>();
                foreach (var typeEntry in itemsPerType)
                {
                    List<StashItemsFiltered> newList = new List<StashItemsFiltered>();
                    foreach (StashItemsFiltered item in typeEntry.Value)
                    {
                        StashItemsFiltered newItem = copyStashItem(item);
                        newList.Add(newItem);
                    }
                    itemsPerTypeCopy.Add(typeEntry.Key, newList);
                }

                set = SetCalculation.MakeSets(itemsPerTypeCopy);

                kb.SendDown(Keys.ControlKey);
                {
                    foreach (StashItemsFiltered item in set)
                    {
                        Tuple<int, int> click = Coordinates.GetClickCoordinates(item.x, item.y, item.quadLayout);
                        Thread.Sleep(50);
                        mouse.MouseLeftClick(click.Item1, click.Item2);
                    }
                }
                kb.SendUp(Keys.ControlKey);

                Thread.Sleep(2000);
                RefreshStash();
            }
        }

        private void dataGridView1_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {

        }
    }
}

