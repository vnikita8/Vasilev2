using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Vasilev2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML files(*.xml)|*.xml";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            string filepath = openFileDialog1.FileName;// получаем путь к файлу
            Log log = new Log(XMLtoLog.XmlDoc(filepath)); //Создаём лог
            TreeUpd(log); //Выводим на форму
        }

        public void TreeUpd(Log log) //Вывод на форму
        {
            List < Event > eventList = log.Events;
            if (eventList != null)
            {
                foreach (Event oneEvent in eventList)
                {
                    TreeNode parentNode = new TreeNode($"{oneEvent.Date.Trim()} : {oneEvent.Result}");
                    TreeNode ip = new TreeNode("ip-from");
                    TreeNode ip_values = new TreeNode(oneEvent.Ip_from);
                    ip.Nodes.Add(ip_values);
                    parentNode.Nodes.Add(ip);

                    TreeNode method = new TreeNode("method");
                    TreeNode method_values = new TreeNode(oneEvent.Method);
                    method.Nodes.Add(method_values);
                    parentNode.Nodes.Add(method);

                    TreeNode url = new TreeNode("url-to");
                    TreeNode url_values = new TreeNode(oneEvent.Url_to);
                    url.Nodes.Add(url_values);
                    parentNode.Nodes.Add(url);

                    TreeNode response = new TreeNode("response");
                    TreeNode response_values = new TreeNode(oneEvent.Response);
                    response.Nodes.Add(response_values);
                    parentNode.Nodes.Add(response);
                    treeView1.Nodes.Add(parentNode);
                }
            }
            else 
            {
                MessageBox.Show("Ошибка, неверный формат файла, либо данные повреждены", "Ошибка");
            }
            
        }
    }

    static class XMLtoLog 
    {
        public static XDocument XmlDoc(string filepath) //Получить документ из ссылки
        {
            XDocument xdoc = XDocument.Load(filepath);   
            return xdoc;
        }

        public static Event ParseElementToEvent(XElement element) //Превратить XElement в Event
        {
            string[] element_values = new string[6];
            element_values[0] = element.Attribute("date").Value;
            element_values[1] = element.Attribute("result").Value;
            int i = 2;
            foreach (XElement el in element.Elements())
            {
                element_values[i] = el.Value;
                i++;
            }
            return AddEvent(element_values);
        }

        public static Event AddEvent (string[] values) //Побочная, осталась с другого варианта решения, но оставил
        {
            Event OneEvent = new Event();
            for (int i = 2; i< values.Length; i++)
            {
                if (values[i] == null || values[i].Replace(" ", "") == "")
                {
                    values[i] = "Error, no data";
                }
            }
            OneEvent.Date = values[0];
            OneEvent.Result = values[1];
            OneEvent.Ip_from = values[2];
            OneEvent.Method = values[3];
            OneEvent.Url_to = values[4];
            OneEvent.Response = values[5];
            if (OneEvent.Date.Replace(" ", "") == ""|| OneEvent.Result.Replace(" ", "") == "")
            {
                OneEvent.EmptyGo();
            }
            return  OneEvent;
        }
    }

    public abstract class PreLog //Абстракция, сложно придумать другую, учитывая что классы в целом разные
    {
        public abstract List<Event> EventList(XDocument xdoc);
    }

    public class Log : PreLog
    {
        public List<Event> Events;
        public Log(XDocument xdoc) //Получаем список Event
        {
            Events = EventList(xdoc);
        }

        public override List<Event> EventList(XDocument xdoc) //Методы статического класса
        {
            List<Event> result_list = new List<Event>();
            if  (xdoc.Element("log") == null)
            {
                return null;
            }
            else
            {
                foreach (XElement element in xdoc.Element("log").Elements("event"))
                {
                    Event One_event = XMLtoLog.ParseElementToEvent(element);
                    result_list.Add(One_event);
                }
                return result_list;
            }
            
        }
    }

    public class Event : EmptyEvent //Чисто поля и метод (из-за интерфейса, чтобы хоть как-то плюс минус юзабельный был)
    {
        public string Date { get; set; }
        public string Result { get; set; }
        public string Ip_from { get; set;}
        public string Method { get; set; }
        public string Url_to { get; set; }
        public string Response { get; set; }
        public Event(){}
        public void EmptyGo()
        {
            if (Date == null || Date.Replace(" ", "") == "")
            {
                Date = EmptyDate();
            }
            if (Result == null || Result.Replace(" ", "") == "")
            {
                Result = EmptyDate();
            }
        }
        public string EmptyDate(string empty = "Empty")
        {
            return empty;
        }
        public string EmptyResult(string empty = "Empty")
        {
            return empty;
        }
    }

    public interface EmptyEvent
    {
        string EmptyDate(string empty = "Empty");
        string EmptyResult(string empty = "Empty");
    }
}
