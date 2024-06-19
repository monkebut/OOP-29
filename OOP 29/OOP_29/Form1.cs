using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace OOP_29
{
    public partial class Form1 : Form
    {
        const int LOCALPORT = 8001; // порт для приймання повідомлень
        const int REMOTEPORT = 8001; // порт для передавання повідомлень
        const int TTL = 20; // Time to Live для multicast
        const string HOST = "235.5.5.1"; // адреса хоста для multicast

        IPAddress groupAddress; // адреса групи для multicast
        string nickName; // ім’я користувача в чаті
        bool alive = false; // прапорець для перевірки активності підключення
        UdpClient client; // клієнт для UDP-з'єднання

        public Form1()
        {
            InitializeComponent();

            button1.Enabled = true; // увімкнення кнопки входу
            button2.Enabled = false; // вимкнення кнопки виходу
            button3.Enabled = false; // вимкнення кнопки відправки повідомлень
            richTextBox1.ReadOnly = true; // поле для повідомлень доступне лише для читання
            groupAddress = IPAddress.Parse(HOST); // парсинг адреси хоста
        }

        private void ReceiveMessages()
        {
            alive = true; // встановлення прапорця активності підключення
            try
            {
                while (alive)
                {
                    IPEndPoint remoteIp = null;
                    byte[] data = client.Receive(ref remoteIp); // приймання даних
                    string message = Encoding.Unicode.GetString(data); // декодування повідомлення

                    // додавання отриманого повідомлення в текстове поле
                    this.Invoke(new MethodInvoker(() =>
                    {
                        string time = DateTime.Now.ToShortTimeString();
                        richTextBox1.Text = time + " " + message + "\r\n" + richTextBox1.Text;
                    }));
                }
            }
            catch (ObjectDisposedException)
            {
                if (!alive) return; // вихід, якщо підключення неактивне
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // виведення повідомлення про помилку
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            nickName = textBox1.Text; // збереження введеного імені користувача
            textBox1.ReadOnly = true; // поле для введення імені стає доступним лише для читання
            try
            {
                client = new UdpClient(LOCALPORT); // ініціалізація UDP-клієнта з локальним портом
                client.JoinMulticastGroup(groupAddress, TTL); // під'єднання до групового розсилання

                // задача на приймання повідомлень
                Task receiveTask = new Task(ReceiveMessages);
                receiveTask.Start(); // запуск задачі

                // перше повідомлення про вхід нового користувача
                string message = nickName + " увійшов до чату";
                byte[] data = Encoding.Unicode.GetBytes(message); // кодування повідомлення
                client.Send(data, data.Length, HOST, REMOTEPORT); // відправлення повідомлення
                button1.Enabled = false; // вимкнення кнопки входу
                button2.Enabled = true; // увімкнення кнопки виходу
                button3.Enabled = true; // увімкнення кнопки відправки повідомлень
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // виведення повідомлення про помилку
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string message = String.Format("{0}: {1}", nickName, textBox2.Text); // форматування повідомлення
                byte[] data = Encoding.Unicode.GetBytes(message); // кодування повідомлення
                client.Send(data, data.Length, HOST, REMOTEPORT); // відправлення повідомлення
                textBox2.Clear(); // очищення поля введення повідомлення
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // виведення повідомлення про помилку
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string message = nickName + " покидає чат"; // повідомлення про вихід користувача
            byte[] data = Encoding.Unicode.GetBytes(message); // кодування повідомлення
            client.Send(data, data.Length, HOST, REMOTEPORT); // відправлення повідомлення
            client.DropMulticastGroup(groupAddress); // вихід з групового розсилання
            alive = false; // встановлення прапорця активності підключення в false
            client.Close(); // закриття клієнта
            button1.Enabled = true; // увімкнення кнопки входу
            button2.Enabled = false; // вимкнення кнопки виходу
            button3.Enabled = false; // вимкнення кнопки відправки повідомлень
            textBox1.ReadOnly = false; // поле для введення імені стає доступним для редагування
            textBox1.Clear(); // очищення поля введення імені
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (alive)
            {
                string message = nickName + " покидає чат"; // повідомлення про вихід користувача
                byte[] data = Encoding.Unicode.GetBytes(message); // кодування повідомлення
                client.Send(data, data.Length, HOST, REMOTEPORT); // відправлення повідомлення
                client.DropMulticastGroup(groupAddress); // вихід з групового розсилання
                alive = false; // встановлення прапорця активності підключення в false
                client.Close(); // закриття клієнта
                button1.Enabled = true; // увімкнення кнопки входу
                button2.Enabled = false; // вимкнення кнопки виходу
                button3.Enabled = false; // вимкнення кнопки відправки повідомлень
                richTextBox1.ReadOnly = false; // поле для повідомлень стає доступним для редагування
                richTextBox1.Clear(); // очищення поля для повідомлень
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog(); // ініціалізація діалогового вікна вибору кольору
            dialog.ShowDialog(); // показ діалогового вікна
            richTextBox1.ForeColor = dialog.Color; // встановлення обраного кольору для тексту
        }
    }
}
