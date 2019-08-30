using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Newtonsoft.Json;

namespace EjemploSAP1.rabbitMq
{
    class Producer : IDisposable
    {

        protected IModel Model;
        protected IConnection Connection;
        protected string QueueName;
        protected ConnectionFactory connectionFactory;

        public Producer()
        {

            string json = Form1.ReadAllText("station.conf");

            Program.myConf = JsonConvert.DeserializeObject<Conf>(json);

            if (Program.myConf != null)
            {

                Console.WriteLine("no nulo");
                QueueName = Program.myConf.queueName;
                Console.WriteLine("Program.myConf.queueName " + Program.myConf.queueName);
                connectionFactory = new ConnectionFactory();
                connectionFactory.UserName = Program.myConf.userMsg;
                Console.WriteLine("Program.myConf.userMsg " + Program.myConf.userMsg);
                connectionFactory.Password = Program.myConf.passMsg;
                Console.WriteLine("Program.myConf.passMsg " + Program.myConf.passMsg);


                connectionFactory.Port = 5672;
                connectionFactory.VirtualHost = "/";

                connectionFactory.HostName = Program.myConf.serverMsg;

                Connection = connectionFactory.CreateConnection();

                Model = Connection.CreateModel();
                bool durable = true;
                Model.QueueDeclare(QueueName, durable, false, false, null);
                
            }
        }

        public void SendMessage(byte[] message)
        {
            IBasicProperties basicProperties = Model.CreateBasicProperties();

            
            basicProperties.SetPersistent(true);
            Model.BasicPublish("", QueueName, basicProperties, message);
        }


        public void SendMessageTTL(byte[] message)
        {
            IBasicProperties basicProperties = Model.CreateBasicProperties();
            basicProperties.Expiration = "60000";
            basicProperties.SetPersistent(true);
            Model.BasicPublish("", QueueName, basicProperties, message);
        }

        
        public void Dispose()
        {
            if (Connection != null)
                Connection.Close();

            if (Model != null)
                Model.Abort();

            if (connectionFactory != null)
                connectionFactory = null;
        }
        
    }
}
